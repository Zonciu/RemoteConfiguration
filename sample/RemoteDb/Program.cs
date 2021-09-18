using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RemoteConfiguration;
using RemoteDb.Basic;
using Timer = System.Timers.Timer;

namespace RemoteDb
{
    public class EfCoreRemoteConfigurationAccessor : IRemoteConfigurationAccessor
    {
        private readonly MyDb _db;

        public EfCoreRemoteConfigurationAccessor(MyDb db)
        {
            _db = db;
        }

        public async Task<string?> GetJsonString()
        {
            var options = await _db.Options.ToDictionaryAsync(e => e.Key, e => e.Value);
            var json = JsonSerializer.Serialize(
                new
                {
                    MyOptions = options
                });
            return json;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var db = new MyDb();

            db.Options.AddRange(
                new() { Key = "A", Value = "abcdefg" },
                new() { Key = "B", Value = "123456" },
                new() { Key = "C", Value = DateTime.Now.ToString(CultureInfo.InvariantCulture) });
            db.SaveChanges();
            Console.WriteLine(JsonSerializer.Serialize(db.Options.ToList()));
            var optionsUpdateTimer = new Timer();
            optionsUpdateTimer.Interval = 1000;
            optionsUpdateTimer.Elapsed += (_, _) =>
            {
                db.Options.RemoveRange(db.Options.ToList());
                db.Options.AddRange(
                    new() { Key = "A", Value = Guid.NewGuid().ToString() },
                    new() { Key = "B", Value = RandomNumberGenerator.GetInt32(0, 1000).ToString() },
                    new() { Key = "C", Value = DateTime.Now.ToString(CultureInfo.InvariantCulture) });
                db.SaveChanges();
            };
            optionsUpdateTimer.Start();

            var accessor = new EfCoreRemoteConfigurationAccessor(db);
            var config = new ConfigurationBuilder().AddRemote(accessor, false, 5).Build();

            var serviceProvider = new ServiceCollection().Configure<MyOptions>(config.GetSection("MyOptions")).BuildServiceProvider();
            var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<MyOptions>>();

            Console.WriteLine(JsonSerializer.Serialize(monitor.CurrentValue));
            monitor.OnChange(myOptions => { Console.WriteLine(JsonSerializer.Serialize(myOptions)); });

            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
            Console.WriteLine("Exit");
            optionsUpdateTimer.Stop();
        }
    }
}