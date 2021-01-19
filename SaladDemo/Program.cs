using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SaladDemo {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    private static object _repo;

    public static object Repository {
      get {
        if(_repo == null) {
          _repo = new object();
        }
        return _repo;
      }
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();
  }
}
