using System;
using System.Net.Http;
using System.Threading;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace RemoteConfiguration.Tests
{
    public class UnitTest : IDisposable
    {
        private readonly IHost _host;

        public UnitTest()
        {
            _host = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(
                             b => b.UseSetting("urls", "http://127.0.0.1:56789")
                                   .ConfigureServices(s => s.AddSingleton<TestData>().AddControllers())
                                   .Configure(ab => ab.UseRouting().UseEndpoints(r => r.MapControllers())))
                        .Build();
            _host.Start();
        }

        [Fact]
        public void NormalTest()
        {
            var remoteOptions = _host.Services.GetRequiredService<TestData>().TestBasic;
            var config = new ConfigurationBuilder().AddRemote(async () => await new HttpClient().GetStringAsync("http://127.0.0.1:56789/test")).Build();
            var serviceProvider = new ServiceCollection().Configure<TestOptions>(config.GetSection(nameof(TestData.TestBasic))).BuildServiceProvider();
            var localOptions = serviceProvider.GetRequiredService<IOptions<TestOptions>>().Value;
            localOptions.A.Should().Be(remoteOptions.A);
            localOptions.B.Should().Be(remoteOptions.B);
            localOptions.C.Should().Be(remoteOptions.C);
        }

        [Fact]
        public void IntervalTest()
        {
            var config = new ConfigurationBuilder().AddRemote(async () => await new HttpClient().GetStringAsync("http://127.0.0.1:56789/test/interval"), true, 3).Build();
            var serviceProvider = new ServiceCollection().Configure<TestOptions>(config.GetSection(nameof(TestData.TestInterval))).BuildServiceProvider();
            var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();
            {
                var remoteOptions = _host.Services.GetRequiredService<TestData>().TestInterval;
                var localOptions = monitor.CurrentValue;
                localOptions.A.Should().Be(remoteOptions.A);
                localOptions.B.Should().Be(remoteOptions.B);
                localOptions.C.Should().Be(remoteOptions.C);
            }

            Exception? exception = null;
            var sema = new Semaphore(1, 1);
            sema.WaitOne();
            monitor.OnChange(
                newOptions =>
                {
                    try
                    {
                        var remoteOptions = _host.Services.GetRequiredService<TestData>().TestInterval;
                        newOptions.A.Should().Be(remoteOptions.A);
                        newOptions.B.Should().Be(remoteOptions.B);
                        newOptions.C.Should().Be(remoteOptions.C);
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                    finally
                    {
                        sema.Release();
                    }
                });

            sema.WaitOne();
            if (exception != null)
            {
                throw exception;
            }
        }

        [Fact]
        public void ReloadTest()
        {
            var reloadToken = new ConfigurationReloadToken();
            var config = new ConfigurationBuilder().AddRemote(() => new HttpClient().GetStringAsync("http://127.0.0.1:56789/test/interval"), true, reloadToken).Build();
            var serviceProvider = new ServiceCollection().Configure<TestOptions>(config.GetSection(nameof(TestData.TestInterval))).BuildServiceProvider();
            var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();
            {
                var remoteOptions = _host.Services.GetRequiredService<TestData>().TestInterval;
                var localOptions = monitor.CurrentValue;
                localOptions.A.Should().Be(remoteOptions.A);
                localOptions.B.Should().Be(remoteOptions.B);
                localOptions.C.Should().Be(remoteOptions.C);
            }
            Exception? exception = null;
            var sema = new Semaphore(1, 1);

            monitor.OnChange(
                newOptions =>
                {
                    try
                    {
                        var remoteOptions = _host.Services.GetRequiredService<TestData>().TestInterval;
                        newOptions.A.Should().Be(remoteOptions.A);
                        newOptions.B.Should().Be(remoteOptions.B);
                        newOptions.C.Should().Be(remoteOptions.C);
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                    finally
                    {
                        sema.Release();
                    }
                });
            sema.WaitOne();
            reloadToken.OnReload();
            sema.WaitOne();
            if (exception != null)
            {
                throw exception;
            }
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}