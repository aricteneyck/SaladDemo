using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SaladDemo {

  // From https://stackoverflow.com/questions/54224810/posting-date-to-asp-net-core-api
  public class ShortDateConverter : IsoDateTimeConverter {
    public ShortDateConverter() {
      base.DateTimeFormat = "yyyy-MM-dd";
    }
  }

  public class GameEntry {
    public int gameId { get; set; }
    public string name { get; set; }
    public int added { get; set; }
    public int metacritic { get; set; }
    public decimal rating { get; set; }
    [JsonConverter(typeof(ShortDateConverter))]
    public DateTime released { get; set; }
    public DateTime updated { get; set; }
  }

  class GameEntryComparer : IEqualityComparer<GameEntry> {
    public bool Equals(GameEntry x, GameEntry y) {
      return x.gameId == y.gameId;
    }

    public int GetHashCode(GameEntry obj) {
      return obj.gameId.GetHashCode();
    }
  }


}
