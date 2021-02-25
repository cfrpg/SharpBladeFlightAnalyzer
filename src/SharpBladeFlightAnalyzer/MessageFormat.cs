using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeFlightAnalyzer
{
	public class MessageFormat
	{
		string name;
		//type,name,spec
		List<Tuple<string, string, SpecialField>> fieldList;

		bool allElementFlag;
		private List<int> subscribedID;

		public string Name { get => name; set => name = value; }
		public List<Tuple<string, string, SpecialField>> FieldList { get => fieldList; set => fieldList = value; }
		public bool AllElementFlag { get => allElementFlag; set => allElementFlag = value; }
		public List<int> SubscribedID { get => subscribedID; set => subscribedID = value; }

		

		public MessageFormat(string defstr)
		{
			int msgNameLen = defstr.IndexOf(":");
			name = defstr.Substring(0, msgNameLen);
			defstr = defstr.Substring(msgNameLen + 1);
			subscribedID = new List<int>();
			string[] fieldNames = defstr.Split(';');
			fieldList = new List<Tuple<string, string, SpecialField>>();
			string tstr;
			for (int i = 0; i < fieldNames.Length; i++)
			{
				if (fieldNames[i].Length == 0)
					continue;
				string[] field = fieldNames[i].Split(' ');
				if (field[0].IndexOf('[') > 0)
				{
					string realType = field[0];
					int len = getArrayLength(ref realType);
					for (int j = 0; j < len; j++)
					{
						tstr = field[1] + "[" + j.ToString() + "]";
						if (tstr.IndexOf("_padding") >= 0)
							fieldList.Add(new Tuple<string, string, SpecialField>(realType, tstr, SpecialField.Padding));							
						else if (tstr=="timestamp")
							fieldList.Add(new Tuple<string, string, SpecialField>(realType, tstr, SpecialField.TimeStamp));						
						else
							fieldList.Add(new Tuple<string, string, SpecialField>(realType, tstr, SpecialField.None));						
					}
				}
				else
				{
					tstr = field[1];
					if (tstr.IndexOf("_padding") >= 0)
						fieldList.Add(new Tuple<string, string, SpecialField>(field[0], tstr, SpecialField.Padding));
					else if (tstr=="timestamp")
						fieldList.Add(new Tuple<string, string, SpecialField>(field[0], tstr, SpecialField.TimeStamp));
					else
						fieldList.Add(new Tuple<string, string, SpecialField>(field[0], tstr, SpecialField.None));
				}
			}
			CheckElementType();
		}

		public bool CheckElementType()
		{
			allElementFlag = true;
			foreach (var v in fieldList)
			{
				if (!ULogFile.TypeSize.ContainsKey(v.Item1))
				{
					allElementFlag = false;
					break;
				}
			}
			return allElementFlag;
		}

		private int getArrayLength(ref string name)
		{
			int pos = name.IndexOf("[");
			int len;
			if (pos < 0)
				return 1;
			string str = name.Substring(pos + 1);
			str = str.Substring(0, str.IndexOf(']'));
			len = int.Parse(str);
			name = name.Substring(0, pos);
			return len;
		}

	}
}
