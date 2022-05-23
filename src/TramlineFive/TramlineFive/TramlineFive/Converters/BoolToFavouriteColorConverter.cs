using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace TramlineFive.Converters;

public class BoolToFavouriteColorConverter : IValueConverter
{
    private string selectedColor = "#1e90ff";
    private string notSelectedColor = "#eae9f0";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        ResourceDictionary theme = Application.Current.Resources.MergedDictionaries.FirstOrDefault(i => i.ContainsKey("MenuTextColor"));

        Color notSelected = Color.FromHex(notSelectedColor);
        Color selected = Color.FromHex(selectedColor);
        if (theme != null)
        {
            selected = (Color)theme["IconsColor"];
        }

        if ((bool)value)
            return selected;

        return notSelected;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
