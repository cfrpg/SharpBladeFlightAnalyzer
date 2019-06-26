using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;

namespace SharpBladeFlightAnalyzer
{
	public class ColorBrushConvert : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			System.Windows.Media.Color c = (System.Windows.Media.Color)value;
			return new System.Windows.Media.SolidColorBrush(c);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var b = (System.Windows.Media.SolidColorBrush)value;
			return b.Color;
		}
	}
}
