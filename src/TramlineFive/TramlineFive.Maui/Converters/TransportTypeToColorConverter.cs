using Microsoft.Maui.ApplicationModel;
using SkgtService.Models;
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
            bool isLightTheme = Application.Current.RequestedTheme == AppTheme.Light;

            string textValue = string.Empty;
            if (value is TransportType type)
            {
                textValue = type switch
                {
                    TransportType.Tram => "tram",
                    TransportType.Trolley => "trolley",
                    TransportType.Subway => "subway",
                    _ => "bus"
                }; ;
            }

            if (value is string)
                textValue = value as string;

            return (textValue as string) switch
            {
                "Автобус" => isLightTheme ? Colors.Red : Colors.Crimson,
                "Трамвай" => isLightTheme ? Colors.Orange : Colors.DarkOrange,
                "Тролей" => isLightTheme ? Color.FromArgb("2aa9e0") : Colors.DarkBlue,
                "Електробус" =>  isLightTheme ? Colors.Green : Colors.DarkGreen,
                "Допълнителна" => isLightTheme ? Color.FromArgb("13b79d") : Color.FromArgb("13b79d"),
                "Метро" => Colors.Blue,
                "bus" => Colors.Crimson,
                "tram" => Colors.DarkOrange,
                "trolley" => Colors.RoyalBlue,
                _ => Colors.White,
            };

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
