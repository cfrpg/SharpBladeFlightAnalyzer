﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace SharpBladeFlightAnalyzer
{
	public class ULogFile :IDisposable
	{
		private static Dictionary<string, int> typeSize;
		public static DateTime UnixStartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
		const string AllMsgTypes = "BFIMPQARDLCSO";

		UInt64 timestamp;
		byte version;

		BinaryReader reader;

		ulong[] appendedOffset;

		FileInfo file;
		
		//name,format
		Dictionary<string, MessageFormat> formatList;
		//id,message
		Dictionary<int, Message> messageDict;
		//key,value
		List<Tuple<string, string>> infomations;
		//key,value
		List<Parameter> parameters;

		List<LoggedMessage> messages;

		List<MessageViewModel> messageList;		

		bool defEndFlag = false;

		bool readCompleted;

		long currLoadPos;

		bool[] ignoreErrorFlags;

		public ulong Timestamp
		{
			get { return timestamp; }
			set { timestamp = value; }
		}

		public byte Version
		{
			get { return version; }
			set { version = value; }
		}

		public List<Tuple<string, string>> Infomations
		{
			get { return infomations; }
			set { infomations = value; }
		}

		public static Dictionary<string, int> TypeSize
		{
			get
			{
				if(typeSize==null)
				{
					typeSize = new Dictionary<string, int>();
					for(int i=8;i<=64;i<<=1)
					{
						typeSize.Add("int" + i.ToString() + "_t", i >> 3);
						typeSize.Add("uint" + i.ToString() + "_t", i >> 3);
					}
					typeSize.Add("float", 4);
					typeSize.Add("double", 8);
					typeSize.Add("char", 1);
					typeSize.Add("bool", 1);
				}
				return typeSize;
			}
		}

		public List<Parameter> Parameters
		{
			get { return parameters; }
			set { parameters = value; }
		}
		
		public List<LoggedMessage> Messages
		{
			get { return messages; }
			set { messages = value; }
		}

		public FileInfo File
		{
			get { return file; }
		}

		public List<MessageViewModel> MessageList { get => messageList; set => messageList = value; }
		public Dictionary<string, MessageFormat> FormatList { get => formatList; set => formatList = value; }
		public Dictionary<int, Message> MessageDict { get => messageDict; set => messageDict = value; }
		public long TotalSize { get => reader == null ? -1 : reader.BaseStream.Length; }

		public long CurrPos { get => currLoadPos; }
		public bool ReadCompleted { get => readCompleted; set => readCompleted = value; }

		public ULogFile()
		{
			appendedOffset = new UInt64[3] { 0, 0, 0 };
			FormatList = new Dictionary<string, MessageFormat>();
			MessageDict = new Dictionary<int, Message>();
			infomations=new List<Tuple<string, string>>();
			parameters=new List<Parameter>();
			Messages = new List<LoggedMessage>();
			readCompleted = false;
			ignoreErrorFlags = new bool[256];
		}

		public bool Load(string path, Dictionary<string, FieldConfig> fieldConfigs)
		{
			file = new FileInfo(path);			
			try
			{
				reader = new BinaryReader(new FileStream(path,FileMode.Open,FileAccess.Read));
			}
			catch
			{				
				return false;
			}
			
			byte[] buff = reader.ReadBytes(8);
			if (buff[0] != 0x55)
				return false;
			if (buff[1] != 0x4c)
				return false;
			if (buff[2] != 0x6f)
				return false;
			if (buff[3] != 0x67)
				return false;
			if (buff[4] != 0x01)
				return false;
			if (buff[5] != 0x12)
				return false;
			if (buff[6] != 0x35)
				return false;
			version = buff[7];
			timestamp = reader.ReadUInt64();
			while (readMessage())
			{
				currLoadPos = reader.BaseStream.Position;
			}
			reader.Close();

			FileInfo fi = new FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + "config\\Quaternions.txt");
			if(fi.Exists)
			{
				StreamReader sr = new StreamReader(System.AppDomain.CurrentDomain.BaseDirectory + "config\\Quaternions.txt");
				while(!sr.EndOfStream)
				{
					string line = sr.ReadLine();
					if (line[0] == '/' && line[1] == '/')
						continue;
					string[] strs = line.Split(' ');
					string topic = strs[0].Substring(0,strs[0].IndexOf('.'));
					string name = strs[0].Substring(strs[0].IndexOf('.') + 1);
					string newname = strs.Length == 2 ? strs[1]+"." : "";
					processQuaternion(topic, name, newname);
				}
				sr.Close();
			}
			messageList = new List<MessageViewModel>();
			foreach(var v in MessageDict)
			{
				if (v.Value.TimeStamps.Count == 0)
					continue;
				MessageViewModel mvm = new MessageViewModel();				
				mvm.Name = v.Value.Name;
				mvm.ID = v.Key;
				mvm.IsMassage = true;
				mvm.Size = v.Value.TimeStamps.Count;
				mvm.Message = v.Value;
				foreach(var df in v.Value.FieldDict)
				{
					if (df.Value.Flag == SpecialField.Padding)
					{
						df.Value.Dispose();
						continue;
					}
					if (df.Value.Flag == SpecialField.TimeStamp)
					{						
						continue;//mvm.Children.Add(new MessageViewModel() { Name = df.Key + "_[TS]", DataField = df.Value });
					}
					else
						mvm.Children.Add(new MessageViewModel() { Name = df.Key, DataField = df.Value, Size = df.Value.Values.Count });
					df.Value.Values.TrimExcess();
				}
				messageList.Add(mvm);
				v.Value.TimeStamps.TrimExcess();
			}
			messageList.Sort((a, b) =>
			{
				return a.Name.CompareTo(b.Name);
			});
			messageList.TrimExcess();
						
			GC.Collect();
			readCompleted = true;
			return true;
		}

		public void Dispose()
		{
			foreach(var v in MessageDict)
			{
				v.Value.Dispose();
			}
			messageList.Clear();
			GC.Collect();
		}

		private string readASCIIString(int n)
		{
			byte[] buff = reader.ReadBytes(n);
			return Encoding.ASCII.GetString(buff, 0, n).TrimEnd('\0');
		}

		private bool readMessage()
		{
			if (reader.BaseStream.Length - reader.BaseStream.Position < 3)
				return false;
			long pos = reader.BaseStream.Position;
			ushort size = reader.ReadUInt16();
			byte msgtype = reader.ReadByte();
			if (reader.BaseStream.Length - reader.BaseStream.Position < size)
				return false;
			bool res = false;
			string errmsg="读取类型为"+msgtype.ToString()+"的消息时发生未知错误";
			switch (msgtype)
			{
				case 66://B
					res = readFlagBitset(size);
					break;
				case 70://F
					res = readFormatDefinition(size);
					break;
				case 73://I
					res = readInformation(size);
					break;
				case 77://M
					res = readInformationMulti(size);
					break;
				case 80://P
					res = readParameter(size, false);
					break;
				case 81://Q
					res = readParameter(size, true);
					break;
				case 65://A
					if(!defEndFlag)					
						expendDefinition();
					res = readSubscribe(size);
					break;
				case 82://R
					res = readUnsubscribe(size);
					break;
				case 68://D
					res = readLoggedData(size);
					break;
				case 76://L
					if (!defEndFlag)
						expendDefinition();
					res = readLoggedString(size,false);
					break;
				case 67://C Tagged Logged string message
					res = readLoggedString(size, true);
					break;
				case 83://S
					res = readSynchronization(size);
					break;
				case 79://O
					res = readDropoutMark(size);
					break;
				
				default:
					if (msgtype == 0 && size==0)
					{						
						int blanklen = 0;
						while (reader.BaseStream.Position != reader.BaseStream.Length)
						{
							byte b = reader.ReadByte();
							if (b == 0)
								blanklen++;
							else
								break;
						}
						Debug.WriteLine("Suspected blank data:at {0}, len {1}.", pos, blanklen);						
					}
					Debug.WriteLine("Unknow message type:{0},at {1}, size {2}.", msgtype, pos, size);
					byte[] tmp= reader.ReadBytes(size);
					res = false;
					errmsg = "未知日志消息类型：" + msgtype.ToString();
					Debug.WriteLine("Try to find the next package.");
					if (!findNextData())
					{
						errmsg = "文件末端损坏";
					}
					break;
			}
			
			if (!res)
			{
				if(ignoreErrorFlags[msgtype])
				{
					return true;
				}
				errmsg = "读取文件时发生错误：" + System.Environment.NewLine + errmsg + System.Environment.NewLine + "是否继续？";
				if (MessageBox.Show(errmsg, "SharpBladeFlightAnalyzer", MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					ignoreErrorFlags[msgtype] = true;
					return true;
				}
				else
					return false;
			}
			
			return res;
		}

		private bool checkData(int thrd)
		{			
			long posbackup = reader.BaseStream.Position;

			for(int i=0;i<thrd;i++)
			{
				if (reader.BaseStream.Length - reader.BaseStream.Position < 3)
					return false;
				ushort len = reader.ReadUInt16();
				char msg = Convert.ToChar(reader.ReadByte());
				if (reader.BaseStream.Length - reader.BaseStream.Position < len)
					return false;
				if (!AllMsgTypes.Contains(msg))
					return false;
				byte[] tmp = reader.ReadBytes(len);
			}
			reader.BaseStream.Position = posbackup;
			return true;
		}

		private bool findNextData()
		{
			while(!checkData(3))
			{				
				while(true)
				{
					if (reader.BaseStream.Length - reader.BaseStream.Position < 3)
						return false;
					ushort len = reader.ReadUInt16();
					char msg = Convert.ToChar(reader.ReadByte());
					if (AllMsgTypes.Contains(msg))
					{
						reader.BaseStream.Position -= 3;
						break;
					}
					reader.BaseStream.Position -= 2;
				}
			}
			Debug.WriteLine("Find avalible data at {0}.", reader.BaseStream.Position);
			return true;
		}
		private void expendDefinition()
		{
			bool alldone = false;
			while (!alldone)
			{
				alldone = true;
				foreach (var mf in FormatList)
				{
					if (mf.Value.AllElementFlag)
						continue;
					MessageFormat format = mf.Value;
					for (int i = 0; i < format.FieldList.Count; i++)
					{
						if (!ULogFile.TypeSize.ContainsKey(format.FieldList[i].Item1))
						{
							string typeName = format.FieldList[i].Item1;
							string varName = format.FieldList[i].Item2 + ".";
							if (FormatList.ContainsKey(typeName))
							{
								MessageFormat type = FormatList[typeName];
								format.FieldList.RemoveAt(i);
								for (int j = 0; j < type.FieldList.Count; j++)
								{
									if(type.FieldList[j].Item3== SpecialField.TimeStamp)
										format.FieldList.Insert(i, new Tuple<string, string, SpecialField>(type.FieldList[j].Item1, varName + type.FieldList[j].Item2, SpecialField.None));
									else
										format.FieldList.Insert(i, new Tuple<string, string, SpecialField>(type.FieldList[j].Item1, varName + type.FieldList[j].Item2, type.FieldList[j].Item3));
									i++;
								}
							}
						}
					}
					if (!format.CheckElementType())
						alldone = false;
				}
			}
			defEndFlag = true;
		}

		private bool readFlagBitset(ushort msglen)
		{
			byte[] compat = reader.ReadBytes(8);
			byte[] incompat = reader.ReadBytes(8);
			appendedOffset[0] = reader.ReadUInt64();
			appendedOffset[1] = reader.ReadUInt64();
			appendedOffset[2] = reader.ReadUInt64();
			return true;
		}

		private bool readFormatDefinition(ushort msglen)
		{
			string defstr = readASCIIString(msglen);
			MessageFormat mf = new MessageFormat(defstr);
			FormatList.Add(mf.Name, mf);			
			return true;
		}

		private bool readInformation(ushort msglen)
		{
			int keylen = reader.ReadByte();
			string key = readASCIIString(keylen);
			string keytype = key.Substring(0, key.IndexOf(' '));
			string keyname = key.Substring(keytype.Length + 1);
			int arrlen = getArrayLength(ref keytype);
			int len = msglen - 1 - keylen;

			string info = "";
			if(!TypeSize.ContainsKey(keytype))
			{
				for(int i=0;i<len;i++)
				{
					info += reader.ReadByte().ToString("X2");
				}
			}
			else
			{
				if(len!=typeSize[keytype]*arrlen)
				{
					Debug.WriteLine("[I]:msglen not match.");
					return false;
				}
				if(keytype=="char")
				{
					info = readASCIIString(len);
				}
				else
				{
					for(int i=0;i<arrlen;i++)
					{
						switch(keytype)
						{
							case "int8_t":
								info += reader.ReadSByte().ToString();
								break;
							case "uint8_t":
								info += reader.ReadByte().ToString();
								break;
							case "int16_t":
								info += reader.ReadInt16().ToString();
								break;
							case "uint16_t":
								info += reader.ReadUInt16().ToString();
								break;
							case "int32_t":
								info += reader.ReadInt32().ToString();
								break;
							case "uint32_t":
								info += reader.ReadUInt32().ToString();
								break;
							case "int64_t":
								info += reader.ReadInt64().ToString();
								break;
							case "uint64_t":
								info += reader.ReadUInt64().ToString();
								break;
							case "float":
								info += reader.ReadSingle().ToString();
								break;
							case "double":
								info += reader.ReadDouble().ToString();
								break;
							case "bool":
								info += reader.ReadBoolean().ToString();
								break;
						}
						if (i != arrlen - 1)
							info += ",";
					}
				}
			}
			infomations.Add(new Tuple<string, string>(keyname, info));
			return true;
		}

		private bool readInformationMulti(ushort msglen)
		{
			reader.ReadByte();
			readInformation((ushort)(msglen - 1));
			return true;
		}

		private bool readParameter(ushort msglen,bool def)
		{
			Parameter p = new Parameter();
			byte defaultType=0;
			if(def)
			{
				defaultType = reader.ReadByte();
			}
			int keylen = reader.ReadByte();
			string key = readASCIIString(keylen);
			string keytype = key.Substring(0, key.IndexOf(' '));
			p.Name = key.Substring(keytype.Length + 1);
			float v;
			switch(keytype)
			{
				case "int32_t":
					v = reader.ReadInt32();
					break;
				case "float":
					v = reader.ReadSingle();
					break;
				default:
					Debug.WriteLine("[P]:Unknow parameter type {0}.", keytype);
					return false;
			}
			if (!def)
			{
				p.Value = v;
				parameters.Add(p);
				return true;
			}
			else
			{				
				for(int i=0;i<parameters.Count;i++)
				{
					if(parameters[i].Name==p.Name)
					{
						if ((defaultType & 0x01) != 0)
							parameters[i].SystemDefaultValue = v;
						if ((defaultType & 0x02) != 0)
							parameters[i].AirframeDefaultValue = v;
					}
				}
			}
			return true;
		}

		private bool readSubscribe(ushort msglen)
		{
			byte mid = reader.ReadByte();
			ushort id = reader.ReadUInt16();
			string name = readASCIIString(msglen - 3);
			if (!FormatList.ContainsKey(name))
				return true;
			FormatList[name].SubscribedID.Add(id);
			if (mid == 0)
			{
				MessageDict.Add(id, new Message(id, name, FormatList[name]));
				//msgNameDict.Add(id, name);
			}
			else
			{
				string newMsgName = name + "_" + mid.ToString();
				MessageDict.Add(id, new Message(id, newMsgName, FormatList[name]));				
			}
			return true;
		}

		private bool readUnsubscribe(ushort msglen)
		{
			//not use
			reader.ReadBytes(msglen);
			return true;
		}

		private bool readLoggedData(ushort msglen)
		{		
			List<double> values = new List<double>();
			double ts=0;
			ushort msgid = reader.ReadUInt16();
			if(!MessageDict.ContainsKey(msgid))
			{
				reader.ReadBytes(msglen - 2);
				return true;
			}			
			Message msg = MessageDict[msgid];
				
			double value = 0;
			int size = 0;
			int cnt = 0;
			foreach(var v in msg.Format.FieldList)
			{
				if (size >= msglen - 2)
				{
					bool f = false;
					for(;cnt<msg.Format.FieldList.Count;cnt++)
					{
						if(msg.Format.FieldList[cnt].Item3!=SpecialField.Padding)
						{ 
							f = true;
							break;
						}
					}
					if(f)
						Debug.WriteLine("[L]data end before package.");
					break;
				}
				cnt++;
				size += typeSize[v.Item1];
				switch(v.Item1)
				{
					case "int8_t":
						value = reader.ReadSByte();
						break;
					case "uint8_t":
						value = reader.ReadByte();
						break;
					case "int16_t":
						value = reader.ReadInt16();
						break;
					case "uint16_t":
						value = reader.ReadUInt16();
						break;
					case "int32_t":
						value = reader.ReadInt32();
						break;
					case "uint32_t":
						value = reader.ReadUInt32();
						break;
					case "int64_t":
						value = reader.ReadInt64();
						break;
					case "uint64_t":
						value = reader.ReadUInt64();
						break;
					case "float":
						value = reader.ReadSingle();
						break;
					case "double":
						value = reader.ReadDouble();
						break;
					case "bool":
						value = reader.ReadBoolean() ? 1 : 0;
						break;
					case "char":
						value = reader.ReadByte();
						break;
				}				
			
				if (v.Item3==SpecialField.TimeStamp)
				{
					ts = (value*(1e6/msg.TimeDivider)-timestamp)/msg.TimeDivider;					
				}
				msg.FieldDict[v.Item2].Values.Add(value);				
			}
			msg.TimeStamps.Add(ts);
			
			return true;
		}

		private bool readLoggedString(ushort msglen,bool taged)
		{
			LoggedMessage msg = new LoggedMessage();
			msg.Level = (LogLevel)reader.ReadByte();
			if (taged)
				msg.Tag = (LogTag)reader.ReadUInt16();
			msg.Timestamp = reader.ReadUInt64();
			msg.Message = readASCIIString(msglen - 9);
			Messages.Add(msg);
			return true;
		}

		private bool readSynchronization(ushort msglen)
		{
			//not use
			reader.ReadBytes(msglen);
			return true;
		}

		private bool readDropoutMark(ushort msglen)
		{
			//unhandled
			reader.ReadBytes(msglen);
			return true;
		}

		private int getArrayLength(ref string name)
		{
			int pos = name.IndexOf("[");
			int len;
			if (pos < 0)
				return 1;
			string str = name.Substring(pos + 1);
			str = str.Substring(0,str.IndexOf(']'));
			len = int.Parse(str);
			name = name.Substring(0, pos);
			return len;
		}

		private void processQuaternion(string topic,string name,string newname)
		{
			foreach(var mf in FormatList)
			{
				if(mf.Key==topic)
				{
					foreach(var id in mf.Value.SubscribedID)
					{
						Message msg = MessageDict[id];
						DataField[] quats = new DataField[4];
						for (int i = 0; i < 4; i++)
						{
							string key = name + "[" + i.ToString() + "]";
							if (!msg.FieldDict.ContainsKey(key))
								return;
							quats[i] = msg.FieldDict[key];
						}
						DataField psi = new DataField(newname + "yaw",msg);
						DataField theta = new DataField(newname + "pitch", msg);
						DataField phi = new DataField(newname + "roll", msg);
						for (int i = 0; i < quats[0].Values.Count; i++)
						{
							double w = quats[0].Values[i];
							double x = quats[1].Values[i];
							double y = quats[2].Values[i];
							double z = quats[3].Values[i];
							phi.Values.Add(Math.Atan2(2 * (w * x + y * z), 1 - 2 * (x * x + y * y)));
							theta.Values.Add(Math.Asin(2 * (w * y - z * x)));
							psi.Values.Add(Math.Atan2(2 * (w * z + x * y), 1 - 2 * (y * y + z * z)));							
						}
						msg.FieldDict.Add(phi.Name, phi);
						msg.FieldDict.Add(theta.Name, theta);
						msg.FieldDict.Add(psi.Name, psi);
					}
				}
			}
		
		}

	}
}
