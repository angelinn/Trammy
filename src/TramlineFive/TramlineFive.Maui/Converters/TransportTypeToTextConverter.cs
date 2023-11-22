using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;


namespace TramlineFive.Converters;

public class TransportTypeToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value as string) switch
        {
            "bus" => "Автобус",
            "tram" => "Твамвай",
            "trolley" => "Тролей",
            _ => "Транспорт",
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
