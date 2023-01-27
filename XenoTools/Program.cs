using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using XenoTools.FileFormats;
using XenoTools.Utils;

namespace XenoTools
{


    class Program
    {

        static void Main(string[] args)
        {
			PackDecoder packDecoder = new PackDecoder();
			//packDecoder.ReadPKHFile("pkh/work.pkh");
			packDecoder.Unpack("pkh/work.pkb");

			/*foreach (string path in commonPkhJpFilePaths) {
				string filename = Path.GetFileName(path);
				//The hash calculation seems to only use the file name itself, not the entire path
				ulong hash = PackFileHashUtil.CalculatePackFileHash(filename, packDecoder.packHeader.hashValTable);
				Console.WriteLine("File name: {0}, hash: {1}",filename,hash.ToString("X16"));
			}*/

			//PKHFileNameFinder.Init(packDecoder.packHeader.hashValTable);
			//PKHFileNameFinder.FindAllPossibleStringsFromHash(packDecoder.packHeader.fileHashTable[1]);
        }

        public static void DecompressBDATFiles() {
			BDATDecoder bdat = new BDATDecoder();

			foreach (string file in Directory.EnumerateFiles("bdat", "*.bdat")) {
				Console.WriteLine("Converting " + file);
				string[] lines = bdat.DecodeBdat(File.ReadAllBytes(file));
				string path = file.Replace(".bdat", ".csv");
				File.WriteAllLines(path, lines);
			}
		}
    }
}

