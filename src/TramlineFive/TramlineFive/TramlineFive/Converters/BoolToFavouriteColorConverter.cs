﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace TramlineFive.Converters;

public class BoolToFavouriteColorConverter : IValueConverter
{
    private string selectedColor = "#1e90ff";
    private string notSelectedColor = "#eae9f0";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
            return Color.FromHex(selectedColor);

        return Color.FromHex(notSelectedColor);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
