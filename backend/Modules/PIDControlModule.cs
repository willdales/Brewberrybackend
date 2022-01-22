using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.Logic;
using backend.Hubs;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using backend.Hardware;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;

namespace backend.Modules
{
    public class PIDControlModule
    {
        //private volatile PIDState _pidStatus;
        public volatile PIDOperationalParameters op;
        private volatile int counterTick = 0;
        private PIDController pidController;
        private AutoTunePID autoTune;
        private IStoreProvider StoreProvider;
        // private PIDParameter parameters;
        private string activeSensorID;
        private TempSensor activeSensor;
        private IHubContext<PIDHub> Hub;
        private Timer _timer;
        private readonly TimeSpan _tickInterval = TimeSpan.FromMilliseconds(1);
        //private readonly TimeSpan _windowInterval = TimeSpan.FromMilliseconds(5000);
        private DateTime _windowStart;
        private bool prevElState;

        private TemperatureModule _temperatureControlModule;
        private HardwareIOModule _hardwareIOModule;
        private DisplayManager _displayManager;
        private bool UseTuning;

        //const string pidFileName = "pidParameters.json";

        //private string pidStorePath; 

        //public PIDState PIDStatus
        //{
        //    get { return _pidStatus; }
        //    private set { _pidStatus = value; }
        //}

        public PIDControlModule(IHubContext<PIDHub> hub, IConfiguration configuration, TemperatureModule temperatureControlModule, HardwareIOModule hardwareIOModule, DisplayManager displayManager)
        {
            UseTuning = false;
            //_pidStatus = PIDState.Stopped;
            Hub = hub;
            _temperatureControlModule = temperatureControlModule;
            _hardwareIOModule = hardwareIOModule;
            _displayManager = displayManager;
            prevElState = false;
            //pidStorePath = Path.Combine(env.ContentRootPath, pidFileName);
            // StoreProvider = new JsonStoreProvider(configuration);
            //StoreProvider.SetStoreName("pidModule");
            PIDParameter parameters;
            List<PIDParameter> p = new List<PIDParameter>();

            //  = StoreProvider.LoadStore<PIDParameter>();
            if (p.Count > 0)
            {
                parameters = p[0];
            }
            else
            {
                //set default parameters...
                parameters = new PIDParameter()
                {


                    Name = "PID",
                    Kd = 0.01,
                    Ki = 10,
                    Kp = 4000,
                    WindowSize = 10000,
                    Target = 40,
                    LastOutput = 0,
                    LastValue = 0
                    

                };
                p.Add(parameters);
                //StoreProvider.SaveStore<PIDParameter>(p);
            }

            activeSensorID = _temperatureControlModule.GetSavedProbes().Find(sensor => sensor.DefaultSelected).SensorId;

            

            op = new PIDOperationalParameters();

            op.State = PIDState.Stopped;
            op.Params = parameters;


            pidController = new PIDController(op.Params.Kp, op.Params.Ki, op.Params.Kd, 1);

            pidController.SetOutputLimits(0, parameters.WindowSize);
            pidController.Setup(PIDMode.AUTOMATIC, op.Params.Target);

            autoTune = new AutoTunePID(parameters);

            if (UseTuning)
            {
                UseTuning = false;
                SetupAutoTune();
                UseTuning = true;
            }

            _windowStart = DateTime.Now;
            _timer = new Timer(TimerTick, null, _tickInterval, _tickInterval);

            //  temperatureControlModule.TemperatureChanged += TemperatureControlModule_TemperatureChanged;
        }

        private void SetupAutoTune()
        {
            if (!UseTuning)
            {
                autoTune.SetNoiseBand(1);
                autoTune.SetOutputStep(500);
                autoTune.SetLookbackSec(20);
                UseTuning = true;
            }
        }

        private void EnableTuning()
        {
            UseTuning = !UseTuning;
            if (UseTuning)
            {
                autoTune = new AutoTunePID(op.Params);

                if (UseTuning)
                {
                    UseTuning = false;
                    SetupAutoTune();
                    UseTuning = true;
                }
            }
        }

        public async Task GetPing()
        {

            op.ElementState = prevElState;
            await this.Hub.Clients.All.SendAsync("pid_ping", op);
        }

