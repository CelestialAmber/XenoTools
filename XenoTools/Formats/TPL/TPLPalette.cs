using System;
using XenoTools.Utils;
using XenoTools.Graphics;

namespace XenoTools.Formats.TPL
{

	public class TPLPalette
	{
		int entries;
		bool unpacked;
		TPLPaletteFormat format;
		int paletteDataOffset;

		byte[] data;
		public Color[] palette;

		public TPLPalette() {
		}

		public TPLPalette(byte[] data, int headerOffset)
		{
			this.data = data;
			ReadHeader(headerOffset);
			ParsePaletteData();
		}

		void ReadHeader(int offset) {
			entries = MemoryUtils.ReadUInt16Update(ref offset, data);
			unpacked = MemoryUtils.ReadByteUpdate(ref offset, data) == 1 ? true : false;
			offset++; //padding byte at 0x3
			format = (TPLPaletteFormat)MemoryUtils.ReadUInt32Update(ref offset, data);
			paletteDataOffset = (int)MemoryUtils.ReadUInt32Update(ref offset, data);
		}

		void ParsePaletteData() {
			palette = new Color[entries];

			int offset = paletteDataOffset;
			for(int i = 0; i < entries; i++) {
				palette[i] = ReadColor(ref offset);
			}
		}

		Color ReadColor(ref int offset) {
			Color col;

			switch (format) {
				case TPLPaletteFormat.IA8:
				col = TPLColorUtil.ReadIA8(data, ref offset);
				break;
				case TPLPaletteFormat.RGB565:
				col = TPLColorUtil.ReadRGB565(data, ref offset);
				break;
				case TPLPaletteFormat.RGB5A3:
				col = TPLColorUtil.ReadRGB5A3(data, ref offset);
				break;
				default:
				throw new Exception("wat happen");
			}

			return col;
		}
	}
}

