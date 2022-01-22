using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using backend.Modules;
using backend.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Cors;

namespace backend.Controllers
{
    
    [Route("backend/api/[controller]")]
    
    public class SettingsController : Controller
    {
            private TemperatureModule _tempModule;
            private CountDownModule _countModule;

            public SettingsController(TemperatureModule temperatureControlModule, CountDownModule countDownModule)
            {
                _tempModule = temperatureControlModule;
                _countModule = countDownModule;
            }

        [HttpGet("[action]")]
        public string GetMoo()
            {
            return "moooooo";
            }


            [HttpGet("[action]")]
            public IEnumerable<CountDown> GetAllCountDowns()
            {
                return _countModule.GetAllCountDowns();
            }

            //moo fuc

            [HttpGet("[action]")]
            public IEnumerable<TempSensor> GetAllTempSensors()
            {
            return _tempModule.GetSavedProbes();
            }
            
            [HttpGet("[action]")]
            public Dictionary<string, double> GetCurrentTempValues()
        {
            return _tempModule.Readings;
        }
 

            [HttpPost]
            [Route("[action]")]

        public void SaveTempSensorNames([FromBody] List<TempSensor> newSensors)
        {

            Console.WriteLine(newSensors);
             _tempModule.SaveSensors(newSensors);
        }

    }
    }
