using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeFlightAnalyzer
{
	public class Parameter
	{
		string name;
		float value;
		float systemDefaultValue;
		float airframeDefaultValue;
		string displayedSysDefalut;
		string displayedAirframeDefault;

		public string Name { get => name; set => name = value; }
		public float Value { get => value; set => this.value = value; }
		public float SystemDefaultValue 
		{ 
			get => systemDefaultValue;
			set
			{
				systemDefaultValue = value;
				displayedSysDefalut = value.ToString();
			}
		}
		public float AirframeDefaultValue 
		{ 
			get => airframeDefaultValue;
			set
			{
				airframeDefaultValue = value;
				displayedAirframeDefault = value.ToString();
			}
		}
		public string DisplayedSysDefalut { get => displayedSysDefalut; set => displayedSysDefalut = value; }
		public string DisplayedAirframeDefault { get => displayedAirframeDefault; set => displayedAirframeDefault = value; }

		public Parameter()
		{
			name = "";
			value = 0;
			systemDefaultValue = 0;
			airframeDefaultValue = 0;
			displayedSysDefalut = "";
			displayedAirframeDefault = "";
		}
	}
}
