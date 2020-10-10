using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Device.Gpio;

namespace backend.Modules
{
    public class HardwareIOModule
    {
        private int[] elementsPin;
        GpioController controller;

        public HardwareIOModule(IConfiguration configuration)
        {
            elementsPin = configuration.GetSection("HeatingPort").Get<int[]>();

            controller = new GpioController();

            foreach (int i in elementsPin)
            {
                controller.OpenPin(i, PinMode.Output);
            }
        }

        public void SetHeatingElement(bool turnOn)
        {
            foreach (int i in elementsPin)
            {
                controller.Write(i, turnOn ? PinValue.High : PinValue.Low);
            }
        }
    }
}
