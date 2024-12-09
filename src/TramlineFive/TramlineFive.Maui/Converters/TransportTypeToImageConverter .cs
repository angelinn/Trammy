using Microsoft.Maui.ApplicationModel;
using SkgtService;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;


namespace TramlineFive.Converters;

public class TransportTypeToImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string textValue = string.Empty;
        
        if (value is string)
            textValue = value as string;

        StopInformation stop = ServiceContainer.ServiceProvider
            .GetService<PublicTransport>()
            .FindStop(textValue);

        if (stop == null)
            return ImageSource.FromFile("bus_icon.svg");

        return stop.Type switch
        {
            TransportType.Bus => ImageSource.FromFile("bus_icon.svg"),
            TransportType.Tram => ImageSource.FromFile("tram_icon.svg"),
            TransportType.Trolley => ImageSource.FromFile("trolleyBus_icon.svg"),
            TransportType.Subway => ImageSource.FromFile("subway_icon.svg"),
            _ => ImageSource.FromFile("bus_icon.svg")
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
