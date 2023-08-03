using System;
using System.Linq;
using XenoTools.Graphics;
using XenoTools.Utils;

namespace XenoTools.Formats.TPL
{
	/*
	//Handles the image data found in BRRES/TPL files.

	Format info:

	Block sizes:
	I4/C4/CMPR: 8x8
	I8/IA4/C8: 8x4
	IA8/RGB565/RGB5A3/RGBA8/C14X2: 4x4
	*/
	public class TPLImageDataUtils{
		public static Color[] palette;

		//Converts the given image data to a PNG file.
		public static void ConvertToPng(byte[] data, int width, int height, TPLImageFormat format, string path) {
			Bitmap bitmap = new Bitmap(width, height);

			if((format == TPLImageFormat.C4 || format == TPLImageFormat.C8 || format == TPLImageFormat.C14X2) && palette == null) {
				throw new Exception("Error: The given palette cannot be null for a palette image.");
			}

			MemoryUtils.currentNybble = 0; //TODO: change this bad code

			int blockSizeWidth = 4;
			int blockSizeHeight = 4;

			//Determine the block size to use based on the format
			switch (format) {
				case TPLImageFormat.I4:
				case TPLImageFormat.C4:
				case TPLImageFormat.CMPR:
				blockSizeWidth = 8;
				blockSizeHeight = 8;
				break;
				case TPLImageFormat.I8:
				case TPLImageFormat.IA4:
				case TPLImageFormat.C8:
				blockSizeWidth = 8;
				blockSizeHeight = 4;
				break;
				case TPLImageFormat.IA8:
				case TPLImageFormat.RGB565:
				case TPLImageFormat.RGB5A3:
				case TPLImageFormat.RGBA8:
				case TPLImageFormat.C14X2:
				blockSizeWidth = 4;
				blockSizeHeight = 4;
				break;
			}

			int blockWidth = (int)Math.Ceiling((float)width / (float)blockSizeWidth);
			int blockHeight = (int)Math.Ceiling((float)height / (float)blockSizeHeight);
			int length = (blockWidth * blockSizeWidth) * (blockHeight * blockSizeHeight);

			int i = 0;
			int offset = 0;

			while (i < length) {
				int blockIndex = i / (blockSizeWidth * blockSizeHeight);

				int currentBlockWidth = blockSizeWidth;
				int currentBlockHeight = blockSizeHeight;

				//Account for edge blocks that are only partially used
				if ((blockIndex % blockWidth) == blockWidth - 1 && width % blockSizeWidth != 0) {
					currentBlockWidth = width % blockSizeWidth;
				}

				if (blockIndex >= blockWidth * (blockHeight - 1) && height % blockSizeHeight != 0) {
					currentBlockHeight = height % blockSizeHeight;
				}

				if (format == TPLImageFormat.CMPR) {
					/*
					CMPR is unique from the others as a lossy compression method (DXT1), so it is handled separately.
					Each block is made up of 4 4x4 subblocks, which go from left right and top down. They
					each have their own small 4 color palette, encoded as two values, with the other two
					being calculated from the first two. The palette values themselves use the RGB565 format.
					The last 4 bytes are used to store the subblock pixel vales, each being a 2 bit palette
					index.
					*/

					//Read each of the 4 4x4 subblocks
					for (int j = 0; j < 4; j++) {
						//Calculate the 4 color palette for the current subblock from the two provided colors
						ushort col1Val = MemoryUtils.ReadUInt16(offset, data);
						ushort col2Val = MemoryUtils.ReadUInt16(offset + 2, data);
						Color[] blockPalette = new Color[4];
						blockPalette[0] = ReadColor(data, ref offset, TPLImageFormat.RGB565);
						blockPalette[1] = ReadColor(data, ref offset, TPLImageFormat.RGB565);

						if(col1Val > col2Val) {
							//If the first color value is greater than the second, calculate the other two
							//colors by linearly interpolating at 1/3 and 2/3 between them
							blockPalette[2] = Color.Lerp(blockPalette[1], blockPalette[0], 0.333f);
							blockPalette[3] = Color.Lerp(blockPalette[1], blockPalette[0], 0.666f);
						} else {
							//If the first is less than the second, the third color is calculated by
							//linearly interpolating halfway between, and the fourth is transparent
							blockPalette[2] = Color.Lerp(blockPalette[0], blockPalette[1], 0.5f);
							blockPalette[3] = Color.transparent;
						}

						//Read the pixels ahead of time to make writing them to the image easier
						byte[,] pixels = new byte[4,4];

						for(int k = 0; k < 16; k += 4) {
							byte rowByte = MemoryUtils.ReadByteUpdate(ref offset, data);
							pixels[0, k / 4] = (byte)((rowByte >> 6) & 0x3);
							pixels[1, k / 4] = (byte)((rowByte >> 4) & 0x3);
							pixels[2, k / 4] = (byte)((rowByte >> 2) & 0x3);
							pixels[3, k / 4] = (byte)(rowByte & 0x3);
						}

						for (int y = 0; y < 4; y++) {
							for (int x = 0; x < 4; x++) {
								Color col = blockPalette[pixels[x, y]];
								int subX = x + (j % 2) * 4;
								int subY = y + (j / 2) * 4;

								//If the current pixel in the block is used, save it to the image
								if (subX < currentBlockWidth && subY < currentBlockHeight) {
									int xPos = subX + (blockIndex % blockWidth) * blockSizeWidth;
									int yPos = subY + (blockIndex / blockWidth) * blockSizeHeight;
									bitmap.SetPixel(xPos, yPos, col);
								}
								i++;
							}
						}
					}
				} else {
					//Read the next block from the image data
					for (int y = 0; y < blockSizeHeight; y++) {
						for (int x = 0; x < blockSizeWidth; x++) {
							Color col = ReadColor(data, ref offset, format);

							//If the current pixel in the block is used, save it to the image
							if (x < currentBlockWidth && y < currentBlockHeight) {
								int xPos = x + (blockIndex % blockWidth) * blockSizeWidth;
								int yPos = y + (blockIndex / blockWidth) * blockSizeHeight;
								bitmap.SetPixel(xPos, yPos, col);
							}
							i++;
						}
					}
				}

				//If the format is rgba8, skip over the green/blue group for the current block
				if (format == TPLImageFormat.RGBA8) offset += 32;
			}


			bitmap.SaveToPng(path);
		}

