using SkgtService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TramlineFive.Common.Services;

namespace TramlineFive.Converters
{
    public class CodeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string code = value as string;
            if (parameter is string type && type == "last")
                code = (value as List<string>)[^1];

            if (code == null)
                return code;

            return ServiceContainer.ServiceProvider.GetService<PublicTransport>().FindStop(code).PublicName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
