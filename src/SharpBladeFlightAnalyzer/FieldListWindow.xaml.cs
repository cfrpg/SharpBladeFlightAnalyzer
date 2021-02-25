using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SharpBladeFlightAnalyzer
{
	/// <summary>
	/// FieldListWindow.xaml 的交互逻辑
	/// </summary>
	public partial class FieldListWindow : Window
	{
		public bool needClose;
		public FieldListWindow()
		{
			InitializeComponent();
			needClose = false;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!needClose)
			{
				this.Hide();
				e.Cancel = true;
			}
		}

		private void okBtn_Click(object sender, RoutedEventArgs e)
		{

		}

		private void closeBtn_Click(object sender, RoutedEventArgs e)
		{
			this.Hide();
		}

		private void messageList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			
		}
	}
}
