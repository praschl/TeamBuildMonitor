namespace MiP.TeamBuilds.UI.Notifications
{
    public interface IRefreshBuildsTimer
    {
        void RestartTimer();

        void StopRefreshingFor(int minutes);
    }
}
