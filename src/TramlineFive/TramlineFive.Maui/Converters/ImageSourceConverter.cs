using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TramlineFive.Converters
{
    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string prefix = (DeviceInfo.Platform == DevicePlatform.Android) ? "Resources/drawable" : "Assets";
            char[] type = (value as string).ToCharArray();

            // Shout-out to Atanas Semerdzhiev
            type[0] &= (char)0xDF;
            return $"{prefix}/{new string(type)}64.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
