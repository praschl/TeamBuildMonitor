﻿using System;
using System.Windows.Threading;

using PropertyChanged;

namespace MiP.TeamBuilds.UI.Notifications
{
    [AddINotifyPropertyChangedInterface]
    public class TimerRefreshViewModel : ITimerRefreshViewModel
    {
        private readonly KnownBuildsViewModel _knownBuildsViewModel;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private DateTime _sleepUntil;

        public TimerRefreshViewModel(KnownBuildsViewModel knownBuildsViewModel)
        {
            _knownBuildsViewModel = knownBuildsViewModel;
        }

        public int SleepForMinutes { get; private set; }

        public void RestartTimer()
        {
            _timer.Stop();

            _knownBuildsViewModel.RebuildTfsProvider();

            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_sleepUntil < DateTime.Now.AddMinutes(30)) SleepForMinutes = 30;
            else if (_sleepUntil < DateTime.Now.AddMinutes(15)) SleepForMinutes = 15;
            else if (_sleepUntil < DateTime.Now) SleepForMinutes = 0;

            _knownBuildsViewModel.NotificationsEnabled = _sleepUntil < DateTime.Now;
            _knownBuildsViewModel.RefreshBuildInfos();
        }

        public void StopRefreshingFor(int minutes)
        {
            SleepForMinutes = minutes;

            if (minutes == -1)
                _sleepUntil = DateTime.MaxValue;
            else
                _sleepUntil = DateTime.Now.AddMinutes(minutes);
        }
    }
}
