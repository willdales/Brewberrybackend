using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
//using BrewManagementFrontend.Backend;
using backend.Modules;
//using BrewFramework.Models;
using backend.Models;

namespace backend.Hubs
{

    public partial class TemperatureHub : Hub
    {
        private readonly TemperatureModule temperatureModule;

        public TemperatureHub(TemperatureModule module)
        {
            temperatureModule = module;
        }

        public List<TempSensor> GetSavedProbes()
        {
            return temperatureModule.GetSavedProbes();
        }



        //public Dictionary<string, GetTempSensors()
        //{
        //    return temperatureModule.GetCurrentValues();
        //}
        //public List<string> getRawHardwareDevices()
        //{
        //   return temperatureModule.
        //}



        //public async Task RegisterHardwareDevice()
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, "hardware");
        //}

        //public async Task TempReadings(Dictionary<string, double> readings)
        //{
        //    await _temperatureControlModule.SetTemperatureValues(readings);
        //}


        //public List<TempSensor> GetCurrentSensorValues()
        //\{
        //    return _temperatureControlModule.GetCurrentValues();
        //}

        //public void SetActiveProbe(string activeProbe)
        //{
        //    _PIDControlModule.ChangeTempSensor(activeProbe);
        //}
    }
}
