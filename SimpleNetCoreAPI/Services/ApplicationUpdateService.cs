using SimpleNetCoreAPI.Server.Models;
using SimpleNetCoreAPI.WebSockets;

namespace SimpleNetCoreAPI.Services
{
    public class ApplicationUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan checkInterval = TimeSpan.FromSeconds(1);
        private readonly ApplicationWebSocketHandler _webSocketHandler;
        public ApplicationUpdateService(IServiceProvider serviceProvider, ApplicationWebSocketHandler handler)
        {
            _serviceProvider = serviceProvider;
            _webSocketHandler = handler;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Fetch the current time
                    var currentTime = DateTime.UtcNow;

                    // Select applications that were created more than a minute ago and have not yet been updated
                    var applicationsToUpdate = dbContext.Applications
                        .Where(a => a.Date.AddMinutes(1) <= currentTime && a.Status != Enums.ApplicationStatus.Completed)
                        .ToList();

                    bool updatedApplications = false;

                    foreach (var app in applicationsToUpdate)
                    {
                        app.Status = Enums.ApplicationStatus.Completed;
                        updatedApplications = true;
                    }

                    // Save changes and notify clients if any applications were updated
                    if (updatedApplications)
                    {
                        dbContext.SaveChanges();
                        await _webSocketHandler.NotifyClientsOfDataChangeAsync("Applications updated");
                    }
                }

                // Wait for the next check interval
                // This interval can be shorter than 1 minute as the update logic is based on the application's creation time
                await Task.Delay(checkInterval, stoppingToken);
            }
        }
        protected async Task Test(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
                    var applicationsToUpdate = dbContext.Applications
                        .Where(a => a.Date <= oneMinuteAgo && a.Date > oneMinuteAgo.Add(-checkInterval))
                        .ToList();

                    foreach (var app in applicationsToUpdate)
                    {
                        app.Status = Enums.ApplicationStatus.Completed;
                    }
                    await Task.Delay(checkInterval, stoppingToken);
                    dbContext.SaveChanges();
                    
                    await _webSocketHandler.NotifyClientsOfDataChangeAsync("Applications updated");
                }

                
            }
        }
    }
}
