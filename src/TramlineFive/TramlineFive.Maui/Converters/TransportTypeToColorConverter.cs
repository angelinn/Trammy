using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;


namespace TramlineFive.Converters
{
    public class TransportTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isLightTheme = Application.Current.UserAppTheme == AppTheme.Light;
            return (value as string) switch
            {
                "Автобус" => isLightTheme ? Colors.Red : Colors.Crimson,
                "Трамвай" => isLightTheme ? Colors.Orange : Colors.DarkOrange,
                "Тролей" => isLightTheme ? Colors.Blue : Colors.DarkBlue,
                _ => Colors.White,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
