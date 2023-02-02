using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using XenoTools.Bdat;
using XenoTools.Pack;
using XenoTools.Utils;

namespace XenoTools
{
    class Program
    {

        static void Main(string[] args)
        {
			//UnpackPackArchive("/Users/amberbrault/Documents/Xenoblade Decomp/Game Files/Pack Files/common.pkb");
			DecompressBDATArchives();

		}

		public static void UnpackPackArchive(string path) {
			PackTools.Unpack(path);
		}

		public static void FindPossibleHashStrings() {
			string path = "/Users/amberbrault/Documents/Xenoblade Decomp/Game Files/Pack Files/font.pkh";
			PackTools.ReadPKHFile(path);
			PackHeader packHeader = PackTools.packHeader;
			List<ulong> fileHashes = packHeader.fileHashTable.ToList();
			PKHFileNameFinder.hashValTable = packHeader.hashValTable;
			PKHFileNameFinder.CreateHashValueInfoList();
			PKHFileNameFinder.logger.ClearFile();
			PKHFileNameFinder.logger.Log("Archive file: " + path + "\n");

			for (int i = 0; i < packHeader.files; i++) {
				PKHFileNameFinder.logger.Log("Finding possible strings for file " + i + "'s hash\n");
				PKHFileNameFinder.FindAllPossibleStringsFromHash(fileHashes[i]);
				PKHFileNameFinder.logger.Log("\n");
			}
		}

		public static void CheckFilenameListHashes() {
			PackTools.ReadPKHFile("/Users/amberbrault/Documents/Xenoblade Decomp/Game Files/Pack Files/common.pkh");
			PackHeader packHeader = PackTools.packHeader;
			List<ulong> fileHashes = packHeader.fileHashTable.ToList();

			foreach (string path in PKHArchiveFiles.commonPkhJpFiles) {
				string filename = Path.GetFileName(path);
				//The hash calculation seems to only use the file name itself, not the entire path
				ulong hash = PackFileHashUtil.CalculatePackFileHash(filename, packHeader.hashValTable);
				Console.WriteLine("File name: {0}, hash: {1}", path, hash.ToString("X16"));
				if (fileHashes.Contains(hash)) {
					Console.WriteLine("Matches hash of file {0}", fileHashes.IndexOf(hash));
				} else {
					Console.WriteLine("No match found");
				}
				Console.WriteLine();
			}
		}

		public static void DecompressPackFiles(string folder) {
			foreach (string file in Directory.EnumerateFiles(folder, "*.pkb")) {
				Console.WriteLine(file);
				PackTools.Unpack(file);
			}
		}

        public static void DecompressBDATArchives() {
			foreach (string file in Directory.EnumerateFiles("out/common/jp", "*.bin")) {
				Console.WriteLine("Unpacking " + file);
				BDATCsv[] bdatCsvFiles = BDATTools.UnpackBDATArchive(File.ReadAllBytes(file));
				string basePath = file.Replace(".bin", "") + "/";
				Directory.CreateDirectory(basePath);

				foreach(BDATCsv bdatCsvFile in bdatCsvFiles) {
					string path = basePath + bdatCsvFile.name + ".csv";
					File.WriteAllLines(path, bdatCsvFile.data);
					File.WriteAllBytes(path.Replace(".csv",".bdat"), bdatCsvFile.originalData);
				}
				
			}
		}
    }
}

