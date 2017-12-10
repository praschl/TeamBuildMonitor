using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace MiP.TeamBuilds.UI.Ambient
{
    public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool parsed && parsed ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
