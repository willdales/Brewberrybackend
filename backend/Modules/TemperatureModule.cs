using backend.Hardware;
using backend.Hubs;
using backend.Logic;
using backend.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Modules
{
    public class TemperatureModule
    {
        private Timer _timer;
        private IHubContext<TemperatureHub> Hub;
        private IConfiguration Configuration;
        private TemperatureProbeManager probemanager;
        private IStoreProvider StoreProvider;
        private List<TempSensor> sensors;
        public Dictionary<string, double> Readings { get; set; }
        private string envFolder;
        private readonly TimeSpan _tickInterval = TimeSpan.FromSeconds(5);
        

        public event EventHandler TemperatureChanged;


        //const string tempFileName = "tempSensors.json";
        //private string tempStorePath;

        public TemperatureModule(IHubContext<TemperatureHub> hub, IConfiguration configuration, TemperatureProbeManager temperatureProbeManager)
        {
            sensors = new List<TempSensor>();
            probemanager = temperatureProbeManager;
            Hub = hub;
            Configuration = configuration;
            StoreProvider = new JsonStoreProvider(configuration);
            
            StoreProvider.SetStoreName("tempSensors");
            //Configuration.
            //envFolder = env.ContentRootPath;
            //tempStorePath = Path.Combine(envFolder, tempFileName);
            this.Initialize();



            sensors = StoreProvider.LoadStore<TempSensor>();
            //this.StartCollection();

            //StoreProvider.LoadStore
            //LoadStore();



        }

        public List<TempSensor> GetSavedProbes()
        {

            foreach(TempSensor t in sensors)
            {
                if(!Readings.ContainsKey(t.SensorId))
                {
                    t.State = SensorState.Deleted;
                }
            }

            foreach(string sensorId in Readings.Keys)
            {
                TempSensor existing = sensors.Find(s => s.SensorId == sensorId);

                if(existing is null)
                {
                    sensors.Add(
                        new TempSensor()
                        {
                            SensorId = sensorId,
                            DefaultSelected = false,
                            State = SensorState.New,
                            Name = ""
                            
                        });
                }
               
            }

            return sensors;
        }

        public void Initialize()
        {
            probemanager.InitializeHardware();
            probemanager.InitializeProbes();
        }


        protected virtual void OnTemperaturesChanged(EventArgs e)
        {
            EventHandler handler = TemperatureChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }



        private async void CountDownTick(object state)
        {
            await GetTemperatureValues();
        }

        //    private void LoadStore()
        //{
        //    if (File.Exists(tempStorePath))
        //    {
        //        using (StreamReader file = File.OpenText(tempStorePath))
        //        {
        //            JsonSerializer serializer = new JsonSerializer();
        //            sensors = (List<TempSensor>)serializer.Deserialize(file, typeof(List<TempSensor>));
        //        }
        //    }
        //}

        public void StartCollection()
        {
            _timer = new Timer(CountDownTick, null, _tickInterval, _tickInterval);
        }

        //public List<TempSensor> GetCurrentValues()
        //{
        //    return sensors;
        //}

        
        

        //public TempSensor GetCurrentSensorValue(string sensorID)
        //{
        //    return sensors.Find(item => item.SensorId == sensorID);
        //}

        public void SaveSensors(List<TempSensor> _sensors)
        {
            sensors = _sensors;
            StoreProvider.SaveStore<TempSensor>(sensors);
            // SaveStore();
        }


        public async Task GetTemperatureValues()
        {
            Readings = probemanager.readTemperatures();
            
            await SetTemperatureValues();

        }

        public async Task SetTemperatureValues()
        {
         

            OnTemperaturesChanged(EventArgs.Empty);
            await Hub.Clients.All.SendAsync("tempvalues_set", Readings);
        }
    
}
}
