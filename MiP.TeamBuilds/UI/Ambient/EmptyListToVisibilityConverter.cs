using System;
using System.Windows.Data;
using System.Globalization;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace MiP.TeamBuilds.UI.Ambient
{
    public class EmptyListToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public Visibility EmptyValue { get; set; }
        public Visibility ElseValue { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEmpty = value is ICollectionView view && view.IsEmpty;
            return isEmpty ? EmptyValue : ElseValue;
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
