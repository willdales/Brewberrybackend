using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using backend.Hubs;
using backend.Models;
//using BrewFramework.Models;
using Microsoft.Extensions.Configuration;
using backend.Hardware;
using Microsoft.Extensions.Hosting;
//using HouseBrewPanel.Hardware;

namespace backend.Modules 
{
    public class CountDownModule 
        //IHostedService
    {
        private Timer _timer;
        private readonly SemaphoreSlim _countdownStateLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _countdownValueLock = new SemaphoreSlim(1, 1);
       // private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(250);
        private readonly TimeSpan _tickInterval = TimeSpan.FromSeconds(1);
        private volatile CountDown _countDown;
        private List<CountDown> _allCountDowns;
        private volatile CountDownStatus _countDownState;
        private DisplayManager _displayManager;


        private IConfiguration Configuration;
        private IHubContext<CountDownHub> Hub;

        public CountDownStatus CountDownState
        {
            get { return _countDownState; }
            private set { _countDownState = value; }
        }

        public CountDown CurrentCountDownParams
        {
            get { return _countDown; }
            private set { _countDown = value;  }
        }

        public List<CountDown> GetAllCountDowns()
        {
            return _allCountDowns;
        }

        public CountDownModule(IConfiguration configuration, IHubContext<CountDownHub> hub, DisplayManager displayManager)
        {
            Hub = hub;   
            Configuration = configuration;
            _allCountDowns = new List<CountDown>();
            _displayManager = displayManager;
            Configuration.Bind("CountDowns", _allCountDowns);
            _countDown = _allCountDowns.Find(x => x.Index == 0);
            _countDown.CurrentCount = _countDown.TargetCount;
            displayManager.WriteCountdownDisplays(_countDown);
           // _countDown = new CountDown();
            _countDownState = CountDownStatus.Reset;
        }

        

        public async Task SetCountDownFromPreset(int index)
        {
            await _countdownValueLock.WaitAsync();
            try
            {
                _countDown = _allCountDowns[index];
                _countDown.CurrentCount = _countDown.TargetCount;
                _displayManager.WriteCountdownDisplays(_countDown);
            }
            finally
            {
                _countdownValueLock.Release();
            }

            await BroadcastCountDownChange();
        }

        private async Task BroadcastCountDownChange()
        {
            await _countdownValueLock.WaitAsync();
            try
            {
                await Hub.Clients.All.SendAsync("CountDown_Set", _countDown);
                // OnStatusChanged(new CountDownStatusEventArgs(CountDownStatus.Reset));
                //await Hub.Clients.All.SendAsync("countdown_set", _countDown);
            }
            finally
            {
                _countdownValueLock.Release();
            }
        }

        public async Task SetInterval(TimeSpan interval)
        {
            if (_countDownState == CountDownStatus.Stopped)
            {
                await _countdownValueLock.WaitAsync();
                try
                {
                    _countDown.Name = "Timer";
                    _countDown.TargetCount = interval;
                    _countDown.CurrentCount = interval;
                }
                finally
                {
                    _countdownValueLock.Release();
                }


                await BroadcastCountDownChange();

            }
        }

        
        private async void CountDownTick(object state)
        {
            await _countdownValueLock.WaitAsync();
            try
            {
                if (_countDown.CurrentCount.TotalSeconds > 0)
                {
                    _countDown.CurrentCount = _countDown.CurrentCount.Add(TimeSpan.FromSeconds(-1));
                }


                _displayManager.WriteCountdownDisplays(_countDown);

                await Hub.Clients.All.SendAsync("CountDown_Tick", _countDown);

                // await Hub.Clients.All.SendAsync("countdown_tick", _countDown);



            }
            finally
            {
                _countdownValueLock.Release();
            }
        }
        
        public async Task Start()
        {
            await _countdownStateLock.WaitAsync();
            try
            {
                if (CountDownState != CountDownStatus.Started)
                {
                    _timer = new Timer(CountDownTick, null, _tickInterval, _tickInterval);
                    CountDownState = CountDownStatus.Started;
                    await Hub.Clients.All.SendAsync("CountDown_StatusChanged", CountDownStatus.Started);


                }
            }
            finally
            {
                _countdownStateLock.Release();
            }
        }

        public async Task Stop()
        {
            await _countdownStateLock.WaitAsync();
            try
            {
                if(CountDownState == CountDownStatus.Started)
                {
                    if(_timer != null)
                    {
                        _timer.Dispose();
                    }
                    CountDownState = CountDownStatus.Stopped;
                    await Hub.Clients.All.SendAsync("CountDown_StatusChanged", CountDownStatus.Stopped);

                }
            }
            finally
            {
                _countdownStateLock.Release();
            }
        }

        public async Task Reset()
        {
            await _countdownStateLock.WaitAsync();
            try
            {
                if(CountDownState == CountDownStatus.Stopped)
                {
                    CountDownState = CountDownStatus.Reset;

                    if (_timer != null)
                    {
                        _timer.Dispose();
                    }

                    _countDown.CurrentCount = _countDown.TargetCount;
                   
                    await Hub.Clients.All.SendAsync("CountDown_StatusChanged", CountDownStatus.Reset);


                }
            }
            finally
            {
                _countdownStateLock.Release();
            }
            }



 

    }

}
