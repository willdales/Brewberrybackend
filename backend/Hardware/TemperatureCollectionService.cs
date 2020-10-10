using backend.Modules;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Hardware
{
    public class TemperatureCollectionService : IHostedService, IDisposable 
    {
        private Timer _timer;
        private TemperatureModule _temperatureModule;

        public TemperatureCollectionService(TemperatureModule temperatureModule)
        {
            _temperatureModule = temperatureModule;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CollectTemperatures, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private async void CollectTemperatures(object state)
        {
            await _temperatureModule.GetTemperatureValues();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
