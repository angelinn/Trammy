using Microsoft.Maui.ApplicationModel;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TramlineFive.Common.Services;


namespace TramlineFive.Converters
{
    public class TransportTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TransportType type)
                return Color.FromArgb(TransportConvertеr.TypeToColor(type, Application.Current.RequestedTheme == AppTheme.Light));

            return TransportConvertеr.DEFAULT_COLOR;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
