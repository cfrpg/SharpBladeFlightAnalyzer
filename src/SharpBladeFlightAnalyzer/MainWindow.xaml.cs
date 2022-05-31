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
using System.Threading;


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

		LoadViewModel lvm;

		bool isloading;

		public MainWindow()
		{
			InitializeComponent();
			fieldListWindow = new FieldListWindow();
			fieldListWindow.okBtn.Click += OkBtn_Click;
			fieldListWindow.messageList.MouseDoubleClick += MessageList_MouseDoubleClick;
			fieldListWindow.exportBtn.Click += ExportBtn_Click;
			fieldConfigs = new Dictionary<string, FieldConfig>();
			FileInfo fi = new FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + "config\\Fields.csv");
			if(fi.Exists)
			{
				StreamReader sr = new StreamReader(System.AppDomain.CurrentDomain.BaseDirectory + "config\\Fields.csv");
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

			isloading = false;
			lvm = new LoadViewModel();
			
			lvm.Visibility = Visibility.Hidden;
			mainTabControl.DataContext = lvm;

			//ULogFile f = new ULogFile();
			//f.Load("D:\\temp\\log.ulg");
			//LogPageControl lpc = new LogPageControl(f, this);
			//lpc.addFieldBtn.Click += AddFieldBtn_Click;
			//testPage.Content = lpc;
			//currentPage = (LogPageControl)((TabPage)mainTabControl.Items[0]).Content;
		}

		private void ExportBtn_Click(object sender, RoutedEventArgs e)
		{
			if (fieldListWindow.messageList.SelectedItem == null)
				return;
			MessageViewModel mvm = ((MessageViewModel)(fieldListWindow.messageList.SelectedItem));
			System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
			sfd.Filter = "csv files (*.csv)|*.csv";
			//sfd.AddExtension = true;
			if(mvm.IsMassage)
			{
				//export message
				Message msg = mvm.Message;
				sfd.FileName = msg.Name;
				if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
					return;
				StreamWriter sw = new StreamWriter(sfd.FileName, false);
				//creat header
				sw.Write("Time,");
				foreach (var v in msg.FieldDict)
				{
					if (v.Value.Values.Count == 0)
						continue;
					sw.Write(v.Value.Name);
					sw.Write(",");
				}
				sw.WriteLine();
				//write data
				for(int i=0;i<msg.TimeStamps.Count;i++)
				{
					sw.Write(msg.TimeStamps[i]);
					sw.Write(",");
					foreach(var v in msg.FieldDict)
					{
						if (v.Value.Values.Count == 0)
							continue;
						sw.Write(v.Value.Values[i]);
						sw.Write(",");
					}
					sw.WriteLine();
				}
				sw.Close();
			}
			else
			{
				//export data field
				DataField df = mvm.DataField;
				sfd.FileName = df.Topic.Name+"."+df.Name;
				if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
					return;
				StreamWriter sw = new StreamWriter(sfd.FileName, false);
				//creat header				
				
				sw.Write("Time,");
				sw.Write(df.Name);
				sw.WriteLine(",");
				//write data
				for (int i = 0; i < df.Timestamps.Count; i++)
				{
					sw.Write(df.Timestamps[i]);
					sw.Write(",");					
					sw.Write(df.Values[i]);
					sw.WriteLine(",");
				}
				sw.Close();
			}
			ShowMessageBox("导出成功!");
		}

		private void MessageList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (fieldListWindow.messageList.SelectedItem != null)
			{
				if (!((MessageViewModel)(fieldListWindow.messageList.SelectedItem)).IsMassage)
					currentPage.AddLine(((MessageViewModel)(fieldListWindow.messageList.SelectedItem)).DataField);
			}
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
			if (fieldListWindow.messageList.SelectedItem != null)
			{
				if(!((MessageViewModel)(fieldListWindow.messageList.SelectedItem)).IsMassage)
					currentPage.AddLine(((MessageViewModel)(fieldListWindow.messageList.SelectedItem)).DataField);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			fieldListWindow.needClose = true;
			fieldListWindow.Close();
		}

		private void setFieldList()
		{
			//fieldListWindow.fieldList.ItemsSource = currentPage.LogFile.DataFields;
			fieldListWindow.messageList.ItemsSource = currentPage.LogFile.MessageList;			
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
			ofd.Filter = "ulog日志文件 (*.ulg)|*.ulg|All files (*.*)|*.*";
			if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				//loadFile(ofd.FileName);	
				startLoadFile(ofd.FileName);			
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
				//fieldListWindow.fieldList.ItemsSource = null;
				fieldListWindow.messageList.ItemsSource = null;
			}
		}

		private void ShowMessageBox(string str)
		{
			MessageBox.Show(str, "SharpBladeFlightAnalyzer");
		}

		private void Window_Drop(object sender, DragEventArgs e)
		{
			if (isloading)
				return;
			if(e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				
				startLoadFile(files[0]);
				
			}
		}

		private void loadFile(string path)
		{			
			ULogFile f = new ULogFile();
			if (f.Load(path, fieldConfigs))
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

		private void startLoadFile(string path)
		{
			if (isloading)
				return;
			isloading = true;
			Thread uiThread = new Thread(new ParameterizedThreadStart(updateUI));
			uiThread.IsBackground = true;
			uiThread.Start(path);
			
		}

		private void updateUI(object o)
		{
			string path = (string)o;
			Thread loadThread= new Thread(new ParameterizedThreadStart(loadingFile));
			loadThread.IsBackground = true;
			lvm.CurrProgress = 0;
			lvm.Visibility = Visibility.Visible;
			ULogFile f = new ULogFile();
			loadThread.Start(new Tuple<ULogFile, string>(f, path));
			Thread.Sleep(20);
			lvm.MaxProgress = f.TotalSize;
			while (true)
			{
				lvm.CurrProgress = f.CurrPos;
				Thread.Sleep(20);
				if (f.ReadCompleted)
					break;
			}
			lvm.Visibility = Visibility.Hidden;
			Dispatcher.Invoke(new Action(delegate ()
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
			}));
			isloading = false;
		}

		private void loadingFile(object o)
		{
			Tuple<ULogFile, string> args = (Tuple<ULogFile, string>)o;
			ULogFile f = args.Item1;
			bool res = f.Load(args.Item2, fieldConfigs);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			string[] args = Environment.GetCommandLineArgs();
			if (args.Length > 1)
			{
				startLoadFile(args[1]);
			}
		}
	}
}
