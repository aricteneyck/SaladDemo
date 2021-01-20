using System.Collections.Generic;
using System.Linq;

namespace SaladDemo {
  public class UserEntry {
    public int userId { get; set; }
    public GameEntry[] games {
      get {
        if (userGames == null) {
          return new GameEntry[0];
        }
        return userGames.OrderBy(ug => ug.gameId).ToArray();
      }
    }
    /// <summary>
    /// Add a game to the user's list
    /// </summary>
    /// <param name="g">ID of the game to add</param>
    public void AddGame(GameEntry g) {
      if (userGames == null) {
        userGames = new List<GameEntry>();
      }
      userGames.Add(g);
    }

    /// <summary>
    /// Remove a game from the user's list
    /// </summary>
    /// <param name="gameId">ID of the game to remove</param>
    /// <returns>true if the game was removed, false if the user did not own the game</returns>
    public bool DeleteGame(int gameId) {
      var g = userGames.Where(ug => ug.gameId == gameId).FirstOrDefault();
      if (g == null) {
        return false;
      }
      userGames.Remove(g);
      return true;
    }

    private List<GameEntry> userGames;
  }
}
