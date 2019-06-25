using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Media;
using InteractiveDataDisplay.WPF;

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
		LineGraph line;
		DataField rawData;

		double[] xValues;
		double[] yValues;

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
				if (lpf < 0)
					lpf = 0;
				if (lpf > 1)
					lpf = 1;
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
				line.Stroke = new SolidColorBrush(color);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color"));
			}
		}

		public LineGraph Line
		{
			get { return line; }
			set { line = value; }
		}

		public Polar(DataField d, Color c)
		{
			line = new LineGraph();
			rawData = d;
			XOffset = 0;
			YOffset = 0;
			Scale = 0;
			Lpf = 0;
			Name = d.Name;
			Color = c;
			line.StrokeThickness = 1;			
			RefreshPolar();
		}

		public void RefreshPolar()
		{
			xValues = rawData.Timestamps.Select(v => v + xOffset).ToArray();
			if (yValues == null)
				yValues = new double[rawData.Values.Count];
			yValues[0] = (rawData.Values[0] + yOffset) * scale;
			for (int i = 1; i < yValues.Length; i++)
			{
				yValues[i] = lpf * yValues[i - 1] + (1 - lpf) * (rawData.Values[0] + yOffset) * scale;
			}
			line.Plot(xValues, yValues);
		}
	}
}
