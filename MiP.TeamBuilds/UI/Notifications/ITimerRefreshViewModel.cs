namespace MiP.TeamBuilds.UI.Notifications
{
    public interface ITimerRefreshViewModel
    {
        int SleepForMinutes { get; }

        void RestartTimer();

        void StopRefreshingFor(int minutes);
    }
}
