using System;
using System.Linq;

namespace XenoTools.Utils
{
	public static class MemoryUtils
	{

		public static uint ReadUInt32(int offset, byte[] data) {
			uint val = BitConverter.ToUInt32(data.Skip(offset).Take(4).Reverse().ToArray());
			return val;
		}

		public static uint ReadUInt32Update(ref int offset, byte[] data) {
			uint val = BitConverter.ToUInt32(data.Skip(offset).Take(4).Reverse().ToArray());
			offset += 4;
			return val;
		}

		public static int ReadInt(int offset, byte[] data) {
			int val = BitConverter.ToInt32(data.Skip(offset).Take(4).Reverse().ToArray());
			return val;
		}

		public static int ReadIntUpdate(ref int offset, byte[] data) {
			int val = BitConverter.ToInt32(data.Skip(offset).Take(4).Reverse().ToArray());
			offset += 4;
			return val;
		}

		public static ushort ReadUInt16(int offset, byte[] data) {
			ushort val = BitConverter.ToUInt16(data.Skip(offset).Take(2).Reverse().ToArray());
			return val;
		}

		public static ushort ReadUInt16Update(ref int offset, byte[] data) {
			ushort val = BitConverter.ToUInt16(data.Skip(offset).Take(2).Reverse().ToArray());
			offset += 2;
			return val;
		}

		public static short ReadShort(int offset, byte[] data, bool littleEndian = false) {
			short val;

			if (!littleEndian) {
				val = BitConverter.ToInt16(data.Skip(offset).Take(2).Reverse().ToArray());
			} else {
				val = BitConverter.ToInt16(data.Skip(offset).Take(2).ToArray());
			}

			return val;
		}

		public static byte ReadByte(int offset, byte[] data) {
			byte val = data[offset];
			return val;
		}

		public static byte ReadByteUpdate(ref int offset, byte[] data) {
			byte val = data[offset];
			offset++;
			return val;
		}

		//TODO: find a better way to do this. maybe make this into a regular class?
		//Keeps track of which nybble of the current byte to use
		public static int currentNybble = 0;

		public static byte ReadNybble(byte[] data, ref int offset) {
			byte val = data[offset];

			//If currentNybble is 0, use the first half
			if (currentNybble == 0) {
				currentNybble++;
				val = (byte)(val >> 4);
			} else {
				//Otherwise, use the first half, and increment the offset to the next byte
				currentNybble = 0;
				offset++;
				val = (byte)(val & 0x7);
			}

			return val;
		}

		public static float ReadFloat(int offset, byte[] data) {
			float val = BitConverter.ToSingle(data.Skip(offset).Take(4).Reverse().ToArray());
			return val;
		}

		public static float ReadFloatUpdate(ref int offset, byte[] data) {
			float val = BitConverter.ToSingle(data.Skip(offset).Take(4).Reverse().ToArray());
			offset += 4;
			return val;
		}

		//Reads a zero terminated string at the current offset.
		public static string ReadString(int offset, byte[] data) {
			string str = "";
			//Keep going until we reach the terminator byte
			while (data[offset] != 0) {
				str += (char)data[offset++];
			}
			offset++; //Increment past the terminator byte;
			return str;
		}

		public static string ReadShiftJISString(int offset, byte[] data) {
			return ShiftJISDecoder.DecodeShiftJIS(data, offset, true);
		}
	}
}

