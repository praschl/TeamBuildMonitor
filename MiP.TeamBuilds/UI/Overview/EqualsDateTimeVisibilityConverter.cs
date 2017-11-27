using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace MiP.TeamBuilds.UI.Overview
{
    public class EqualsDateTimeVisibilityConverter : MarkupExtension, IValueConverter
    {
        public Visibility DateTimeMinValue { get; set; } = Visibility.Collapsed;
        public Visibility DateTimeNullValue { get; set; } = Visibility.Collapsed;
        public Visibility ElseValue { get; set; } = Visibility.Visible;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime parsed)
            {
                if (parsed == DateTime.MinValue)
                    return DateTimeMinValue;
                if (parsed == null)
                    return DateTimeNullValue;
            }

            return ElseValue;
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
