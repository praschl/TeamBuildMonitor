using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Markup;

namespace MiP.TeamBuilds.UI.Ambient
{
    public class EqualsInt32ToBooleanConverter : MarkupExtension, IValueConverter
    {
        public int ComparisonValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int parsed && parsed == ComparisonValue;
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
