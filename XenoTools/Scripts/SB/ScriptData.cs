using System;
using System.Linq;
using System.Text;
using System.IO;
using XenoTools.Utils;
using System.Collections.Generic;

namespace XenoTools.Scripts.SB
{
	public class ScriptData {

		//Section offsets
		public int codeOffset;
		public int idPoolOffset;
		public int intPoolOffset;
		public int fixedPoolOffset;
		public int stringPoolOffset;
		public int functionPoolOffset;
		public int pluginImportsOffset;
		public int ocImportsOffset;
		public int functionImportsOffset;
		public int staticVarsOffset;
		public int localPoolOffset;
		public int systemAttrPoolOffset;
		public int userAttrPoolOffset;
		public int debugSymbolsOffset;

		//Raw section data
		public byte[] codeData;
		public byte[] idPoolData;
		public byte[] intPoolData;
		public byte[] fixedPoolData;
		public byte[] stringPoolData;
		public byte[] functionPoolData;
		public byte[] pluginImportsData;
		public byte[] ocImportsData;
		public byte[] functionImportsData;
		public byte[] staticVarsData;
		public byte[] localPoolData;
		public byte[] sysAttrPoolData;
		public byte[] userAttrPoolData;
		public byte[] debugSymbolsData;

		public byte[] data;

		public Instruction[] instructions;


		public ScriptData(byte[] data)
		{
			this.data = data;
			ParseScriptData();
		}

		void ParseScriptData() {
			int offset = 0;
			string magic = Encoding.Default.GetString(data.Take(4).ToArray());
			offset += 4;

			if (magic != "SB  ") {
				throw new Exception("Error: Invalid SB script.");
			}

			byte version = MemoryUtils.ReadByte(offset++, data); //0x4
			offset++; //byte at 0x5 is unused?
			byte flags = MemoryUtils.ReadByte(offset++, data); //0x6
			offset++; //the byte at offset 0x7 is reserved by the vm as a place to keep the loaded flag

			//Read the section offsets
			codeOffset = MemoryUtils.ReadInt(offset, data); //0x8
			offset += 4;
			idPoolOffset = MemoryUtils.ReadInt(offset, data); //0xC
			offset += 4;
			intPoolOffset = MemoryUtils.ReadInt(offset, data); //0x10
			offset += 4;
			fixedPoolOffset = MemoryUtils.ReadInt(offset, data); //0x14
			offset += 4;
			stringPoolOffset = MemoryUtils.ReadInt(offset, data); //0x18
			offset += 4;
			functionPoolOffset = MemoryUtils.ReadInt(offset, data); //0x1C
			offset += 4;
			pluginImportsOffset = MemoryUtils.ReadInt(offset, data); //0x20
			offset += 4;
			ocImportsOffset = MemoryUtils.ReadInt(offset, data); //0x24
			offset += 4;
			functionImportsOffset = MemoryUtils.ReadInt(offset, data); //0x28
			offset += 4;
			staticVarsOffset = MemoryUtils.ReadInt(offset, data); //0x2C
			offset += 4;
			localPoolOffset = MemoryUtils.ReadInt(offset, data); //0x30
			offset += 4;
			systemAttrPoolOffset = MemoryUtils.ReadInt(offset, data); //0x34
			offset += 4;
			userAttrPoolOffset = MemoryUtils.ReadInt(offset, data); //0x38
			offset += 4;
			debugSymbolsOffset = MemoryUtils.ReadInt(offset, data); //0x3C
			offset += 4;

			PrintSectionOffsets();
			ParseCodeSection(codeOffset);

			File.WriteAllBytes("scriptcode.bin", codeData);

		}

		public void PrintSectionOffsets() {

			Console.WriteLine("Code offset: 0x{0}", codeOffset.ToString("X"));
			Console.WriteLine("ID Pool offset: 0x{0}", idPoolOffset.ToString("X"));
			Console.WriteLine("Int Pool offset: 0x{0}", intPoolOffset.ToString("X"));
			Console.WriteLine("Fixed Pool offset: 0x{0}", fixedPoolOffset.ToString("X"));
			Console.WriteLine("String Pool offset: 0x{0}", stringPoolOffset.ToString("X"));
			Console.WriteLine("Function Pool offset: 0x{0}", functionPoolOffset.ToString("X"));
			Console.WriteLine("Plugin Imports offset: 0x{0}", pluginImportsOffset.ToString("X"));
			Console.WriteLine("OC Imports offset: 0x{0}", ocImportsOffset.ToString("X"));
			Console.WriteLine("Function Imports offset: 0x{0}", functionImportsOffset.ToString("X"));
			Console.WriteLine("Static Vars offset: 0x{0}", staticVarsOffset.ToString("X"));
			Console.WriteLine("Local Pool offset: 0x{0}", localPoolOffset.ToString("X"));
			Console.WriteLine("System Attributes Pool offset: 0x{0}", systemAttrPoolOffset.ToString("X"));
			Console.WriteLine("User Attributes Pool offset: 0x{0}", userAttrPoolOffset.ToString("X"));
			Console.WriteLine("Debug Symbols offset: 0x{0}", debugSymbolsOffset.ToString("X"));
		}

		void ParseCodeSection(int offset) {
			int length = MemoryUtils.ReadInt(offset + 8, data);
			int codeOffset = offset + MemoryUtils.ReadInt(offset, data);
			byte[] sectionData = data.Skip(codeOffset).Take(length).ToArray();
			//DescrambleSection(sectionData);
			codeData = sectionData;
			offset = codeOffset;

			List<Instruction> instructionsList = new List<Instruction>();

			while(offset < codeOffset + length) {
				Instruction instruction = Instruction.Parse(data, ref offset);
				instructionsList.Add(instruction);
				Console.WriteLine(instruction.opcodeName);
			}

			foreach(Instruction instruction in instructionsList) {
				Console.WriteLine(instruction.opcodeName);
			}
		}

		void DescrambleSection(byte[] sectionBytes) {
			if(sectionBytes.Length % 4 != 0) {
				Console.WriteLine("Warning: section length is not a multiple of 4.");
			}

			//Rotate each group of 4 bytes by 2 to the right
			for(int i = 0; i < sectionBytes.Length; i += 4) {
				uint groupVal = MemoryUtils.ReadUInt32(i, sectionBytes);
				groupVal = RotateRight(groupVal, 2);
				sectionBytes[i] = (byte)((groupVal >> 24) & 0xFF);
				sectionBytes[i + 1] = (byte)((groupVal >> 16) & 0xFF);
				sectionBytes[i + 2] = (byte)((groupVal >> 8) & 0xFF);
				sectionBytes[i + 3] = (byte)(groupVal & 0xFF);
			}
		}

		uint RotateRight(uint val, int amount) {
			return (val >> amount) | (val << (32 - amount));
		}
	}
}

