using System;
using XenoTools.Graphics;
using XenoTools.Utils;

namespace XenoTools.Formats.TPL
{
	public class TPLColorUtil
	{
		public static Color ReadI4(byte[] data, ref int offset) {
			byte intensity = (byte)(255f * (MemoryUtils.ReadNybble(data, ref offset) / 15f));
			return new Color(intensity);
		}

		public static Color ReadI8(byte[] data, ref int offset) {
			byte intensity = data[offset++];
			return new Color(intensity);
		}

		public static Color ReadIA4(byte[] data, ref int offset) {
			byte val = data[offset++];
			byte intensity = (byte)(255f * ((val >> 4) / 15f));
			byte a = (byte)(255f * ((val & 0xF) / 15f));
			return new Color(intensity, intensity, intensity, a);
		}

		public static Color ReadIA8(byte[] data, ref int offset) {
			byte intensity = data[offset++];
			byte a = data[offset++];
			return new Color(intensity, intensity, intensity, a);
		}

		public static Color ReadRGB565(byte[] data, ref int offset) {
			ushort colVal = MemoryUtils.ReadUInt16Update(ref offset, data);
			byte r = (byte)(255f * ((colVal >> 11) / 31f));
			byte g = (byte)(255f * ((colVal >> 5) & 0x3F) / 63f);
			byte b = (byte)(255f * ((colVal & 0x1F) / 31f));
			return new Color(r, g, b);
		}

		public static Color ReadRGB5A3(byte[] data, ref int offset) {
			ushort colVal = MemoryUtils.ReadUInt16Update(ref offset, data);
			byte r, g, b, a;
			bool hasAlpha = (colVal >> 15) == 0;

			if (hasAlpha) {
				a = (byte)(255f * (((colVal >> 12) & 0x7) / 7f));
				r = (byte)(255f * (((colVal >> 8) & 0xF) / 15f));
				g = (byte)(255f * (((colVal >> 4) & 0xF) / 15f));
				b = (byte)(255f * ((colVal & 0xF) / 15f));
			} else {
				a = 255;
				r = (byte)(255f * (((colVal >> 10) & 0x1F) / 32f));
				g = (byte)(255f * (((colVal >> 5) & 0x1F) / 32f));
				b = (byte)(255f * ((colVal & 0x1F) / 32f));
			}

			return new Color(r, g, b, a);
		}
	}
}

