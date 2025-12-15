using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Flexi2.Models;

namespace Flexi2.Converters
{
    public class TableStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            return (TableStatus)value == TableStatus.Free
                ? new SolidColorBrush(Color.FromRgb(34, 197, 94))   // зелено
                : new SolidColorBrush(Color.FromRgb(239, 68, 68)); // червено
        }

        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => throw new NotImplementedException();
    }
}