        private async void TimerTick(object state)
        {




            DateTime current = DateTime.Now;

            double tempReading = _temperatureControlModule.Readings[activeSensorID];

            if (tempReading != op.Params.LastValue)
            {
                op.Params.LastValue = tempReading;


                _displayManager.WriteTemperatureDisplays(op.Params);
            }



            if (current.Subtract(_windowStart).TotalMilliseconds > op.Params.WindowSize)
            {
                _windowStart = current;
                await this.Hub.Clients.All.SendAsync("pid_ping", op);
            }

            if (UseTuning)
            {
                int i = autoTune.Runtime();
                if (i != 0)
                {
                    UseTuning = false;
                }


                if (!UseTuning)
                {

                    PIDParameter p = autoTune.GetTuningValues();
                    Console.WriteLine("Tuning Finished...");
                    Console.WriteLine("KP: " + p.Kp);
                    Console.WriteLine("KI: " + p.Ki);
                    Console.WriteLine("KD: " + p.Kd);

                    _hardwareIOModule.SetHeatingElement(false);
                    return;
                }
            }
            else
            {
                pidController.Compute(tempReading);
            }

            double newOutput;
            if (UseTuning)

            {
                newOutput = autoTune.GetOutput();
            }
            else

            {
                newOutput = pidController.PIDOutput;
            }

            if (!UseTuning)
            {
                if (pidController.PIDOutput != op.Params.LastOutput)
                {

                    op.Params.LastOutput = newOutput;
                    await this.Hub.Clients.All.SendAsync("pid_ping", op);
                    // Console.WriteLine("New Output: " + newOutput);

                }
            }
            else
            {
                if (autoTune.Output != op.Params.LastOutput)
                {
                    op.Params.LastOutput = autoTune.Output;
                    Console.WriteLine("Using:");
                    PIDParameter p = autoTune.GetTuningValues();

                    Console.WriteLine("KP: " + p.Kp);
                    Console.WriteLine("KI: " + p.Ki);
                    Console.WriteLine("KD: " + p.Kd);
                }
            }


            if ((newOutput > 100) && (newOutput > current.Subtract(_windowStart).TotalMilliseconds))
            {
                if (!prevElState)
                {
                    prevElState = true;
                    op.ElementState = prevElState;
                    await this.Hub.Clients.All.SendAsync("pid_change", "CONTROL", "ON");
                    _hardwareIOModule.SetHeatingElement(prevElState);
                    
                }

            }
            else
            {
                if (prevElState)
                {
                    prevElState = false;
                    op.ElementState = prevElState;
                    _hardwareIOModule.SetHeatingElement(prevElState);
                    await this.Hub.Clients.All.SendAsync("pid_change", "CONTROL", "OFF");
                }
            }


            //if (now - windowStartTime > WindowSize)
            //{ //time to shift the Relay Window
            //    windowStartTime += WindowSize;
            //}
            //if ((onTime > 100) && (onTime > (now - windowStartTime)))
            //{
            //    digitalWrite(RelayPin, HIGH);
            //}
            //else
            //{
            //    digitalWrite(RelayPin, LOW);
            //}


            // if (counterTick == 10)
            //{
            //counterTick = 0;
            //    await this.Hub.Clients.All.SendAsync("pid_ping", op);
            //}

            //wtf

            // activeSensorID

            // output will be a value between 0 and 5000



            //if (current - _windowStart > TimeSpan.FromMilliseconds(op.Params.WindowSize))
            //{
            //    _windowStart = current;
            //    await this.Hub.Clients.All.SendAsync("pid_change", "NEWWINDOW", op.Params.WindowSize);
            //    //Console.WriteLine("new window");
            //}


            // Console.WriteLine("total milis: " + (current - _windowStart).TotalMilliseconds);
            //Console.WriteLine("pid output:" + pidController.PIDOutput);

            //if (pidController.PIDOutput > (current - _windowStart).TotalMilliseconds)  
            //{
            //    Console.WriteLine("ON");
            //    //if (op.State == PIDState.Started)
            //    //{
            //        _hardwareIOModule.SetHeatingElement(true);

            //    //}
            //    await this.Hub.Clients.All.SendAsync("pid_change", "CONTROL", "ON");
            //}
            //else
            //{

            //    Console.WriteLine("OFF");
            //    //if (op.State == PIDState.Started)
            //    //{
            //        _hardwareIOModule.SetHeatingElement(false);
            //    //}
            //    await this.Hub.Clients.All.SendAsync("pid_change", "CONTROL", "OFF");
            //}

            //void loop()
            //{
            //    sensors.requestTemperatures();
            //    Input = sensors.getTempCByIndex(0);
            //    Serial.print("Temperature: ");
            //    Serial.println(Input);
            //    myPID.Compute();

            //    /************************************************
            //     * turn the output pin on/off based on pid output
            //     ************************************************/
            //    if (millis() - windowStartTime > WindowSize)
            //    {e
            //        //time to shift the Relay Window
            //        windowStartTime += WindowSize;
            //    }
            //    if (Output < millis() - windowStartTime)
            //        digitalWrite(RELAY_PIN, HIGH);
            //    else
            //        digitalWrite(RELAY_PIN, LOW);

            //}
            //    if()

        }
        //private void TemperatureControlModule_TemperatureChanged(object sender, EventArgs e)
        //{
        //    if(this.activeSensorID != null)
        //    {
        //        this.activeSensor = _temperatureControlModule.GetCurrentSensorValue(this.activeSensorID);

        //        Hub.Clients.All.SendAsync("pidTemp_change", this.activeSensor, parameters.Target);
        //        //pidTemp_change
        //    }
        //}

        public async Task setTargetTemperature(string target)
        {
            Console.WriteLine("target change: " + target);
            op.Params.Target = Convert.ToDouble(target);
            pidController.Setup(PIDMode.AUTOMATIC, op.Params.Target);
            await this.Hub.Clients.All.SendAsync("pid_change", "NEWTARGET", target);
        }

        public async Task ChangeTempSensor(string sensorID)
        {
            activeSensorID = sensorID;
            await setTargetTemperature(op.Params.Target.ToString());

            //Hub.Clients.All.SendAsync("pidTemp_change", this.activeSensor, parameters.Target);
            //_temperatureControlModule.
        }


    }



}
