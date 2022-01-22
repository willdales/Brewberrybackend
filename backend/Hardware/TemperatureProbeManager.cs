using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Rinsen.IoT.OneWire;

namespace backend.Hardware
{
    public class TemperatureProbeManager
    {
        int i2cBus = 1;
        int i2cAddress = 0x18;
        DS2482_100 ds2482;
        IReadOnlyCollection<DS18B20> sensors;
        private readonly SemaphoreSlim _probeReadLock = new SemaphoreSlim(1, 1);


        public TemperatureProbeManager(IConfiguration configuration)
        {
            i2cBus = configuration.GetValue<Int32>("I2CBus");
            i2cAddress = configuration.GetValue<Int32>("ds2482Address");
            //using (var ds2482_100 = await _dS2482DeviceFactory.CreateDS2482_100(true, true))
            //{
            //}

        }

        public void InitializeHardware()
        {
            
            DS2482DeviceFactory dS2482DeviceFactory = new DS2482DeviceFactory();
            ds2482 = dS2482DeviceFactory.CreateDS2482_100(false, false);

        }

        public void InitializeProbes()
        {
            sensors = ds2482.GetDevices<DS18B20>();
            Task.Delay(TimeSpan.FromSeconds(3)).Wait();

        }

        public Dictionary<string, double> readTemperatures()
        {
            
            _probeReadLock.Wait();

            try
            {

                if(sensors is null)
                {
                    this.InitializeProbes();
                }

                Dictionary<string, double> readings = new Dictionary<string, double>();

                foreach (var tempSensor in sensors)
                {
                    try
                    {
                        var result = tempSensor.GetTemperature();
                        Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
                        readings.Add(tempSensor.OneWireAddressString, Math.Round(result, 2));
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Error Reading Probes...");
                    }
                    //connection.InvokeAsync<Dictionary<string, double>>("TempReadings", readings);
                }

                return readings;

            }
            
            finally
            {
                _probeReadLock.Release();
            }


        }

    }
}
