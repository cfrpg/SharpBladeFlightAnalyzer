using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SharpBladeFlightAnalyzer
{
	public class ULogFileReader
	{
		UInt64 timestamp;
		byte version;

		BinaryReader reader;

		public virtual int HeaderSize
		{
			get { return 0; }
		}

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

		public ULogFileReader(string path, int buffsize)
		{
			FileInfo fi = new FileInfo(path);
			reader = new BinaryReader(new FileStream(path, FileMode.Open));
		}
		#region Read&Write

		public virtual bool StartRead()
		{
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

		public sbyte NextSByte()
		{
			unchecked
			{
				return (sbyte)reader.ReadByte();
			}
		}
		public byte NextByte()
		{
			return reader.ReadByte();
		}
		public short NextShort()
		{
			return reader.ReadInt16();
		}
		public ushort NextUShort()
		{
			return reader.ReadUInt16();
		}
		public int NextInt32()
		{
			return reader.ReadInt32();
		}
		public uint NextUInt32()
		{
			return reader.ReadUInt32();
		}
		public float NextSingle()
		{
			return reader.ReadSingle();
		}
		public double NextDouble()
		{
			return reader.ReadDouble();
		}

		public UInt64 NextUInt64()
		{
			return reader.ReadUInt64();
		}

		public string NextASCIIString(int n)
		{
			byte[] buff = reader.ReadBytes(n);
			return Encoding.ASCII.GetString(buff, 0, n).TrimEnd('\0');
		}
	
		#endregion

		public void Close()
		{
			reader.Close();
		}
	}
}
