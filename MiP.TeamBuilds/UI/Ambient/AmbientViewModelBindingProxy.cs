using System.Windows;

namespace MiP.TeamBuilds.UI.Ambient
{
    /// <summary>
    /// This class helps binding to models, when the UI element is not within the required visual tree, and has not the correct datacontext set.
    /// </summary>
    public class AmbientViewModelBindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new AmbientViewModelBindingProxy();
        }

        public AmbientViewModel Data
        {
            get { return (AmbientViewModel)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(AmbientViewModel), typeof(AmbientViewModelBindingProxy), new UIPropertyMetadata(null));
    }
}
