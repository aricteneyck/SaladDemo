using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaladDemo {
  public class GameEntry {
    public int gameId { get; set; }
    public string name { get; set; }
    public int added { get; set; }
    public int metacritic { get; set; }
    public decimal rating { get; set; }
    public DateTime released { get; set; }
    public DateTime updated { get; set; }
  }
}
