using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using InteractiveDataDisplay.WPF;

namespace SharpBladeFlightAnalyzer
{
	public class Graph : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		List<Polar> polars;
		RenderTargetBitmap thumb;

		public List<Polar> Polars
		{
			get { return polars; }
			set { polars = value; }
		}

		public RenderTargetBitmap Thumb
		{
			get { return thumb; }
			set
			{
				thumb = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Thumb"));
			}
		}

		public Graph()
		{
			polars = new List<Polar>();			
		}

		public void TakeSnapShot(FrameworkElement cam)
		{
			if (Thumb == null)
			{
				Thumb = new RenderTargetBitmap((int)cam.ActualWidth, (int)cam.ActualHeight, 96, 96, PixelFormats.Default);
			}			
			Thumb.Render(cam);			
		}
	}
}
