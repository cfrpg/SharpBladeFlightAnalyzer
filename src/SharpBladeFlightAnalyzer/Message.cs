using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeFlightAnalyzer
{
	public class Message:IDisposable
	{
		int id;
		string name;

		MessageFormat format;
		//field name,data
		Dictionary<string, DataField> fieldDict;
		List<double> timeStamps;
		double timeDivider;

		public string Name { get => name; set => name = value; }		
		public Dictionary<string, DataField> FieldDict { get => fieldDict; set => fieldDict = value; }
		public double TimeDivider { get => timeDivider; set => timeDivider = value; }
		public int ID { get => id; set => id = value; }
		public MessageFormat Format { get => format; set => format = value; }
		public List<double> TimeStamps { get => timeStamps; set => timeStamps = value; }

		public Message(int i,string n,MessageFormat f)
		{
			id = i;
			name = n;
			format = f;
			fieldDict = new Dictionary<string, DataField>();
			TimeStamps = new List<double>();
			timeDivider = 1e6;
			foreach(var v in format.FieldList)
			{
				fieldDict.Add(v.Item2, new DataField(v.Item2, v.Item3, this));
			}
		}

		public void Dispose()
		{
			TimeStamps.Clear();
			TimeStamps.TrimExcess();
			foreach(var v in fieldDict)
			{
				v.Value.Dispose();
			}
			fieldDict.Clear();
		}


	}
}
