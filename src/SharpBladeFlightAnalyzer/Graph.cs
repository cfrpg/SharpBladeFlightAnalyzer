using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
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
				PropertyChanged(this, new PropertyChangedEventArgs("Thumb"));
			}
		}


	}
}
