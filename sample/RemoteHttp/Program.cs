using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RemoteConfiguration;
using RemoteConfiguration.Tests;

namespace Testbed
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var host = Host.CreateDefaultBuilder()
                                 .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning))
                                 .ConfigureWebHostDefaults(
                                      b =>
                                          b.UseSetting("urls", "http://127.0.0.1:56789")
                                           .ConfigureServices(
                                                (ctx, s) =>
                                                {
                                                    s.AddSingleton<TestData>();
                                                    s.AddControllers();
                                                })
                                           .Configure(ab => ab.UseRouting().UseEndpoints(r => r.MapControllers())))
                                 .Build();
            host.Start();

            var http = new HttpClient();
            var config = new ConfigurationBuilder().SetRemoteLoadExceptionHandler(
                                                        context =>
                                                        {
                                                            Console.WriteLine(context.Exception.Message);
                                                            context.Ignore = false;
                                                        })
                                                   .AddRemote(async () => await http.GetStringAsync("http://127.0.0.1:56789/test/interval"), false, 5).Build();
            var serviceProvider = new ServiceCollection().Configure<TestOptions>(config).BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();
            Console.WriteLine(JsonSerializer.Serialize(options.CurrentValue));

            options.OnChange(testOptions => { Console.WriteLine(JsonSerializer.Serialize(testOptions)); });
            await host.WaitForShutdownAsync();
            await host.StopAsync();
        }
    }
}