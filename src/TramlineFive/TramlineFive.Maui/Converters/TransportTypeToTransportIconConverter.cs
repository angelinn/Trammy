﻿using Microsoft.Maui.ApplicationModel;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;


namespace TramlineFive.Converters;

public class TransportTypeToTransportIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string textValue = string.Empty;
        if (value is TransportType type)
        {
            textValue = type switch
            {
                TransportType.Tram => "tram",
                TransportType.Trolley => "trolley",
                _ => "bus"
            }; ;
        }

        if (value is string)
            textValue = value as string;

        return (textValue as string) switch
        {
            "bus" => "directions_bus",
            "tram" => "tram",
            "trolley" => "directions_bus",
            _ => "tram",
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
