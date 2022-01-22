using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using backend.Modules;

namespace backend.Hubs
{
    public class PIDHub : Hub
    {
        private PIDControlModule module;
        public PIDHub(PIDControlModule pIDControlModule)
        {
            module = pIDControlModule;
        }

        public async Task SetPIDTarget(string target)
        {
            Console.WriteLine("Hub target change: " + target);
            await module.setTargetTemperature(target);
        }

        public async Task SetPIDSensor(string sensorID)
        {
             module.ChangeTempSensor(sensorID);
        }

        public async Task GetPing()
        {
            await module.GetPing();
        }


        //public async Task StartCountDown()
        //{
        //    await _countDownModule.Start();
        //}

        //public CountDownStatus GetCountDownState()
        //{
        //    return _countDownModule.CountDownState;
        //}
        //pppput stuff here
    }
}
