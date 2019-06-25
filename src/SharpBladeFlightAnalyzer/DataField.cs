using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeFlightAnalyzer
{
	public class DataField
	{
		string name;
		string description;
		SpecialField flag;
		
		List<Tuple<double, double>> data;

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public List<Tuple<double, double>> Data
		{
			get { return data; }
			set { data = value; }
		}

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

		public DataField(string n)
		{
			name = n;
			data = new List<Tuple<double, double>>();
			flag = SpecialField.None;
		}

		public DataField(string n,SpecialField sf)
		{
			name = n;
			data = new List<Tuple<double, double>>();
			flag = sf;
		}
	}

	public enum SpecialField
	{
		None,
		TimeStamp,
		Padding
	}
}
