using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace XenoTools.Graphics
{
	public class Bitmap {
		int width;
		int height;
		Color[,] pixels;

		public Bitmap(int width, int height) {
			this.width = width;
			this.height = height;
			pixels = new Color[width, height];

			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					pixels[x, y] = new Color();
				}
			}
		}

		public void SetPixel(int x, int y, Color color) {
			pixels[x, y] = color;
		}

		public void GetPixel(int x, int y, Color color) {
			pixels[x, y] = color;
		}

		public byte[] ToByteArray() {
			Image<Rgba32> image = new Image<Rgba32>(width, height);

			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					Color col = pixels[x, y];
					image[x, y] = new Rgba32(col.r, col.g, col.b);
				}
			}

			MemoryStream ms = new MemoryStream();
			image.SaveAsBmp(ms);
			image.Dispose();
			return ms.ToArray();
		}

		public void SaveToPng(string path) {
			Image<Rgba32> image = new Image<Rgba32>(width, height);

			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					Color col = pixels[x, y];
					image[x, y] = new Rgba32(col.r, col.g, col.b);
				}
			}

			image.SaveAsPng(path);
		}
	}
}

