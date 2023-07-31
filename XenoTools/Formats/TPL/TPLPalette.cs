using System;
using XenoTools.Utils;

namespace XenoTools.Formats.TPL
{

	public class TPLPalette
	{
		int entries;
		bool unpacked;
		TPLPaletteFormat format;
		int paletteDataAddress;
		byte[] paletteData;
		byte[] data;


		public TPLPalette(byte[] data, int headerOffset)
		{
			this.data = data;
			ReadHeader(headerOffset);
		}

		void ReadHeader(int offset) {
			entries = MemoryUtils.ReadUInt16Update(ref offset, data);
			unpacked = MemoryUtils.ReadByteUpdate(ref offset, data) == 1 ? true : false;
			offset++; //padding byte at 0x3
			format = (TPLPaletteFormat)MemoryUtils.ReadUInt32Update(ref offset, data);
			paletteDataAddress = (int)MemoryUtils.ReadUInt32Update(ref offset, data);
		}
	}
}

