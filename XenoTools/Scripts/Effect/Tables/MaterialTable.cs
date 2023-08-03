using System;
using System.Collections.Generic;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class MaterialData {
		public ushort unk0;
		public ushort unk2;
		public ushort unk4;
		public ushort unk6;
	}

	public class MaterialTable : ScriptTable {
		public ushort unk0;
		public ushort uvTableOffset;
		public MatMode mode;
		public List<MaterialData> entries = new List<MaterialData>();

		public const int headerLength = 6;
		public const int dataEntryLength = 8;

		public override int GetLength() {
			return headerLength + (dataEntryLength * entries.Count) + 2;
		}
	}
}

