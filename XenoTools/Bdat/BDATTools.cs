using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XenoTools.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace XenoTools.Bdat
{
	public class TableCategory {
		//Type data (starts at offset 0x20)
		public BDATMemberType memberType; //usually 1 (single value)
		public BDATValueType valType;
		public short dataIndex;
		//Category data
		public short categoryTypeDataOffset;
		public short unk3;
		public string name;
	}

	public class BDATCsv {
		public string[] data;
		public string name;
		public byte[] originalData;

		public BDATCsv(string[] data, string name) {
			this.data = data;
			this.name = name;
		}
	}

	public class BDATTools {

		//Unpacks a .bin BDAT archive, containing multiple BDAT files back to back.
		public static BDATCsv[] UnpackBDATArchive(byte[] data) {
			int offset = 0;
			int files = MemoryUtils.ReadInt(offset, data);
			offset += 4;
			int archiveFileSize = MemoryUtils.ReadInt(offset, data);
			offset += 4;

			int[] bdatFileOffsets = new int[files];
			BDATCsv[] bdatCsvArray = new BDATCsv[files];

			for (int i = 0; i < files; i++) {
				bdatFileOffsets[i] = MemoryUtils.ReadInt(offset, data);
				offset += 4;
			}

			for (int i = 0; i < files; i++) {
				//Calculate the bdat file size
				int length = i < files - 1 ? bdatFileOffsets[i + 1] - bdatFileOffsets[i] : archiveFileSize - bdatFileOffsets[i];
				byte[] bdatData = data.Skip(bdatFileOffsets[i]).Take(length).ToArray();
				bdatCsvArray[i] = DecodeBDAT(bdatData);
				bdatCsvArray[i].originalData = bdatData;
			}

			return bdatCsvArray;
		}

		public static BDATCsv DecodeBDAT(byte[] data) {
			int offset = 0;
			string magic = System.Text.Encoding.Default.GetString(data.Take(4).ToArray());
			offset += 4;

			if (magic != "BDAT") {
				throw new Exception("Error: Invalid BDAT file.");
			}

			//0x0200: encrypted, 0x0000: not encrypted
			//always unencrypted for xc1?
			short encryptionFlag = MemoryUtils.ReadShort(offset, data); //0x4
			offset += 2;
			short tableCategoryDataOffset = MemoryUtils.ReadShort(offset, data); //0x6
			offset += 2;
			short entrySize = MemoryUtils.ReadShort(offset, data); //0x8
			offset += 2;
			short entriesDataOffset = MemoryUtils.ReadShort(offset, data); //0xA
			offset += 2;
			short entries = MemoryUtils.ReadShort(offset, data); //0xC
			offset += 2;
			short unknownOffset = MemoryUtils.ReadShort(offset, data); //0xE
			offset += 2;
			//The last four bytes seem to always be "00 40 00 01"
			short unk2 = MemoryUtils.ReadShort(offset, data); //0x10
			offset += 2;
			short unk3 = MemoryUtils.ReadShort(offset, data); //0x12
			offset += 2;

			List<TableCategory> categoryList = new List<TableCategory>();

			//The rest of the header seems unused (bytes 0x14-0x1F)

			int categories = (tableCategoryDataOffset - 0x20) / 4;

			offset = tableCategoryDataOffset;

			string tableName = MemoryUtils.ReadString(offset, data);
			offset += tableName.Length + 1;


			for (int i = 0; i < categories; i++) {
				TableCategory category = new TableCategory();
				//Read the category type data (starts at 0x20)
				category.memberType = (BDATMemberType)MemoryUtils.ReadByte(0x20 + i * 4, data);
				category.valType = (BDATValueType)MemoryUtils.ReadByte(0x20 + i * 4 + 1, data);
				category.dataIndex = MemoryUtils.ReadShort(0x20 + i * 4 + 2, data);

				category.categoryTypeDataOffset = MemoryUtils.ReadShort(offset, data);
				offset += 2;
				category.unk3 = MemoryUtils.ReadShort(offset, data);
				offset += 2;
				//Get the category name
				category.name = MemoryUtils.ReadString(offset, data);
				offset += category.name.Length + 1;
				if (offset % 2 == 1) offset++; //Category name strings are aligned to 2 bytes

				categoryList.Add(category);
			}

			List<byte[]> entriesData = new List<byte[]>();

			offset = entriesDataOffset;

			//Get all the separate entries' data and put them into a list
			for (int i = 0; i < entries; i++) {
				entriesData.Add(data.Skip(offset).Take(entrySize).ToArray());
				//Go to the next entry
				offset += entrySize;
			}

			//Convert the bdat file to csv

			List<string> lines = new List<string>();
			string categoryLine = "";

			for (int i = 0; i < categoryList.Count; i++) {
				categoryLine += categoryList[i].name + (i < categoryList.Count - 1 ? ", " : "");
			}

			//Write the category line
			lines.Add(categoryLine);

			//Convert each entry
			for (int i = 0; i < entries; i++) {
				string line = "";

				//Convert the values for each category/column to strings based on their type, and add them to the line
				for (int j = 0; j < categories; j++) {
					byte[] entryData = entriesData[i];
					TableCategory category = categoryList[j];
					//Convert the current value to a string, and add it to the line
					line += ReadValue(category, entryData, data);
					if (j < categories - 1) line += ", ";
				}

				//Write the current line
				lines.Add(line);
			}

			return new BDATCsv(lines.ToArray(),tableName);
		}

		static string ReadValue(TableCategory category, byte[] entryData, byte[] bdatData) {
			switch (category.valType) {
				case BDATValueType.None:
					throw new NotImplementedException();
				case BDATValueType.UInt8:
					return MemoryUtils.ReadByte(category.dataIndex, entryData).ToString();
				case BDATValueType.UInt16:
					return MemoryUtils.ReadUInt16(category.dataIndex, entryData).ToString();
				case BDATValueType.UInt32:
					return MemoryUtils.ReadUInt32(category.dataIndex, entryData).ToString();
				case BDATValueType.Int8:
					return MemoryUtils.ReadByte(category.dataIndex, entryData).ToString();
				case BDATValueType.Int16: //short
					return MemoryUtils.ReadShort(category.dataIndex, entryData).ToString();
				case BDATValueType.Int32: //int
					return MemoryUtils.ReadInt(category.dataIndex, entryData).ToString();
				case BDATValueType.String: //String pointer (16 bit)
					ushort stringOffset = MemoryUtils.ReadUInt16(category.dataIndex, entryData);
					//In the Japanese version, strings are in Shift-JIS
					//TODO: make this decode the string with the correct encoding based on region
					return MemoryUtils.ReadShiftJISString(stringOffset, bdatData);
				default:
					throw new NotImplementedException("Unknown data type id " + category.valType);
			}
		}
	}
}

