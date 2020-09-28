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

		private void exportAcmiBtn_Click(object sender, RoutedEventArgs e)
		{
			if ((!logFile.FieldDict.ContainsKey("vehicle_global_position.lon")) ||
				(!logFile.FieldDict.ContainsKey("vehicle_global_position.lat")) ||
				(!logFile.FieldDict.ContainsKey("vehicle_global_position.alt")) ||
				(!logFile.FieldDict.ContainsKey("vehicle_attitude.roll")) ||
				(!logFile.FieldDict.ContainsKey("vehicle_attitude.pitch")) ||
				(!logFile.FieldDict.ContainsKey("vehicle_attitude.yaw")))
			{
				MessageBox.Show("数据不全");
				return;
			}
			DataField[] posData = new DataField[3];
			DataField[] attData = new DataField[3];
			posData[0] = logFile.FieldDict["vehicle_global_position.lon"];
			posData[1] = logFile.FieldDict["vehicle_global_position.lat"];
			posData[2] = logFile.FieldDict["vehicle_global_position.alt"];
			attData[0] = logFile.FieldDict["vehicle_attitude.roll"];
			attData[1] = logFile.FieldDict["vehicle_attitude.pitch"];
			attData[2] = logFile.FieldDict["vehicle_attitude.yaw"];
			double[] attFilter = new double[3];

			System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
			sfd.AddExtension = true;
			sfd.Filter = "acmi text files(*.acmi)|*.acmi";
			if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				StreamWriter sw = new StreamWriter(sfd.FileName, false);
				sw.WriteLine("FileType=text/acmi/tacview");
				sw.WriteLine("FileVersion=2.1");
				sw.WriteLine("0,ReferenceTime=2020-09-01T05:00:00Z");
				sw.WriteLine("101,Name=PX4,Type=Air+FixedWing");
				double currtime = -1;
				double time;
				double hdg;
				int pospos = 0, attpos = 0;
				var posTimes = posData[0].Timestamps;
				var attTimes = attData[0].Timestamps;
				sw.WriteLine("#{0}",Math.Min(posTimes[0],attTimes[0]));
				sw.WriteLine("101,T={0}|{1}|{2}", posData[0].Values[0], posData[1].Values[0], posData[2].Values[0]);
				hdg = attData[2].Values[0] * 57.29577951;
				while (pospos < posTimes.Count || attpos < attTimes.Count)
				{
					int flag = 0;
					if(pospos < posTimes.Count && attpos < attTimes.Count)
					{
						if(posTimes[pospos]< attTimes[attpos])
						{
							flag = 1;
						}
						else
						{
							flag = 2;
						}
					}
					else if(pospos < posTimes.Count)
					{
						flag = 1;
					}
					else
					{
						flag = 2;
					}
					if(flag==1)
					{
						//Update pos
						sw.WriteLine("#" + posTimes[pospos].ToString());
					
						sw.WriteLine("101,T={0}|{1}|{2}|{3}|{4}|{5}", posData[0].Values[pospos], posData[1].Values[pospos], posData[2].Values[pospos], attFilter[0] * 57.29577951, attFilter[1] * 57.29577951, hdg);
						pospos++;
					}
					else
					{

						for (int i = 0; i < 2; i++)
						{
							if (attpos == 0)
								attFilter[i] = attData[i].Values[attpos];
							else
							{
								attFilter[i] = 0.95 * attFilter[i] + 0.05 * attData[i].Values[attpos];
							}
						}
						hdg = attData[2].Values[attpos] * 57.29577951;
						if (hdg < 0)
							hdg += 360;
						//if (attTimes[attpos] - currtime >= 0.1)
						//{
						//	sw.WriteLine("#" + attTimes[attpos].ToString());

						//	sw.WriteLine("101,T=|||{0}|{1}|{2}", attFilter[0] * 57.29577951, attFilter[1] * 57.29577951, hdg);
						//	currtime = attTimes[attpos];
						//}
						
						attpos++;
					}
				}
				sw.Close();
			}
		}
	}
}
