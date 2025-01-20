﻿using System.Globalization;
using Avalonia.Data.Converters;
using YMCL.Public.Classes;

namespace YMCL.Public.Module.Ui.Converter;

public class IndexIsTwoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var index = (int)value;
        return index == 2;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return false;
    }
}