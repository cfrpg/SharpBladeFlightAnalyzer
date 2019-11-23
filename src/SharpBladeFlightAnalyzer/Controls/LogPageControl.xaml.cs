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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.IO;


namespace SharpBladeFlightAnalyzer
{
	/// <summary>
	/// LogPageControl.xaml 的交互逻辑
	/// </summary>
	public partial class LogPageControl : UserControl
	{
		MainWindow mainWindow;
		ULogFile logFile;

		ObservableCollection<Graph> graphs;

		Graph currentGraph;		

		public ULogFile LogFile
		{
			get { return logFile; }
			set { logFile = value; }
		}

		public MainWindow MainWindow
		{
			get { return mainWindow; }
			set { mainWindow = value; }
		}

		public LogPageControl(ULogFile uf,MainWindow w)
		{
			InitializeComponent();
			logFile = uf;
			paramList.ItemsSource = logFile.Parameters;
			msgList.ItemsSource = logFile.Messages;
			propList.ItemsSource = logFile.Infomations;

			graphs = new ObservableCollection<Graph>();
			graphs.Add(new BlankGraph());
			graphList.ItemsSource = graphs;

			mainWindow = w;

			Graph g = new Graph();
			graphs.Insert(graphs.Count - 1, g);
			graphList.SelectedIndex = graphs.Count - 2;
			setCurrentGraph(g);
			Dispatcher.BeginInvoke(new Action(() => currentGraph.TakeSnapShot(mainChart)), System.Windows.Threading.DispatcherPriority.ContextIdle, null);


		}
		
		public void AddLine(string name)
		{
			AddLine(logFile.FieldDict[name]);
		}

		public void AddLine(DataField df)
		{
			if (currentGraph == null)
				return;
			Polar p = new Polar(df, randomColor());
			p.OnPolarChanged += OnPolarChanged;
			currentGraph.Polars.Add(p);
			OnPolarChanged();
		}

		private void OnPolarChanged()
		{
			refreshGraph();
			Dispatcher.BeginInvoke(new Action(() => currentGraph.TakeSnapShot(mainChart)), System.Windows.Threading.DispatcherPriority.ContextIdle, null);
		}

		private Color randomColor()
		{
			Random r = new Random();
			return Color.FromRgb((byte)r.Next(256), (byte)r.Next(256), (byte)r.Next(256));
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			
		}

		private void newGraphBtn_Click(object sender, RoutedEventArgs e)
		{
			Graph g = new Graph();			
			graphs.Insert(graphs.Count - 1, g);
			graphList.SelectedIndex = graphs.Count - 2;
			setCurrentGraph(g);
			Dispatcher.BeginInvoke(new Action(() => currentGraph.TakeSnapShot(mainChart)), System.Windows.Threading.DispatcherPriority.ContextIdle, null);
		}

		private void closeGraphBtn_Click(object sender, RoutedEventArgs e)
		{
			Graph g = (Graph)graphList.SelectedItem;
			
			graphs.Remove(g);
		}

		private void setCurrentGraph(Graph g)
		{
			currentGraph = g;
			refreshGraph();
			if (currentGraph == null)
				polarListView.ItemsSource = null;
			else
				polarListView.ItemsSource = currentGraph.Polars;
		}

		private void refreshGraph()
		{
			Lines.Children.Clear();
			if(currentGraph!=null)
				foreach (var v in currentGraph.Polars)
				{				
					if(v.Visible)
						Lines.Children.Add(v.Line);
				}			
		}

		private void graphList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!(graphList.SelectedItem is BlankGraph))
			{
				setCurrentGraph((Graph)graphList.SelectedItem);
			}
		}

		private void mainChart_LayoutUpdated(object sender, EventArgs e)
		{
			
		}

		private void addFieldBtn_Click(object sender, RoutedEventArgs e)
		{

		}

		private void polarListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			polarGrid.DataContext = polarListView.SelectedItem;
		}

		private void colorBtn_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
			if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				System.Drawing.Color c = cd.Color;
				((Polar)polarListView.SelectedItem).Color= Color.FromArgb(c.A, c.R, c.G, c.B);
			}
		}

		private void removeFieldBtn_Click(object sender, RoutedEventArgs e)
		{
			if (polarListView.SelectedItem == null)
				return;
			Polar p = (Polar)polarListView.SelectedItem;
			currentGraph.Polars.Remove(p);
			OnPolarChanged();
		}

		private void exportFieldBtn_Click(object sender, RoutedEventArgs e)
		{
			if (polarListView.SelectedItem == null)
				return;
			System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
			sfd.AddExtension = true;
			sfd.Filter = "csv files(*.csv)|*.csv";			
			if(sfd.ShowDialog()== System.Windows.Forms.DialogResult.OK)
			{
				Polar p = (Polar)polarListView.SelectedItem;
				StreamWriter sw = new StreamWriter(sfd.FileName, false);
				sw.WriteLine("Figure,,Raw,,");
				sw.WriteLine("{0},{1},{2},{3},", "Time", p.Name, "Time", p.Name);
				for(int i=0;i<p.RawData.Values.Count;i++)
				{
					sw.WriteLine("{0},{1},{2},{3},", p.XValues[i], p.YValues[i], p.RawData.Timestamps[i], p.RawData.Values[i]);
				}
				sw.Close();
				
			}
			

		}

		private void exportGraphBtn_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
			sfd.AddExtension = true;
			sfd.Filter = "csv files(*.csv)|*.csv";
			if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				StreamWriter sw = new StreamWriter(sfd.FileName, false);
				foreach (var p in currentGraph.Polars)
				{
					if(p.Visible)
					{
						sw.Write("Time,{0},", p.Name);
					}
				}
				sw.WriteLine();
				bool flag = true;
				int cnt = 0;
				while (flag)
				{
					flag = false;
					foreach(var p in currentGraph.Polars)
					{
						if(p.Visible)
						{
							if(p.XValues.Length>cnt)
							{
								sw.Write("{0},{1},", p.XValues[cnt], p.YValues[cnt]);
								flag = true;
							}
							else
							{
								sw.Write(",,");
							}
						}
					}
					cnt++;
					sw.WriteLine();
				}
				sw.Close();

			}
		}
	}
}
