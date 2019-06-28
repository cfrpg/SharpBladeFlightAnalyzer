using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using InteractiveDataDisplay.WPF;

namespace SharpBladeFlightAnalyzer
{
	public class Polar : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public delegate void PolarChanged();
		public event PolarChanged OnPolarChanged;

		double xOffset;
		double yOffset;
		double scale;
		double lpf;
		string name;
		Color color;
		LineGraph line;
		DataField rawData;
		bool visible;

		double[] xValues;
		double[] yValues;

		public double XOffset
		{
			get { return xOffset; }
			set
			{
				xOffset = value;
				RefreshPolar();
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("XOffset"));
				OnPolarChanged?.Invoke();
			}
		}

		public double YOffset
		{
			get { return yOffset; }
			set
			{
				yOffset = value;
				RefreshPolar();
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("YOffset"));
				OnPolarChanged?.Invoke();
			}
		}

		public double Scale
		{
			get { return scale; }
			set
			{
				scale = value;
				RefreshPolar();
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Scale"));
				OnPolarChanged?.Invoke();
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
				RefreshPolar();
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Lpf"));
				OnPolarChanged?.Invoke();
			}
		}

		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
				OnPolarChanged?.Invoke();
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
				OnPolarChanged?.Invoke();
			}
		}

		public LineGraph Line
		{
			get { return line; }
			set { line = value; }
		}

		public bool Visible
		{
			get { return visible; }
			set
			{
				visible = value;
				line.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color"));
				OnPolarChanged?.Invoke();
			}
		}

		public DataField RawData
		{
			get { return rawData; }
		}

		public double[] XValues
		{
			get { return xValues; }
		}

		public double[] YValues
		{
			get { return yValues; }
		}

		public Polar(DataField d, Color c)
		{
			line = new LineGraph();
			visible = true;
			rawData = d;
			XOffset = 0;
			YOffset = 0;
			Scale = 1;
			Lpf = 0;
			Name = d.DispName;
			Color = c;
			line.StrokeThickness = 1;			
			RefreshPolar();
		}

		public void RefreshPolar()
		{
			xValues = RawData.Timestamps.Select(v => v + xOffset).ToArray();
			if (yValues == null)
				yValues = new double[RawData.Values.Count];
			yValues[0] = (RawData.Values[0] + yOffset) * scale;
			for (int i = 1; i < yValues.Length; i++)
			{
				yValues[i] = lpf * yValues[i - 1] + (1 - lpf) * (RawData.Values[i] + yOffset) * scale;
			}
			line.Plot(xValues, yValues);
		}
	}
}
