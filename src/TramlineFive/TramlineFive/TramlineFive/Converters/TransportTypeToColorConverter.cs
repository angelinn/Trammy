using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace TramlineFive.Converters
{
    public class TransportTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isLightTheme = Application.Current.UserAppTheme == OSAppTheme.Light;
            return (value as string) switch
            {
                "Автобус" => isLightTheme ? Color.Red : Color.Crimson,
                "Трамвай" => isLightTheme ? Color.Orange : Color.DarkOrange,
                "Тролей" => isLightTheme ? Color.Blue : Color.DarkBlue,
                _ => Color.White,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
