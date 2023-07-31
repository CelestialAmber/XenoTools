using System;
using XenoTools.Utils;
using System.Linq;

namespace XenoTools.Formats.TPL
{
	public struct TPLImageHeader {
		public ushort width;
		public ushort height;
		public TPLImageFormat format;
		public uint imageDataAddress;
		public uint wrapS;
		public uint wrapT;
		public uint minFilter;
		public uint magFilter;
		public float lodBias;
		public byte edgeLodEnable;
		public byte minLod;
		public byte maxLod;
		public bool unpacked;
	}

	public class TPLImage
	{
		public TPLImageHeader header;
		byte[] data;

		public TPLImage(byte[] data, int headerOffset)
		{
			this.data = data;
			ReadHeader(headerOffset);
		}

		void ReadHeader(int offset) {
			header.height = MemoryUtils.ReadUInt16Update(ref offset, data);
			header.width = MemoryUtils.ReadUInt16Update(ref offset, data);
			header.format = (TPLImageFormat)MemoryUtils.ReadUInt32Update(ref offset, data);
			header.imageDataAddress = MemoryUtils.ReadUInt32Update(ref offset, data);
			header.wrapS = MemoryUtils.ReadUInt32Update(ref offset, data);
			header.wrapT = MemoryUtils.ReadUInt32Update(ref offset, data);
			header.minFilter = MemoryUtils.ReadUInt32Update(ref offset, data);
			header.magFilter = MemoryUtils.ReadUInt32Update(ref offset, data);
			header.lodBias = MemoryUtils.ReadFloatUpdate(ref offset, data);
			header.edgeLodEnable = MemoryUtils.ReadByteUpdate(ref offset, data);
			header.minLod = MemoryUtils.ReadByteUpdate(ref offset, data);
			header.maxLod = MemoryUtils.ReadByteUpdate(ref offset, data);
			header.unpacked = MemoryUtils.ReadByteUpdate(ref offset, data) == 1 ? true : false;
		}

		public void ConvertToPNG(string path) {
			TPLImageDataUtils.ConvertToPng(data.Skip((int)header.imageDataAddress).ToArray(), header.width,
				header.height, header.format, path);
		}
	}
}

