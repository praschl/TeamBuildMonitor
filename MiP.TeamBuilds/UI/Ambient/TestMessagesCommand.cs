using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Notifications;
using System;
using System.Windows.Input;
using ToastNotifications;
using ToastNotifications.Core;
using MiP.TeamBuilds.IoC;
using Autofac;
using MiP.TeamBuilds.UI.CompositeNotifications;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.Client;

namespace MiP.TeamBuilds.UI.Ambient
{
#if DEBUG
    public class TestMessagesCommand : ICommand
    {
        private Notifier _notifier;
        private readonly MessageOptions _defaultOptions = new MessageOptions
        {
            FreezeOnMouseEnter = true,
            UnfreezeOnMouseLeave = true,
            ShowCloseButton = false,
            NotificationClickAction = n => n.Close()
        };


        public TestMessagesCommand()
        {
            _notifier = ServiceLocator.Instance.Resolve<Notifier>();
        }

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object parameter) => true;

        public async void Execute(object parameter)
        {
            // async-void without try catch... Justification = "This is just available in debug mode"
            const int delay = 50;

            _notifier.ShowInformation(new TextMessage("Info", "Information Message"), _defaultOptions);
            await Task.Delay(delay);
            _notifier.ShowSuccess(new TextMessage("Success", "Success Message"), _defaultOptions);
            await Task.Delay(delay);
            _notifier.ShowWarning(new TextWithLinkMessage("Warning", "Information Message", "This is a link", n => n.Close()), _defaultOptions);
            await Task.Delay(delay);
            _notifier.ShowError(new ExceptionMessage("Error", new NotImplementedException(), _notifier), _defaultOptions);

            await Task.Delay(delay);
            var build = new BuildInfo(null)
            {
                BuildStatus = BuildStatus.Failed,
                BuildDefinitionName = "My-Build",
                BuildSummary = new Uri("http://google.com"),
                DropLocation = "C:\\",
                RequestedBy = "Michael Praschl",
                QueuedTime = DateTime.Now.AddMinutes(-23).AddSeconds(14),
                FinishTime = DateTime.Now
            };

            _notifier.ShowBuildInfoMessage(build, _defaultOptions);

            await Task.Delay(delay);
            build.BuildStatus = BuildStatus.InProgress;
            _notifier.ShowBuildInfoMessage(build, _defaultOptions);

            await Task.Delay(delay);
            build.BuildStatus = BuildStatus.NotStarted;
            _notifier.ShowBuildInfoMessage(build, _defaultOptions);

            await Task.Delay(delay);
            build.BuildStatus = BuildStatus.PartiallySucceeded;
            _notifier.ShowBuildInfoMessage(build, _defaultOptions);

            await Task.Delay(delay);
            build.BuildStatus = BuildStatus.Stopped;
            _notifier.ShowBuildInfoMessage(build, _defaultOptions);

            await Task.Delay(delay);
            build.BuildStatus = BuildStatus.Succeeded;
            _notifier.ShowBuildInfoMessage(build, _defaultOptions);
        }
    }
#endif
}