		public static Color ReadColor(byte[] data, ref int offset, TPLImageFormat format) {
			Color col;

			switch (format) {
				case TPLImageFormat.I4:
					col = TPLColorUtil.ReadI4(data, ref offset);
					break;
				case TPLImageFormat.I8:
					col = TPLColorUtil.ReadI8(data, ref offset);
					break;
				case TPLImageFormat.IA4:
					col = TPLColorUtil.ReadIA4(data, ref offset);
					break;
				case TPLImageFormat.IA8:
					col = TPLColorUtil.ReadIA8(data, ref offset);
					break;
				case TPLImageFormat.RGB565:
					col = TPLColorUtil.ReadRGB565(data, ref offset);
					break;
				case TPLImageFormat.RGB5A3:
					col = TPLColorUtil.ReadRGB5A3(data, ref offset);
					break;
				case TPLImageFormat.RGBA8:
					//The red/alpha and green/blue values come in two separate groups,
					//with the green/blue group coming second. To make things easier,
					//the data offset is only incremented by 2 here, and after each block
					//the offset is incremented by another 32 to skip over the green/blue
					//group.
					byte a = data[offset];
					byte r = data[offset + 1];
					byte g = data[offset + 32];
					byte b = data[offset + 33];
					offset += 2;
					col = new Color(r, g, b, a);
					break;
				case TPLImageFormat.C4:
					int palIndex = MemoryUtils.ReadNybble(data, ref offset);
					col = palette[palIndex];
					break;
				case TPLImageFormat.C8:
					palIndex = MemoryUtils.ReadByteUpdate(ref offset, data);
					col = palette[palIndex];
					break;
				case TPLImageFormat.C14X2:
					palIndex = MemoryUtils.ReadUInt16Update(ref offset, data) & 0x3FFF; //only keep the bottom 14 bits
					col = palette[palIndex];
					break;
				default:
					throw new Exception("Error: unsupported format " + format);
			}

			return col;
		}
	}
}

