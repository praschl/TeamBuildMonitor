using System.Windows.Data;
using System;
using System.Windows.Markup;
using System.Globalization;
using System.Windows;

namespace MiP.TeamBuilds.UI.Overview
{

    public class StringEmptyToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public Visibility EmptyResult { get; set; } = Visibility.Collapsed;
        public Visibility ElseResult { get; set; } = Visibility.Visible;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return EmptyResult;
            if (string.Empty.Equals(value))
                return EmptyResult;

            return ElseResult;
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
