using backend.Logic;
using backend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Hardware
{
    public class DisplayManager : IHostedService
    {
        //HubConnection connection;
        private IStoreProvider StoreProvider;
        public Dictionary<string, SevenSegmentDevice> segDisplays = new Dictionary<string, SevenSegmentDevice>();
        //private I2cController controller;
        private bool hardwareEnabled = false;
        private IConfiguration _configuration;

        public DisplayManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.initDisplays();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }



        public void WriteCountdownDisplays(CountDown _countdown)
        {
            // if (!hardwareEnabled) return;
            var green1 = segDisplays["green1"];
            var red1 = segDisplays["red1"];
            if (green1.IsReady)
            {
                green1.WriteCountTime(_countdown.CurrentCount);

            }
            if (red1.IsReady)
            {
                red1.WriteCountTime(_countdown.TargetCount);
            }
        }

        public void WriteTemperatureDisplays(PIDParameter _pidParameter)
        {
            //Console.WriteLine("write PID params");
            // if (!hardwareEnabled) return;
            var red2 = segDisplays["red2"];
            var green2 = segDisplays["green2"];

            if (red2.IsReady)
            {
                //.WriteLine("write PID red2: " + _pidParameter.LastValue + "C");

                if (_pidParameter.Target < 100)
                {
                    red2.WriteDouble(_pidParameter.Target.ToString("00.0C"), false);
                }
                else
                {
                    red2.WriteDouble(_pidParameter.Target.ToString("000C"), false);
                }
            }

            if (green2.IsReady)
            {
                if (_pidParameter.LastValue < 100)
                {
                    green2.WriteDouble(_pidParameter.LastValue.ToString("00.0C"), false);
                }
                else
                {
                    green2.WriteDouble(_pidParameter.LastValue.ToString("000C"), false);
                }
            }
        }

        public void initDisplays()
        {
            Console.WriteLine("init displays....");
            // if (!hardwareEnabled) return;
            StoreProvider = new JsonStoreProvider(_configuration);

            StoreProvider.SetStoreName("segDisplays");
            List<SevenSegmentDevice> p = StoreProvider.LoadStore<SevenSegmentDevice>();


            segDisplays.Add("red1", new SevenSegmentDevice() { DeviceAddress = 0x72 });
            segDisplays.Add("green1", new SevenSegmentDevice() { DeviceAddress = 0x70 });

            segDisplays.Add("red2", new SevenSegmentDevice() { DeviceAddress = 0x73 });
            segDisplays.Add("green2", new SevenSegmentDevice() { DeviceAddress = 0x71 });

            //  segDisplays.Add("rogue1", new SevenSegmentDevice() { DeviceAddress = 0x74 });

            segDisplays.AsParallel().ForAll(disp =>
            {
                disp.Value.SetupDisplay();
            });

            foreach (SevenSegmentDevice dev in segDisplays.Values)
            {
                //teehee

                Console.WriteLine("writing stupid things on: " + dev.DeviceAddress);

                dev.WriteString("bEEr", false);
            }

            //CurrentCountDownParams
            //            connection.InvokeAsync<Dictionary<string, double>>("TempReadings", readings);
            //CountDown c = await connection.InvokeAsync("CurrentCountDownParams");



            //connection.InvokeAsync
        }
    }
}
