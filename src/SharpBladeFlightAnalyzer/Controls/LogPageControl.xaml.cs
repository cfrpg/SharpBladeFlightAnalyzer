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

		bool update;

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

			update = false;
			
		}
		
		public void AddLine(string name)
		{
			AddLine(logFile.FieldDict[name]);
		}

		public void AddLine(DataField df)
		{
			currentGraph.Polars.Add(new Polar(df, Colors.Red));
			refreshGraph();
			update = true;
			//var t = Task.Run(async delegate { await Task.Delay(100); currentGraph.TakeSnapShot(mainChart); });
			//currentGraph.TakeSnapShot(mainChart);
			Dispatcher.BeginInvoke(new Action(() => currentGraph.TakeSnapShot(mainChart)), System.Windows.Threading.DispatcherPriority.ContextIdle, null);
		}
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			
		}

		private void newGraphBtn_Click(object sender, RoutedEventArgs e)
		{
			Graph g = new Graph();
			g.TakeSnapShot(mainChart);
			graphs.Insert(graphs.Count - 1, g);
			graphList.SelectedIndex = graphs.Count - 2;
			setCurrentGraph(g);
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
		}

		private void refreshGraph()
		{
			Lines.Children.Clear();
			foreach (var v in currentGraph.Polars)
			{				
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
	}
}
