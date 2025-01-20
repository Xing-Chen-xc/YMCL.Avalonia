﻿using System.Globalization;
using Avalonia.Data.Converters;

namespace YMCL.Public.Module.Ui.Converter;

public class MaxThreadOverSafe: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double intValue)
        {
            return intValue > 64;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return false;
    }
}