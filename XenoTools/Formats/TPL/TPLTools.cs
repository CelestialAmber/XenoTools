using System;
using System.Drawing;
using System.IO;
using System.Linq;
using XenoTools.Graphics;
using XenoTools.Utils;

namespace XenoTools.Formats.TPL
{
	/*
	Header format:
	0x0-3: file magic (00 20 AF 30)
	0x4-7: number of images
	0x8-B: image table offset
	
	Image table:
	The image table has two values for each entry:
	0x0-3: image header offset
	0x4-7: palette header offset (optional, null if not present)
	
	Palette header format:
	0x0-1: entry count
	0x2: unpacked
	0x3: padding
	0x4-7: palette format
	0x8-B: palette data address
	
	Palette formats:
	0: IA8
	1: RGB565
	2: RGB5A3
	
	Image header:
	0x0-1: height
	0x2-3: width
	0x4-7: format
	0x8-B: image data address
	0xC-F: WrapS
	0x10-13: WrapT
	0x14-17: MinFilter
	0x18-1B: MagFilter
	0x1C-1F: LODBias (float)
	0x20: EdgeLODEnable
	0x21: MinLOD
	0x22: MaxLOD
	0x23: unpacked
	*/


	public class TPLTools {

		public static void ExtractImages(string path) {
			byte[] data = File.ReadAllBytes(path);
			ConvertTPL(data, path.Replace(".tpl", ".png"));
		}


		public static void ConvertTPL(byte[] data, string path) {
			int offset = 0;

			uint magic = MemoryUtils.ReadUInt32Update(ref offset, data);

			if (magic != 0x0020AF30) {
				Console.WriteLine("Error: Invalid TPL file.");
				return;
			}

			int imagesNum = (int)MemoryUtils.ReadUInt32Update(ref offset, data);
			int imageTableOffset = (int)MemoryUtils.ReadUInt32Update(ref offset, data);

			offset = imageTableOffset;

			for (int i = 0; i < imagesNum; i++) {
				int imageHeaderOffset = (int)MemoryUtils.ReadUInt32Update(ref offset, data);
				int paletteHeaderOffset = (int)MemoryUtils.ReadUInt32Update(ref offset, data);

				//If the image has a palette, read the palette data
				if (paletteHeaderOffset != 0) {
					throw new NotImplementedException();
					TPLPalette palette = new TPLPalette(data, paletteHeaderOffset);
				}

				TPLImage image = new TPLImage(data, imageHeaderOffset);
				image.ConvertToPNG(path.Replace(".png", (image.header.format == TPLImageFormat.CMPR ? "_cmpr" : "") + "_" + i + ".png"));
			}
		}

	}
}

