using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SharpBladeFlightAnalyzer
{
	public class LoadViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		double maxProgress;
		double currProgress;

		Visibility visibility;


		public double MaxProgress 
		{ 
			get => maxProgress;
			set
			{
				maxProgress = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaxProgress"));
			}
		}
		public double CurrProgress 
		{ 
			get => currProgress;
			set
			{
				currProgress = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrProgress"));
			}
		}

		public Visibility Visibility
		{
			get => visibility;
			set
			{
				visibility = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Visibility"));
			}
		}
	}
}
