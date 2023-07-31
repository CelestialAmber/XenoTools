using System;
using System.Collections.Generic;
using System.IO;
using XenoTools.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XenoTools.AiDat
{
	public struct AIAction {
		public byte unk0;
		public byte unk1;
		public byte unk2;
		public byte unk3;
		public byte condition1;
		public byte condition1Param;
		public byte condition2;
		public byte condition2Param;
		public byte condition3;
		public byte condition3Param;
		public byte attackID;
		public byte successPercentage;
	};

	public struct AIDatEntry {
		public byte type;
		public byte unk1; //always FF?
		public short fileId; //0x2
		public string filename;
		public byte actionEntriesNum;
		public byte unk17; //always FF?
		public AIAction[] actions;

		public AIDatEntry(byte type, byte unk1, short fileId, string filename, byte totalActions, byte unk17, AIAction[] actions) {
			this.type = type;
			this.unk1 = unk1;
			this.fileId = fileId;
			this.filename = filename;
			this.actionEntriesNum = totalActions;
			this.unk17 = unk17;
			this.actions = actions;
		}
	};

	public class AiDatTools
	{
		public static List<AIDatEntry> entries;

		
		public static void ConvertAiDatToJson(string aiDatPath) {
			entries = ReadAiDatFile(aiDatPath);

			string newPath = aiDatPath.Replace("bin","json");
			string jsonData = JValue.Parse(JsonConvert.SerializeObject(entries, new Newtonsoft.Json.Converters.StringEnumConverter())).ToString(Newtonsoft.Json.Formatting.Indented);
			File.WriteAllText(newPath, jsonData);
		}

		public static void ConvertAiDatToCsv(string aiDatPath) {
			entries = ReadAiDatFile(aiDatPath);
			List<string> csvData = new List<string>();

			//Add the category line
			csvData.Add("Type,unk1,ID,Name,Entries,unk17,Unknown 0,Unknown 1,Unknown 2,Unknown 3,Condition 1,Condition 1 param,Condition 2,Condition 2 param,Condition 3,Condition 3 param,Attack ID,%");

			foreach(AIDatEntry entry in entries) {
				//Add the entry data in its own line before each action
				csvData.Add(entry.type + "," + entry.unk1 + "," + entry.fileId + "," + entry.filename + "," + entry.actionEntriesNum + "," + entry.unk17 + ",,,,,,,,,,,,");
				int index = 0;
				foreach(AIAction action in entry.actions) {
					string curLine = ",,,,,," + action.unk0 + "," + action.unk1 + "," + action.unk2 + "," + action.unk3 + "," + action.condition1 + "," + action.condition1Param
					+ "," + action.condition2 + "," + action.condition2Param + "," + action.condition3 + "," + action.condition3Param + "," + action.attackID + "," + action.successPercentage;
					csvData.Add(curLine);
					index++;
				}
			}

			string newPath = aiDatPath.Replace("bin", "csv");
			File.WriteAllLines(newPath, csvData);
		}

		//For whatever reason Monolithsoft decided it would be a great idea to use little endian for this file :3
		public static List<AIDatEntry> ReadAiDatFile(string aiDatPath) {
			byte[] data = File.ReadAllBytes(aiDatPath);
			int offset = 0;
			List<AIDatEntry> entryList = new List<AIDatEntry>();

			short totalEntries = MemoryUtils.ReadShort(offset, data, true);
			offset += 2;

			for (int i = 0; i < totalEntries; i++) {
				//Console.WriteLine("Offset: " + offset);
				byte type = MemoryUtils.ReadByte(offset++, data);
				byte unk1 = MemoryUtils.ReadByte(offset++, data); //seems to always be FF
				short fileId = MemoryUtils.ReadShort(offset, data, true);
				offset += 2;
				string filename = MemoryUtils.ReadString(offset, data);
				offset += 16; //The string has 16 bytes of space reserved for it, with unused bytes being 0x20 (space)
				byte actionEntriesNum = MemoryUtils.ReadByte(offset++, data);
				byte unk17 = MemoryUtils.ReadByte(offset++, data); //seems to always be FF

				List<AIAction> actions = new List<AIAction>();

				//Console.WriteLine("Name: " + filename + ", number of actions: " + totalActions);

				//Read the data for each action, and add it to the temp list
				for (int j = 0; j < actionEntriesNum; j++) {
					AIAction action = new AIAction();

					action.unk0 = MemoryUtils.ReadByte(offset++, data);
					action.unk1 = MemoryUtils.ReadByte(offset++, data);
					action.unk2 = MemoryUtils.ReadByte(offset++, data);
					action.unk3 = MemoryUtils.ReadByte(offset++, data);
					action.condition1 = MemoryUtils.ReadByte(offset++, data);
					action.condition1Param = MemoryUtils.ReadByte(offset++, data);
					action.condition2 = MemoryUtils.ReadByte(offset++, data);
					action.condition2Param = MemoryUtils.ReadByte(offset++, data);
					action.condition3 = MemoryUtils.ReadByte(offset++, data);
					action.condition3Param = MemoryUtils.ReadByte(offset++, data);
					action.attackID = MemoryUtils.ReadByte(offset++, data);
					action.successPercentage = MemoryUtils.ReadByte(offset++, data);

					actions.Add(action);
				}

				//Add the entry to the list
				entryList.Add(new AIDatEntry(type, unk1, fileId, filename, actionEntriesNum, unk17, actions.ToArray()));
			}

			return entryList;
		}
	}
}

