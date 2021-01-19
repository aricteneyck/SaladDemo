using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SaladDemo.Controllers {
  [Route("[controller]")]
  [ApiController]
  public class GamesController : ControllerBase {
    // GET /games
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameEntry>>> games(string q, string sort) {

      if(string.IsNullOrEmpty(q)) {
        Response.StatusCode = 400;
        return Content("Invalid Query Parameter");
        
      }

      List<string> LegalSorts = (new string[] { "name", "released", "added", "created", "updated", "rating", "metacritic",
      "-name", "-released", "-added", "-created", "-updated", "-rating", "-metacritic"}).ToList();

      
      if(!string.IsNullOrEmpty(sort)) {
        if(!LegalSorts.Contains(sort)) {
          Response.StatusCode = 400;
          Response.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Invalid Sort Parameter"));
          return null;
        }
      }




      using (var httpClient = new HttpClient()) {
        using (var response = await httpClient.GetAsync($"http://api.rawg.io/api/games?search={q}&ordering={sort}?key=REDACTED")) {
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

    // POST api/values
    [HttpPost]
    public void Post([FromBody] string value) {
    }

    // PUT api/values/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value) {
    }

    // DELETE api/values/5
    [HttpDelete("{id}")]
    public void Delete(int id) {
    }
  }
}
