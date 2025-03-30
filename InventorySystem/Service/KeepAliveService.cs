using InventorySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Service
{
    public class KeepAliveService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Ajusta el intervalo según sea necesario

        public KeepAliveService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DbInventoryContext>();
                    await dbContext.Database.ExecuteSqlRawAsync("SELECT 1"); // Consulta ligera para mantener la conexión
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }

}
