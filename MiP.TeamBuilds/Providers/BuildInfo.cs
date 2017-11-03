using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Microsoft.TeamFoundation.Build.Client;

namespace MiP.TeamBuilds.Providers
{
    public class BuildInfo
    {
        private IQueuedBuild _build;

        public BuildInfo(IQueuedBuild build)
        {
            _build = build;
        }

        public int Id { get; set; }

        public string TeamProject { get; set; }

        public string BuildDefinitionName { get; set; }

        public string[] ServerItems { get; set; }

        public string RequestedBy { get; set; }
        
        private BuildStatus _status;
        public BuildStatus Status
        {
            get => _status;
            set
            {
                if (value == _status)
                    return;
                _status = value;
                OnPropertyChanged();
            }
        }

        private Exception _pollingException;
        public Exception PollingException
        {
            get => _pollingException;
            set
            {
                if (value == _pollingException)
                    return;
                _pollingException = value;
                OnPropertyChanged();
            }
        }

        public void Connect()
        {
            _build.Connect();
            _build.StatusChanged += Build_StatusChanged;
        }
        
        public void Disconnect()
        {
            _build.StatusChanged -= Build_StatusChanged;
            _build.Disconnect();
        }
        
        private void Build_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            Status = _build.Build.Status;
        }

        //
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
