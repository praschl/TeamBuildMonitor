using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace MiP.TeamBuilds.UI.Overview
{
    /// <summary>
    /// This class helps binding to models, when the UI element is not within the required visual tree, and has not the correct datacontext set.
    /// </summary>
    public class OverviewBindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new OverviewBindingProxy();
        }

        public OverviewViewModel Data
        {
            get { return (OverviewViewModel)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(OverviewViewModel), typeof(OverviewBindingProxy), new UIPropertyMetadata(null));
    }
}