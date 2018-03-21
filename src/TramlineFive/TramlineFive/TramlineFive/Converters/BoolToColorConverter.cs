using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace TramlineFive.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        private string selectedColor = "#4CACE8";
        private string notSelectedColor = "#808080";

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
}
