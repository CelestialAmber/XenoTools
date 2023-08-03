using System;
using System.Collections.Generic;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class SizeData {
		public ushort unk0;
		public ushort unk2;
		public Vector3 size;
	}


	public class SizeTable : ScriptTable {
		public ushort unk0;
		public ushort unk2;
		public ushort unk4;
		public Vector3 baseVec;
		public Vector3 unk10;
		public List<SizeData> entries = new List<SizeData>();

		public const int headerLength = 30;
		public const int dataEntryLength = 16;

		public override int GetLength() {
			return headerLength + (dataEntryLength * entries.Count) + 2;
		}
	}
}

