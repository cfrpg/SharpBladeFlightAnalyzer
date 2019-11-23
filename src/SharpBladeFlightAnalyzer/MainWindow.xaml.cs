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
using System.IO;


namespace SharpBladeFlightAnalyzer
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		LogPageControl currentPage;

		FieldListWindow fieldListWindow;

		Dictionary<string, FieldConfig> fieldConfigs;

		public MainWindow()
		{
			InitializeComponent();
			fieldListWindow = new FieldListWindow();
			fieldListWindow.okBtn.Click += OkBtn_Click;
			fieldConfigs = new Dictionary<string, FieldConfig>();
			FileInfo fi = new FileInfo(Environment.CurrentDirectory + "\\config\\Fields.csv");
			if(fi.Exists)
			{
				StreamReader sr = new StreamReader(Environment.CurrentDirectory + "\\config\\Fields.csv");
				sr.ReadLine();
				while(!sr.EndOfStream)
				{
					string line = sr.ReadLine();
					string[] col = line.Split(',');
					if (col.Length < 4)
						continue;
					fieldConfigs.Add(col[0], new FieldConfig() { Name = col[0], ShortName = col[1], Description = col[2], Enable = col[3] == "1" });
				}
				sr.Close();
			}

			//ULogFile f = new ULogFile();
			//f.Load("D:\\temp\\log.ulg");
			//LogPageControl lpc = new LogPageControl(f, this);
			//lpc.addFieldBtn.Click += AddFieldBtn_Click;
			//testPage.Content = lpc;
			//currentPage = (LogPageControl)((TabPage)mainTabControl.Items[0]).Content;
		}

		private void AddFieldBtn_Click(object sender, RoutedEventArgs e)
		{
			setFieldList();
			fieldListWindow.Topmost = true;
			fieldListWindow.Show();
			fieldListWindow.Topmost = false;
		}

		private void OkBtn_Click(object sender, RoutedEventArgs e)
		{
			if (fieldListWindow.fieldList.SelectedItem != null)
			{
				currentPage.AddLine((DataField)fieldListWindow.fieldList.SelectedItem);
			}
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

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
			if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				string path = ofd.FileName;
				ULogFile f = new ULogFile();
				if (f.Load(path,fieldConfigs))
				{
					LogPageControl lpc = new LogPageControl(f, this);
					lpc.addFieldBtn.Click += AddFieldBtn_Click;
					TabPage page = new TabPage();
					page.Header = f.File.Name;

					page.Content = lpc;
					page.DisposableContent = f;
					mainTabControl.Items.Insert(mainTabControl.Items.Count - 1, page);
					mainTabControl.SelectedIndex = mainTabControl.Items.Count - 2;
					currentPage = lpc;
				}
			}
		}

		private void mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (mainTabControl.SelectedIndex != mainTabControl.Items.Count - 1 && mainTabControl.SelectedIndex != -1)
			{
				currentPage = (LogPageControl)((TabPage)mainTabControl.SelectedItem).Content;
				setFieldList();
			}
			else
			{
				currentPage = null;
				fieldListWindow.fieldList.ItemsSource = null;
			}
		}
	}
}
