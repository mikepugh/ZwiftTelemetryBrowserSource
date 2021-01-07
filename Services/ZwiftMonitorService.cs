using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System;
using ZwiftTelemetryBrowserSource.Models;
using Newtonsoft.Json;

namespace ZwiftTelemetryBrowserSource.Services
{
    public class ZwiftMonitorService : IHostedService
    {
        public ZwiftMonitorService(ILogger<ZwiftMonitorService> logger, 
            IHostApplicationLifetime applicationLifetime,
            ZwiftPacketMonitor.Monitor zwiftPacketMonitor,
            IConfiguration config,
            INotificationsService notificationsService) {

            Config = config;
            Logger = logger;
            ApplicationLifetime = applicationLifetime;
            ZwiftPacketMonitor = zwiftPacketMonitor;
            NotificationsService = notificationsService;
        }

        private INotificationsService NotificationsService {get;}
        private IConfiguration Config {get;}
        private ILogger<ZwiftMonitorService> Logger {get;}
        private IHostApplicationLifetime ApplicationLifetime {get;}
        private ZwiftPacketMonitor.Monitor ZwiftPacketMonitor {get;}
        private ZwiftPacketMonitor.PlayerState playerState = null;

        public Task StartAsync(CancellationToken cancellationToken) {
            ApplicationLifetime.ApplicationStarted.Register(OnStarted);
            ApplicationLifetime.ApplicationStopping.Register(OnStopping);
            ApplicationLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        private void OnStarted() {
            Logger.LogInformation("OnStarted has been called.");

            // For debug testing we can simply browse other riders and
            // "borrow" their telemetry
            if (Config.GetValue<bool>("Debug"))
            {
                Logger.LogInformation("Debug mode enabled");

                ZwiftPacketMonitor.IncomingPlayerEvent += (s, e) => {     
                    try 
                    {               
                        // When debug mode is on, we'll pick the first INCOMING player's data to use, and will lock
                        // onto that PlayerId to filter out subsequent updates. This makes the testing more consistent. 
                        // This way it's possible to test event dispatch w/o having to be on the bike with power meter 
                        // and heart rate strap actually connected and outputting data.
                        if ((playerState == null) || (playerState.Id == e.PlayerState.Id))
                        {
                            playerState = e.PlayerState;

                            var telemetry = JsonConvert.SerializeObject(new TelemetryModel()
                            {
                                PlayerId = e.PlayerState.Id,
                                Power = e.PlayerState.Power,
                                HeartRate = e.PlayerState.Heartrate
                            });

                            NotificationsService.SendNotificationAsync(telemetry, false).Wait();
                        }
                    }
                    catch (Exception ex) {
                        Logger.LogError(ex, "IncomingPlayerEvent");
                    }
                };
            }
            else {
                // Under normal circumstances we will only be dispatching telemetry data from OUTGOING
                // packets, which represent YOU, that are being sent out to the Zwift servers.
                ZwiftPacketMonitor.OutgoingPlayerEvent += (s, e) => {
                    try 
                    {
                        var telemetry = JsonConvert.SerializeObject(new TelemetryModel()
                        {
                            PlayerId = e.PlayerState.Id,
                            Power = e.PlayerState.Power,
                            HeartRate = e.PlayerState.Heartrate
                        });

                        NotificationsService.SendNotificationAsync(telemetry, false).Wait();
                    }
                    catch (Exception ex) {
                        Logger.LogError(ex, "OutgoingPlayerEvent");
                    }
                };
            }

            Task.Run(() => 
            {
                try 
                {
                    Logger.LogInformation("StartCaptureAsync");
                    ZwiftPacketMonitor.StartCaptureAsync(Config.GetValue<string>("NetworkInterface")).Wait();
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "ZwiftPacketMonitor.StartCaptureAsync");
                }
            });
        }

        private void OnStopping() {
            Logger.LogInformation("OnStopping has been called.");
            ZwiftPacketMonitor.StopCaptureAsync().Wait();
        }

        private void OnStopped() {
            Logger.LogInformation("OnStopped has been called.");
        }
    }
}