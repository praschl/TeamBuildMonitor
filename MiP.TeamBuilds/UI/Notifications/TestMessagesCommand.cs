using System;

using Microsoft.TeamFoundation.Build.Client;

using MiP.TeamBuilds.Providers;

using ToastNotifications;
using ToastNotifications.Core;
using System.Windows.Input;
using System.Threading.Tasks;
using MiP.TeamBuilds.UI.CompositeNotifications;

namespace MiP.TeamBuilds.UI.Notifications
{
    public partial class KnownBuildsViewModel
    {
        // TODO: This command should add some fake builds to the next call of GetCurrentBuildsAsync()
        public class TestMessagesCommand : ICommand
        {
            private Notifier _notifier;
            private readonly MessageOptions _defaultOptions;

            public TestMessagesCommand(Notifier notifier, MessageOptions defaultOptions)
            {
                _notifier = notifier;
                _defaultOptions = defaultOptions;
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
                _notifier.ShowWarning(new TextWithLinkMessage("Warning", "Information Message", "This is a link", n=>n.Close()), _defaultOptions);
                await Task.Delay(delay);
                _notifier.ShowError(new ExceptionMessage("Error", new NotImplementedException(), _notifier), _defaultOptions);

                await Task.Delay(delay);
                var build = new BuildInfo(null)
                {
                    Status = BuildStatus.Failed,
                    BuildDefinitionName = "My-Build",
                    BuildSummary = new Uri("http://google.com"),
                    DropLocation = "C:\\",
                    RequestedBy = "Michael Praschl",
                    QueuedTime = DateTime.Now.AddMinutes(-23).AddSeconds(14),
                    FinishTime = DateTime.Now
                };

                _notifier.ShowBuildInfoMessage(build, _defaultOptions);

                await Task.Delay(delay);
                build.Status = BuildStatus.InProgress;
                _notifier.ShowBuildInfoMessage(build, _defaultOptions);

                await Task.Delay(delay);
                build.Status = BuildStatus.NotStarted;
                _notifier.ShowBuildInfoMessage(build, _defaultOptions);

                await Task.Delay(delay);
                build.Status = BuildStatus.PartiallySucceeded;
                _notifier.ShowBuildInfoMessage(build, _defaultOptions);

                await Task.Delay(delay);
                build.Status = BuildStatus.Stopped;
                _notifier.ShowBuildInfoMessage(build, _defaultOptions);

                await Task.Delay(delay);
                build.Status = BuildStatus.Succeeded;
                _notifier.ShowBuildInfoMessage(build, _defaultOptions);
            }
        }
    }
}
