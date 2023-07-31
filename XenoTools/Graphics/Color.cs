using System;

namespace XenoTools.Graphics
{
	public class Color {
		public byte r = 0, g = 0, b = 0, a = 255;

		public Color(byte val) {
			r = val;
			g = val;
			b = val;
			a = 255;
		}

		public Color(byte r, byte g, byte b) {
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = 255;
		}

		public Color(byte r, byte g, byte b, byte a) {
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public Color() {
		}

		//Linearly interpolates between two colors.
		public static Color Lerp(Color c1, Color c2, float n) {
			byte r = (byte)Math.Floor(c2.r * (1f - n) + c1.r * n);
			byte g = (byte)Math.Floor(c2.g * (1f - n) + c1.g * n);
			byte b = (byte)Math.Floor(c2.b * (1f - n) + c1.b * n);
			byte a = (byte)Math.Floor(c2.a * (1f - n) + c1.a * n);
			return new Color(r, g, b, a);
		}

		public static Color transparent => new Color(0,0,0,0);
	}
}

