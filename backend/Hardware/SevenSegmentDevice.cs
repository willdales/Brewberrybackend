using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Device.I2c;
using backend.Models;

namespace backend.Hardware
{
    public class SevenSegmentDevice : I2cBusDevice, IDisposable, IStored
    {
        //// public sealed class SevenSegDevice : I2cBusDevice, IDisposable, IStored
        // {
        private I2cDevice _i2cDevice;
        public string Name { get; set; }
        public byte DeviceAddress { get; set; }
        public bool IsInitialized { get; private set; }
        public bool IsReady { get; private set; }
        private Dictionary<string, byte> NumberTable = new Dictionary<string, byte>();
        private List<byte[]> DisplayBuffer = new List<byte[]>();
        //I2cController controller;

        private void Initialize()
        {
            Console.WriteLine("Initializing: " + DeviceAddress + " on Bus: " + Bus);
            IsReady = false;
            var i2cSettings = new I2cConnectionSettings(Bus, DeviceAddress);
            //i2cSettings.BusSpeed = I2cBusSpeed.StandardMode;

            _i2cDevice = GetDevice(i2cSettings);
            //_i2cDevice.
            this.IsInitialized = true;
        }

        public void SetupDisplay()
        {
            // this.controller = _controller;
            if (!IsInitialized)
            {
                Initialize();
            }

            _i2cDevice.Write(new byte[] { 0x21 });

            //set display on
            _i2cDevice.Write(new byte[] { 0x81 });

            NumberTable.Add("0", 0x3F);
            NumberTable.Add("1", 0x06);
            NumberTable.Add("2", 0x5B);
            NumberTable.Add("3", 0x4F);
            NumberTable.Add("4", 0x66);
            NumberTable.Add("5", 0x6D);
            NumberTable.Add("6", 0x7D);
            NumberTable.Add("7", 0x07);
            NumberTable.Add("8", 0x7F);
            NumberTable.Add("9", 0x6F);
            NumberTable.Add("a", 0x77);
            NumberTable.Add("b", 0x7C);
            NumberTable.Add("c", 0x98); //????????
            NumberTable.Add("C", 0x39);
            NumberTable.Add("d", 0x5E);
            NumberTable.Add("E", 0x79);
            NumberTable.Add("F", 0x71);
            NumberTable.Add("r", 0x50);
            NumberTable.Add("e", 0x7B);
            NumberTable.Add(" ", 0x00);


            //Wait for display to startup. Can take up to a second (see datasheet)
            //await Task.Delay(1000);
            System.Threading.Tasks.Task.Delay(1000).Wait();

            Clear();
            WriteBuffer();
            IsReady = true;
        }

        private void Clear()
        {
            DisplayBuffer.Clear();

            //Loop addresses, 4 digits plus central colon.
            for (int i = 0; i < 5; i++)
            {
                //Each digit is on an even address
                DisplayBuffer.Add(new byte[] { (byte)(i * 2), 0x00 });
            }
        }

        private void WriteBuffer()
        {
            for (int i = 0; i < 5; i++)
            {
                _i2cDevice.Write(DisplayBuffer[i]);
            }
        }

        public void WriteColon(bool draw)
        {
            //assume always using display of type 0.0.:0.0.
            byte drawVal;
            if (draw)
            {
                drawVal = 0xFF; //???????
            }
            else
            {
                drawVal = 0x00;
            }

            DisplayBuffer[2] = new byte[] { 0x04, drawVal };
        }

        public void WriteCountTime(TimeSpan timespan)
        {

            int minutedisp = timespan.Minutes;
            minutedisp = (timespan.Hours * 60) + minutedisp;

            string moop = minutedisp.ToString("00") + timespan.Seconds.ToString("00");

            WriteDigit(moop[0].ToString(), 1, false);
            WriteDigit(moop[1].ToString(), 2, false);
            WriteColon(true);
            WriteDigit(moop[2].ToString(), 3, false);
            WriteDigit(moop[3].ToString(), 4, false);

            WriteBuffer();
        }

        public void WriteDouble(string writeChars, bool writecolon)
        {

            int hasdot = writeChars.IndexOf('.');

            writeChars = writeChars.Replace(".", "");

            for (int i = 0; i < 4; i++)
            {
                if (writeChars.Length - 1 < i)
                {
                    WriteDigit(" ", i + 1, false);
                }
                else
                {
                    if (hasdot == (i + 1))
                    {
                        WriteDigit(writeChars[i].ToString(), i + 1, true);
                    }
                    else
                    {
                        WriteDigit(writeChars[i].ToString(), i + 1, false);
                    }
                }

            }


            // WriteDigit(writeChars[1].ToString(), 2, false);
            //WriteColon(writecolon);
            // WriteDigit(writeChars[2].ToString(), 3, false);
            // WriteDigit(writeChars[3].ToString(), 4, false);

            WriteBuffer();
        }

        public void WriteString(string writeChars, bool writecolon)
        {
            WriteDigit(writeChars[0].ToString(), 1, false);
            WriteDigit(writeChars[1].ToString(), 2, false);
            WriteColon(writecolon);
            WriteDigit(writeChars[2].ToString(), 3, false);
            WriteDigit(writeChars[3].ToString(), 4, false);

            WriteBuffer();
        }

        public void WriteTime(DateTimeOffset dt)
        {
            string hoursportion = dt.Hour.ToString("00");
            string minutesportion = dt.Minute.ToString("00");

            WriteDigit(hoursportion[0].ToString(), 1, false);
            WriteDigit(hoursportion[1].ToString(), 2, false);
            WriteColon(true);
            WriteDigit(minutesportion[0].ToString(), 3, false);
            WriteDigit(minutesportion[1].ToString(), 4, false);

            WriteBuffer();
        }

        public void WriteDigit(string digit, int position, bool dot)
        {
            //skip position for colon

            if (position > 2)
            {
                position++;
            }

            byte digitValue = NumberTable[digit];

            //puts stupid hat on
            if (dot) digitValue = (byte)(digitValue | 1 << 7);

            DisplayBuffer[position - 1] = new byte[] { (byte)((position - 1) * 2), digitValue };

        }
        public void Dispose()
        {

            if (_i2cDevice != null)
            {
                _i2cDevice.Dispose();
                _i2cDevice = null;
            }

            GC.SuppressFinalize(this);
        }

    }
}
