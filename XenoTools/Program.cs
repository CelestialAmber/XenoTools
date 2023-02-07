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
			//UnpackPackArchive("/Users/amberbrault/Documents/Xenoblade Decomp/Game Files/Pack Files/chr.pkb");
			//DecompressBDATArchives();
			//CheckFilenameListHashes();
			VerifyFilenameList();
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
			Logger logger = new Logger("Hash Matches.txt");
			logger.ClearFile();
			PackTools.ReadPKHFile("/Users/amberbrault/Documents/Xenoblade Decomp/Game Files/Pack Files/adx.pkh");
			PackHeader packHeader = PackTools.packHeader;
			List<ulong> fileHashes = packHeader.fileHashTable.ToList();

			for (int i = 0; i < fileHashes.Count; i++) {
				bool foundMatch = false;
				foreach (string path in PKHArchiveFiles.adxPkhFiles) {
					string filename = Path.GetFileName(path);
					ulong hash = PackFileHashUtil.CalculatePackFileHash(filename, packHeader.hashValTable);

					if (fileHashes[i] == hash) {
						Console.WriteLine("Found match for file {0}: {1}", i, path);
						logger.Log(string.Format("Found match for file {0}: {1}", i, path));
						foundMatch = true;
					}

					if (foundMatch) break;
				}

				if (!foundMatch) {
					Console.WriteLine("No match found for file {0}", i);
					logger.Log(string.Format("No match found for file {0}", i));
				}
			}
		}

		public static void VerifyFilenameList() {
			PackTools.ReadPKHFile("/Users/amberbrault/Documents/Xenoblade Decomp/Game Files/Pack Files/adx.pkh");
			PackHeader packHeader = PackTools.packHeader;
			List<ulong> fileHashes = packHeader.fileHashTable.ToList();
			bool foundMismatch = false;

			for (int i = 0; i < fileHashes.Count; i++) {
					string filename = Path.GetFileName(PKHArchiveFiles.adxPkhFiles[i]);
					ulong hash = PackFileHashUtil.CalculatePackFileHash(filename, packHeader.hashValTable);

					if (fileHashes[i] != hash) {
						Console.WriteLine("The hash of the filename for file " + i + " does not match");
					foundMismatch = true;
					}
			}

			if (!foundMismatch) Console.WriteLine("No mismatches found! :3");
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

