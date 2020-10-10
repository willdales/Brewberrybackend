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

    public class CountDownHub : Hub
    {
       
        private readonly CountDownModule _countDownModule;
        //private readonly TemperatureControlModule _temperatureControlModule;
        //private readonly PIDControlModule _PIDControlModule;

        public CountDownHub(CountDownModule countDownModule)
        {
            _countDownModule = countDownModule;
           
            
            
            //_temperatureControlModule = temperatureControlModule;
           // _PIDControlModule = pidControlModule;
        }

        

        public override Task OnConnectedAsync()
        {

            return base.OnConnectedAsync();
        }
        
       

        public async Task StopCountDown()
        {
            await _countDownModule.Stop();
        }
        public async Task StartCountDown()
        {
            await _countDownModule.Start();
        }

        public CountDownStatus GetCountDownState()
        {
            return _countDownModule.CountDownState;
        }

        public CountDown GetCountDownParameters()
        {
            return _countDownModule.CurrentCountDownParams;
        }

        public List<CountDown> GetAllCountDowns()
        {
            return _countDownModule.GetAllCountDowns();
        }

        public async Task ResetCountDown()
        {
            await _countDownModule.Reset();
        }

        public async Task SetCountDownFromPreset(string index)
        {
            await _countDownModule.SetCountDownFromPreset(Convert.ToInt32(index));
        }

        public async Task SetCountDownInterval(TimeSpan ts)
        {
            await _countDownModule.SetInterval(ts);
        }

        //public async Task RegisterHardwareDevice()
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, "hardware");
        //}
        
        //public async Task TempReadings(Dictionary<string, double> readings)
        //{
        //    await _temperatureControlModule.SetTemperatureValues(readings);
        //}

        //public List<TempSensor> GetCurrentSensorValues()
        //{
        //    return _temperatureControlModule.GetCurrentValues();
        //}

        //public void SetActiveProbe(string activeProbe)
        //{
        //    _PIDControlModule.ChangeTempSensor(activeProbe);
        //}
    }
}
