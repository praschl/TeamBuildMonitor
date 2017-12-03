using System.Windows.Data;
using System;
using System.Windows.Markup;
using Humanizer;
using System.Globalization;

namespace MiP.TeamBuilds.UI.Overview
{

    public class HumanizerConverter : MarkupExtension, IValueConverter
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
                return date.Humanize(false);
            if (value is TimeSpan time)
                return time.Humanize(1);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
