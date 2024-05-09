using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TramlineFive.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        private string selectedColor = "#1e90ff";
        private string notSelectedColor = "#717578";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ResourceDictionary theme = Application.Current.Resources.MergedDictionaries.FirstOrDefault(i => i.ContainsKey("MenuTextColor"));

            Color notSelected = Color.FromArgb(notSelectedColor); 
            Color selected = Color.FromArgb(selectedColor);
            if (theme != null)
            {
                notSelected = (Color)theme["MenuTextColor"];
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
}
