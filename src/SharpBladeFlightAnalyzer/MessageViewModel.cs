using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeFlightAnalyzer
{
	public class MessageViewModel
	{
		string name;
		int id;
		bool isMassage;
		List<MessageViewModel> children;
		int size;
		string displayName;
		DataField dataField;
		Message message;

		public string Name 
		{
			get => name;
			set
			{
				name = value;
				if (isMassage)
					displayName = name + "  ( " + size.ToString() + " )";
				else
					displayName = name;
			}
		}
		public int ID { get => id; set => id = value; }
		public bool IsMassage { get => isMassage; set => isMassage = value; }
		public List<MessageViewModel> Children { get => children; set => children = value; }
		

		public int Size 
		{ 
			get => size;
			set
			{
				size = value;
				if (isMassage) 
					displayName = name + "  ( " + size.ToString() + " )";
			}
		}
		public string DisplayName { get => displayName; }
		public DataField DataField { get => dataField; set => dataField = value; }
		public Message Message { get => message; set => message = value; }

		public MessageViewModel()
		{
			children = new List<MessageViewModel>();
			name = "";
			isMassage = false;
			size = 0;
		}
	}
}
