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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace SharpBladeFlightAnalyzer
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		LogPageControl currentPage;

		FieldListWindow fieldListWindow;

		public MainWindow()
		{
			InitializeComponent();
			fieldListWindow = new FieldListWindow();
			fieldListWindow.okBtn.Click += OkBtn_Click;

			ULogFile f = new ULogFile();
			f.Load("D:\\temp\\log.ulg", 1024);
			LogPageControl lpc = new LogPageControl(f);
			testPage.Content = lpc;
			currentPage = (LogPageControl)((TabPage)mainTabControl.Items[0]).Content;
		}

		private void OkBtn_Click(object sender, RoutedEventArgs e)
		{
			
		}

		private void addFieldBtn_Click(object sender, RoutedEventArgs e)
		{
			setFieldList();
			fieldListWindow.Topmost = true;
			fieldListWindow.Show();
			fieldListWindow.Topmost = false;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			fieldListWindow.needClose = true;
			fieldListWindow.Close();
		}

		private void setFieldList()
		{
			fieldListWindow.fieldList.ItemsSource = currentPage.LogFile.DataFields;
			
		}
	}
}
