using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Media;

namespace SharpBladeFlightAnalyzer
{
	public class Polar : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		double xOffset;
		double yOffset;
		double scale;
		double lpf;
		string name;
		Color color;

		public double XOffset
		{
			get { return xOffset; }
			set
			{
				xOffset = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("XOffset"));
			}
		}

		public double YOffset
		{
			get { return yOffset; }
			set
			{
				yOffset = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("YOffset"));
			}
		}

		public double Scale
		{
			get { return scale; }
			set
			{
				scale = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Scale"));
			}
		}

		public double Lpf
		{
			get { return lpf; }
			set
			{
				lpf = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Lpf"));
			}
		}

		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
			}
		}

		public Color Color
		{
			get { return color; }
			set
			{
				color = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color"));
			}
		}
	}
}
