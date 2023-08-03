using System;
using System.Collections.Generic;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class StructureData {
		public ushort unk0;
		public ushort unk2;
		public ushort emitterTableOffset;
		public ushort particleTableOffset;
		public ushort unk8;
		public ushort unkA;
		public ushort unkC;
		public ushort unkE;
		public ushort unk10;
		public ushort clipTableOffset;
		public ushort unk14;
	}

	public class StructureTable : ScriptTable {
		public List<StructureData> entries = new List<StructureData>();

		public const int dataEntryLength = 22;

		public override int GetLength() {
			return (dataEntryLength * entries.Count) + 2;
		}
	}
}

