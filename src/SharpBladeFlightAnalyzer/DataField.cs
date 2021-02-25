using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeFlightAnalyzer
{
	public class DataField:IDisposable
	{
		string name;
		string dispName;
		string description;
		SpecialField flag;
		Message topic;
		
		//List<Tuple<double, double>> data;

		List<double> timestamps;
		List<double> values;

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		//public List<Tuple<double, double>> Data
		//{
		//	get { return data; }
		//	set { data = value; }
		//}

		public string Description
		{
			get { return description; }
			set { description = value; }
		}

		public SpecialField Flag
		{
			get { return flag; }
			set { flag = value; }
		}

		public List<double> Timestamps
		{
			get { return timestamps; }
			set { timestamps = value; }
		}

		public List<double> Values
		{
			get { return values; }
			set { values = value; }
		}

		public string DispName
		{
			get { return dispName; }
			set { dispName = value; }
		}

		public Message Topic { get => topic; set => topic = value; }

		public DataField(string n,Message t):this(n,SpecialField.None,t)
		{
			
		}

		public DataField(string n,SpecialField sf,Message t)
		{
			name = n;
			dispName = t.Name+"."+n;
			//data = new List<Tuple<double, double>>();
			//timestamps = new List<double>();
			values = new List<double>();
			flag = sf;
			topic = t;
			timestamps = t.TimeStamps;
		}

		public void Dispose()
		{
			values.Clear();
			values.TrimExcess();
		}
	}

	public enum SpecialField
	{
		None,
		TimeStamp,
		Padding
	}
}
