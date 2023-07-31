using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XenoTools.Pack
{

	//PKB/PKH unpacker

	public class PackHeader {
		public uint files;
		/*If the file isn't an AFS archive, the filename is just ".afs" (only the case for
		snd.pkh, adx.pkh, and ahx.pkh) */
		public string afsFilename;
		public byte[] hashValTable;
		public ulong[] fileHashTable;
		//The file sizes and offsets are in terms of 0x800 byte chunks
		public uint[] fileSizeTable;
		//Only present if the file isn't an AFS archive
        public uint[] fileOffsetTable;

		public PackHeader(uint files, uint hashValTableLength, string afsFilename) {
			this.files = files;
			//Initialize the different arrays
			hashValTable = new byte[hashValTableLength];
			fileHashTable = new ulong[files];
			fileSizeTable = new uint[files];
			fileOffsetTable = new uint[files];
			this.afsFilename = afsFilename;
		}

		public bool isAFSArchive => afsFilename != ".afs";
    }

	public class PackTools
	{
		public static PackHeader packHeader;
		public static string archiveName;

		public static void Unpack(string pkbPath) {
			string pkhPath = pkbPath.Replace(".pkb", ".pkh");
			archiveName = Path.GetFileNameWithoutExtension(pkbPath);

			if (!Path.Exists(pkbPath)) {
				Console.WriteLine("Error: could not find .pkb file.");
				return;
			}
			if (!Path.Exists(pkhPath)) {
				Console.WriteLine("Error: could not find .pkh file.");
				return;
			}

			ReadPKHFile(pkhPath);

			if (packHeader.isAFSArchive) {
				Console.WriteLine("Extracting afs archive pkb files is not implemented");
				return;
			}

			UnpackPKB(pkbPath);
		}

		static void UnpackPKB(string path) {
			FileStream fs = File.OpenRead(path);

			string basePath = "out/";

			Console.WriteLine("Unpacking " + path + "...");

			/*Check to see if there's a corresponding filename array, and if so use it to name the files
			while unpacking. If not, fallback to generic names. */

			bool usingFilenameArray = true;
			string[] filenames = new string[0];

			switch (Path.GetFileNameWithoutExtension(path)) {
				case "adx":
					filenames = PKHArchiveFiles.adxPkhFiles;
					break;
				case "chr":
					filenames = PKHArchiveFiles.chrPkhFiles;
					break;
				case "common":
					filenames = PKHArchiveFiles.commonPkhJpFiles;
					break;
				case "eff":
					filenames = PKHArchiveFiles.effPkhFiles;
					break;
				case "map":
					filenames = PKHArchiveFiles.mapPkhFiles;
					break;
				case "menu":
					filenames = PKHArchiveFiles.menuPkhJpFiles;
					break;
				case "obj":
					filenames = PKHArchiveFiles.objPkhFiles;
					break;
				case "script":
					filenames = PKHArchiveFiles.scriptPkhJpFiles;
					break;
				case "snd":
					filenames = PKHArchiveFiles.sndPkhFiles;
					break;
				default:
					//Fallback to generic names
					usingFilenameArray = false;
					break;
			}

			if (!Directory.Exists(basePath)) {
				Directory.CreateDirectory(basePath);
			}

			for(int i = 0; i < packHeader.files; i++) {
				uint fileOffset = packHeader.fileOffsetTable[i];
				uint fileSize = packHeader.fileSizeTable[i];
				byte[] buffer = new byte[fileSize];
				fs.Read(buffer, 0, (int)fileSize);
				List<byte> fileBytes = buffer.ToList();
				bool isTextFile = false;

				string filePath;

				//If there is a corresponding filename list, use it, or else fallback to generic names
				if (usingFilenameArray && filenames[i] != "") {
					filePath = filenames[i];
					string extension = Path.GetExtension(filePath);

					//Check if the file is a text file (only checks .t for now)
					if(extension == ".t") {
						isTextFile = true;
					}
				} else {
					//Try to determine the file extension of the current file based on its contents
					//string fileExtension = DetermineFileExtension(buffer);
					filePath = archiveName + "/" + i + ".bin"; // fileExtension;
					//if (fileExtension == ".t") isTextFile = true;
				}

				//Remove any zero bytes if this file is a text file
				if (isTextFile) {
					int firstZeroIndex = fileBytes.IndexOf(0);
					int bytesToRemove = (int)fileSize - firstZeroIndex;
					fileBytes.RemoveRange(firstZeroIndex, bytesToRemove);
				}

				string outPath = basePath + filePath;

				if (!Directory.Exists(Path.GetDirectoryName(outPath))){
					Directory.CreateDirectory(Path.GetDirectoryName(outPath));
				}

				Console.WriteLine("Unpacking " + filePath);

				File.WriteAllBytes(outPath,fileBytes.ToArray());
			}
		}

		//Attempts to determine the file extension of the given file
		static string DetermineFileExtension(byte[] fileData) {
			if(archiveName == "script") {
				//.t file (seems to always start with //=====)
				if (Encoding.Default.GetString(fileData.Take(3).ToArray()) == "//=") {
					return ".t";
				}

				//Binary script (.sb)
				if(Encoding.Default.GetString(fileData.Take(4).ToArray()) == "SB  ") {
					return ".sb";
				}
			}

			//Return .bin by default
			return ".bin";
		}

		public static void ReadPKHFile(string path) {
			byte[] data = File.ReadAllBytes(path);
			archiveName = Path.GetFileNameWithoutExtension(path);

			//The first 8 bytes are unused? always 00FE1200 and 00000002
			//These values might relate to the version?
			int offset = 8;

			uint fileHashTableOffset = ReadUInt32(data, offset); //always 0x78
			offset += 4;
			uint fileSize = ReadUInt32(data, offset);
			offset += 4;
			uint files = ReadUInt32(data, offset);
			offset += 4;

			string afsFilename = "";

			//Read the afs filename string (usually ".afs", except for the adx, ahx, and snd pkh files)
			while (data[offset] != 0) {
				afsFilename += (char)(data[offset++]);
			}

			offset++; //Skip the terminator byte

			//The bytes after the string and until 0x34 seem unused
			offset = 0x34;

			uint hashValTableLength = ReadUInt32(data, offset);
			offset += 4;

			packHeader = new PackHeader(files, hashValTableLength, afsFilename);

			//Read the hash value table
			for (int i = 0; i < hashValTableLength; i++) {
				packHeader.hashValTable[i] = data[offset++];
			}

			//Skip past the rest of the bytes reserved for the table (usually unused)
			offset = (int)fileHashTableOffset;

			//Read the file hash table
			for (int i = 0; i < files; i++) {
				uint hashLowVal = ReadUInt32(data, offset);
				offset += 4;
				uint hashHighVal = ReadUInt32(data, offset);
				offset += 4;
				packHeader.fileHashTable[i] = (hashHighVal << 32) + hashLowVal;
			}

			//Read the file size table
			for (int i = 0; i < files; i++) {
				//Multiply by 0x800 to get the actual file size (number of chunks * 0x800)
				packHeader.fileSizeTable[i] = (uint)(ReadUInt16(data, offset) * 0x800);
				offset += 2;
			}

			//If the pkh/pkb archive isn't an afs archive, the pkh file also has a file offset table
			if (!packHeader.isAFSArchive) {
				//Read the file offset table
				for (int i = 0; i < files; i++) {
					//Multiply by 0x800 to get the actual file offset (chunk offset * 0x800)
					packHeader.fileOffsetTable[i] = (uint)(ReadUInt32(data, offset) * 0x800);
					offset += 4;
				}
			}

			Console.WriteLine("Pack archive info: ");
			Console.WriteLine("Files in archive: " + packHeader.files);
			Console.WriteLine("AFS filename: " + packHeader.afsFilename);
			Console.WriteLine("Hash value table size: " + packHeader.hashValTable.Length);

			Console.WriteLine("Hash value table: ");
			for (int i = 0; i < packHeader.hashValTable.Length; i++) {
				Console.Write(packHeader.hashValTable[i] + (i < packHeader.hashValTable.Length - 1 ? "," : "\n"));
			}

			Console.WriteLine();

			//SavePKHTablesToTextFile();
		}

		public static void SavePKHTablesToTextFile() {
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("File hashes:");

			for (int i = 0; i < packHeader.files; i++) {
				ulong hash = packHeader.fileHashTable[i];
				int bitsToPrint = packHeader.hashValTable.Length;
				sb.AppendLine(hash.ToString("X16"));
			}

			sb.AppendLine();
			sb.AppendLine("File sizes:");


			for (int i = 0; i < packHeader.files; i++) {
				sb.AppendLine("0x" + packHeader.fileSizeTable[i].ToString("X4"));
			}

			//If the pkh/pkb archive isn't an afs archive, the pkh file also has a file offset table
			if (!packHeader.isAFSArchive) {
				sb.AppendLine();
				sb.AppendLine("File offsets:");
				//Read the file offset table
				for (int i = 0; i < packHeader.files; i++) {
					sb.AppendLine("0x" + packHeader.fileOffsetTable[i].ToString("X8"));
				}
			}

			File.WriteAllText(archiveName + "_pkhtables.txt", sb.ToString());
		}

		static ushort ReadUInt16(byte[] data, int offset) {
			return BitConverter.ToUInt16(data.Skip(offset).Take(2).Reverse().ToArray());
		}

		static uint ReadUInt32(byte[] data, int offset){
			return BitConverter.ToUInt32(data.Skip(offset).Take(4).Reverse().ToArray());
		}
	}
}

