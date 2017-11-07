using System.Windows;

namespace MiP.TeamBuilds.UI.Main
{
    /// <summary>
    /// This class helps binding to models, when the UI element is not within the required visual tree, and has not the correct datacontext set.
    /// </summary>
    public class MainViewModelBindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new MainViewModelBindingProxy();
        }

        public MainViewModel Data
        {
            get { return (MainViewModel)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(MainViewModel), typeof(MainViewModelBindingProxy), new UIPropertyMetadata(null));
    }
}
