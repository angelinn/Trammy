using Plugin.Maui.BottomSheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TramlineFive.Common.ViewModels.MapViewModel;

namespace TramlineFive.Converters;

public class BottomSheetStateToIntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (BottomSheetState)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (SheetState)value;
    }
}
