using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SaladDemo.Controllers {
  [Route("")]
  [ApiController]
  public class GamesController : ControllerBase {
    // Default page so that I don't have to look at an error page while debugging.
    [HttpGet]
    [Route("")]
    public ActionResult DefaultGet() {
      if (Startup.APIKey == "REDACTED") {
        return Content("You need to enter a valid API Key into appsettings.json");
      }
      return Content("RAWG API Demo Tool");
    }

    private string APIKey {
      get {
        if (Startup.APIKey == "REDACTED") {
          throw new Exception("API Key not entered in appsettings.json.  Cannot continue.");
        }

        return Startup.APIKey;
      }
    }

    [HttpGet]
    [Route("games")]
    public async Task<ActionResult<IEnumerable<GameEntry>>> games(string q, string sort) {

      if(string.IsNullOrEmpty(q)) {
        Response.StatusCode = 400;
        return Content("Invalid Query Parameter");
        
      }

      List<string> LegalSorts = (new string[] { "name", "released", "added", "created", "updated", "rating", "metacritic",
      "-name", "-released", "-added", "-created", "-updated", "-rating", "-metacritic"}).ToList();


      if (!string.IsNullOrEmpty(sort)) {
        if (!LegalSorts.Contains(sort.ToLower())) {
          Response.StatusCode = 400;
          Response.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Invalid Sort Parameter"));
          return null;
        }
      }

      using (var httpClient = new HttpClient()) {
        using (var response = await httpClient.GetAsync($"http://api.rawg.io/api/games?search={q}&ordering={sort}&key={APIKey}")) {
          string apiResponse = await response.Content.ReadAsStringAsync();
          Newtonsoft.Json.Linq.JObject list = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(apiResponse);
          var gamelist = list["results"];

          var sendback = new List<GameEntry>();
          foreach (var g in gamelist) {
            try {
              sendback.Add(new GameEntry {
                added = int.Parse(g["added"].ToString()),
                gameId = int.Parse(g["id"].ToString()),
                metacritic = int.Parse(g["metacritic"].ToString()),
                name = g["name"].ToString(),
                rating = decimal.Parse(g["rating"].ToString()),
                released = DateTime.Parse(g["released"].ToString()),
                updated = DateTime.Parse(g["updated"].ToString())
              });
            }
            catch { }
          }
          return sendback;
        }
      }
    }

    // GET api/values/5
    [HttpGet("{id}")]
    public ActionResult<string> Get(int id) {
      return "value";
    }

    // POST users
    [HttpPost]
    [Route("users")]
    public ActionResult<UserEntry> users() {
      var u = Program.Repository.AddUser();
      Response.StatusCode = 201;
      return u;
    }

    [HttpGet]
    [Route("users/{userId}")]
    public ActionResult<UserEntry> users(int userId) {
      var u = Program.Repository.GetUser(userId);
      if (u == null) {
        Response.StatusCode = 404;
        return null;
      }
      return u;
    }


    [HttpPost]
    [Route("/users/{userId}/games")]
    public async void AddGame(int userId, [FromBody] GameID gameId) {
      var u = Program.Repository.GetUser(userId);
      if (u == null) {
        Response.StatusCode = 404;
        return;
      }

      if (u.games.Any(g => g.gameId == gameId.gameId)) {
        Response.StatusCode = 409;
        return;
      }

      // Check if the game exists
      using (var httpClient = new HttpClient()) {
        using (var response = await httpClient.GetAsync($"http://api.rawg.io/api/games/{gameId.gameId}?key={APIKey}")) {
          if(!response.IsSuccessStatusCode) {
            Response.StatusCode = 400;
            return;
          }
          string apiResponse = await response.Content.ReadAsStringAsync();
          Newtonsoft.Json.Linq.JObject list = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(apiResponse);
          var g = list;
          GameEntry game;
          try {
            game = new GameEntry {
              added = int.Parse(g["added"].ToString()),
              gameId = int.Parse(g["id"].ToString()),
              metacritic = int.Parse(g["metacritic"].ToString()),
              name = g["name"].ToString(),
              rating = decimal.Parse(g["rating"].ToString()),
              released = DateTime.Parse(g["released"].ToString()),
              updated = DateTime.Parse(g["updated"].ToString())
            };
          }
          catch {
            // Some kind of parsing problem or something
            // TODO: Log the error
            Response.StatusCode = 400;
            return;
          }

          u.AddGame(game);
          Response.StatusCode = 204;
          return;
        }
      }
    }

    [HttpDelete]
    [Route("/users/{userId}/games/{gameId}")]
    public void DeleteGame(int userId, int gameId) {
      var u = Program.Repository.GetUser(userId);
      if (u == null) {
        // Bad Request - no such user
        Response.StatusCode = 400;
        return;
      }
      if(u.DeleteGame(gameId)) {
        // Success
        Response.StatusCode = 204;
        return;
      }
      // User did not own the game
      Response.StatusCode = 404;
      return;
    }

    [HttpPost]
    [Route("/users/{userId}/comparison")]
    public ActionResult<ComparisonResult> Compare(int userId, [FromBody] ComparisonInfo CompInfo) {
      var user1 = Program.Repository.GetUser(userId);
      if (user1 == null) {
        Response.StatusCode = 404;
        return null;
      }
      var user2 = Program.Repository.GetUser(CompInfo.otherUserId);
      if(user2 == null) {
        Response.StatusCode = 400;
        return null;
      }

      IEnumerable<GameEntry> result;

      switch(CompInfo.comparison.ToLower()) {
        case "union":
          result = user1.games.Union(user2.games, new GameEntryComparer());
          break;
        case "intersection":
          result = user1.games.Intersect(user2.games, new GameEntryComparer());
          break;
        case "difference":
          // Make a list of all of User2's games and then subtract all of User1's
          List<GameEntry> diff = user2.games.ToList();
          foreach(var g in user1.games) {
            // There should never be more than one, but this works correctly if there is.
            diff.RemoveAll(d => d.gameId == g.gameId);
          }
          result = diff;
          break;
        default:
          Response.StatusCode = 400;
          return null;
      }

      ComparisonResult cr = new ComparisonResult {
        comparison = CompInfo.comparison,
        games = result.ToArray(),
        otherUserId = CompInfo.otherUserId,
        userId = userId
      };

      return cr;


    }
  }

  // These classes are used to represent the JSON input and output to some of the functions above
  public class GameID {
    public int gameId { get; set; }
  }

  public class ComparisonInfo {
    public int otherUserId { get; set; }
    public string comparison { get; set; }
  }

  public class ComparisonResult {
    public int userId { get; set; }
    public int otherUserId { get; set; }
    public string comparison { get; set; }
    public GameEntry[] games { get; set; }
  }

}
