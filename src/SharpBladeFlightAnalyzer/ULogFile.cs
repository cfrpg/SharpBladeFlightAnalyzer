using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace SharpBladeFlightAnalyzer
{
	public class ULogFile
	{
		UInt64 timestamp;
		byte version;

		BinaryReader reader;

        ulong[] appendedOffset;

        //ID,msgName
        Dictionary<ushort, string> msgNameDict;
        //msgName,fieldList<Type,fieldName>        
        Dictionary<string, List<Tuple<string, string>>> fieldNameDict;
        //fieldFullName,DataField
        Dictionary<string, DataField> fieldDict;

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

		public ULogFile()
		{
            appendedOffset = new UInt64[3] { 0, 0, 0 };
		}		

        public bool Load(string path, int buffsize)
        {
            FileInfo fi = new FileInfo(path);
            reader = new BinaryReader(new FileStream(path, FileMode.Open));
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
            return true;
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
            ushort size = reader.ReadUInt16();
            byte msgtype = reader.ReadByte();
            if (reader.BaseStream.Length - reader.BaseStream.Position < size)
                return false;
            switch(msgtype)
            {
                case 66://B
                    return readFlagBitset(size);
                case 70://F
                    return readFormatDefinition(size);
                case 73://I
                    return readInformation(size);
                case 77://M
                    return readInformationMulti(size);
                case 80://P
                    return readParameter(size);
                case 65://A
                    return readSubscribe(size);
                case 82://R
                    return readUnsubscribe(size);
                case 68://D
                    return readLoggedData(size);
                case 76://L
                    return readLoggedString(size);
                case 83://S
                    return readSynchronization(size);
                case 79://O
                    return readDropoutMark(size);
                default:
                    Debug.WriteLine("Unknow message type:{0}.", msgtype);
                    return false;                    
            }            
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
            int msgNameLen = defstr.IndexOf(":");
            string msgname = defstr.Substring(0,msgNameLen);
            defstr = defstr.Substring(msgNameLen + 1);
            string[] fieldNames = defstr.Split(':');

            List<Tuple<string, string>> fieldList = new List<Tuple<string, string>>();

            for (int i=0;i<fieldNames.Length;i++)
            {
                if (fieldNames[i].Length == 0)
                    continue;
                string[] field = fieldNames[i].Split(' ');
                if(field[0].IndexOf('[')>0)
                {
                    string realType = field[0].Substring(0, field[0].IndexOf('['));
                    string lens= field[0].Substring(field[0].IndexOf('[')+1);
                    lens = lens.Substring(0, field[0].IndexOf(']'));
                    int len = int.Parse(lens);
                    for(int j=0;j<len;j++)
                    {
                        fieldList.Add(new Tuple<string, string>(realType, field[1]+"["+j.ToString()+"]"));
                    }
                }
                else
                {
                    fieldList.Add(new Tuple<string, string>(field[0], field[1]));
                }
            }
            fieldNameDict.Add(msgname, fieldList);
            return true;
        }

        private bool readInformation(ushort msglen)
        {

            return true;
        }

        private bool readInformationMulti(ushort msglen)
        {
            return true;
        }

        private bool readParameter(ushort msglen)
        {
            return true;
        }

        private bool readSubscribe(ushort msglen)
        {
            return true;
        }

        private bool readUnsubscribe(ushort msglen)
        {
            return true;
        }

        private bool readLoggedData(ushort msglen)
        {
            return true;
        }

        private bool readLoggedString(ushort msglen)
        {
            return true;
        }

        private bool readSynchronization(ushort msglen)
        {
            return true;
        }

        private bool readDropoutMark(ushort msglen)
        {
            return true;
        }


	
	}
}
