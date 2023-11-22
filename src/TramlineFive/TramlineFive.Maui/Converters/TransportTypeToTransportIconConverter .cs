using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;


namespace TramlineFive.Converters;

public class TransportTypeToTransportIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value as string) switch
        {
            "bus" => "MTS_Bus_icon.svg",
            "tram" => "MTS_Tram_icon.svg",
            "trolley" => "MTS_Bus_icon_trolley.svg",
            _ => "MTS_Tram_icon.svg",
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
