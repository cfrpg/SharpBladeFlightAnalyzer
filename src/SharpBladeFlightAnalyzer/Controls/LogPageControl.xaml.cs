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

namespace SharpBladeFlightAnalyzer
{
	/// <summary>
	/// LogPageControl.xaml 的交互逻辑
	/// </summary>
	public partial class LogPageControl : UserControl
	{
		ULogFile logFile;
		
		public LogPageControl(ULogFile uf)
		{
			InitializeComponent();
			logFile = uf;
			paramList.ItemsSource = logFile.Parameters;
			msgList.ItemsSource = logFile.Messages;
			propList.ItemsSource = logFile.Infomations;
		}

		public ULogFile LogFile
		{
			get { return logFile; }
			set { logFile = value; }
		}
	}
}
