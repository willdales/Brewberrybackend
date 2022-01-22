using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Device.I2c;

namespace backend.Hardware
{
    public class I2cBusDevice
    {
        protected int Bus = 1;

        protected I2cDevice GetDevice(I2cConnectionSettings settings)
        {
            return I2cDevice.Create(settings);
            //return new System.Device.I2c.I2cDevice()
           // return new Windows10I2cDevice(settings);
        }
    }
}
