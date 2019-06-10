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
	/// TabPage.xaml 的交互逻辑
	/// </summary>
	public partial class TabPage : TabItem
	{
		private TabControl parent;

		private double defaultWidth = 100;

		public TabPage()
		{
			InitializeComponent();
		}

		private void TabItem_Loaded(object sender, RoutedEventArgs e)
		{
			parent = findParent(this);
			if (parent != null)
				Init();
		}

		private void Init()
		{
			parent.SizeChanged += Parent_SizeChanged;
			parent.SelectionChanged += Parent_SelectionChanged;
			setWidth();
		}

		private void Parent_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			setWidth();
			e.Handled = true;
		}

		private void Parent_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			setWidth();
		}

		private void setWidth()
		{
			double w = (parent.ActualWidth - 20) / parent.Items.Count;			
			if (w > defaultWidth)
				w = defaultWidth;
			else		
				w = (parent.ActualWidth - 20 - (defaultWidth - w + 5)) / parent.Items.Count;				
			
			foreach(TabPage item in parent.Items)
			{
				if (item.IsSelected)
					item.Width = defaultWidth;
				else
					item.Width = w;
			}
		}

		private TabControl findParent(DependencyObject curr)
		{
			DependencyObject dobj = VisualTreeHelper.GetParent(curr);
			if (dobj == null)
				return null;
			if (dobj.GetType() == typeof(TabControl))
				return dobj as TabControl;
			return findParent(dobj);
		}

		private void btn_Close_Click(object sender, RoutedEventArgs e)
		{
			if (parent == null)
				return;
            if (!IsSelected)
                return;
			parent.Items.Remove(this);
			parent.SizeChanged -= Parent_SizeChanged;
			parent.SelectionChanged -= Parent_SelectionChanged;
			setWidth();
		}
	}
}
