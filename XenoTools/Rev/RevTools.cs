using System;
using System.IO;
using XenoTools.Utils;

namespace XenoTools.Rev
{
	public class RevTools
	{
		public RevTools()
		{
		}

		public static string filename;

		public static void DecryptRev(string path) {
			if(Path.GetExtension(path) != ".rev") {
				Console.WriteLine("Error: not a valid .rev file.");
				return;
			}

			byte[] data = File.ReadAllBytes(path);
			//The filename bytes are used as part of decryption
			filename = Path.GetFileNameWithoutExtension(path);

			DecryptSection(ref data, 0);

			string newPath = path.Replace(".rev", "_dec.rev");
			File.WriteAllBytes(newPath, data);
		}

		public static void DecryptSection(ref byte[] data, int offset) {
			byte key = 0x3D;
			int filenameLength = filename.Length;

			//Decrypt the header first to find out needed info
			for (int i = offset; i < offset + 0x58; i++) {
				byte dataKey = (byte)(key ^ (byte)filename[i % filenameLength]);
				key--;
				data[i] ^= dataKey;
			}

			int compressedDataEndOffset = (int)MemoryUtils.ReadUInt32(offset + 0x10, data) + 0x800;

			//Decrypt the rest of the data
			for (int i = offset + 0x58; i < compressedDataEndOffset; i++) {
				byte dataKey = (byte)(key ^ (byte)filename[i % filenameLength]);
				key--;
				data[i] ^= dataKey;
			}
		}
	}
}

