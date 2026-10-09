using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DailyPlaner.Converters
{
    public class BooleanToTextDecorationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool completed = value is bool boolValue && boolValue;
            if (completed)
            {
                return new TextDecorationCollection
                {
                    new TextDecoration
                    {
                        Location = TextDecorationLocation.Strikethrough,
                        Pen = new Pen(System.Windows.Media.Brushes.Gray, 1.5)
                        {
                            DashStyle = new DashStyle(new double[] { 2, 2 }, 0)
                        }
                    }
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class BooleanToStatusBadgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool completed = value is bool boolValue && boolValue;
            if (parameter is string kind && kind == "event")
            {
                return completed ? "Завершено" : "Запланировано";
            }
            return completed ? "Выполнена" : "Ожидает";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue.IndexOf("Выполнена", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            return false;
        }
    }

}
