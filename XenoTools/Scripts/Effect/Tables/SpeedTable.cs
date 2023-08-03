using System;
using System.Collections.Generic;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class SpeedData {
		public ushort unk0;
		public ushort unk2;
		public float spd;
	}

	public class SpeedTable : ScriptTable {
		public ushort unk0;
		public ushort unk2;
		public List<SpeedData> entries = new List<SpeedData>();

		public const int headerLength = 4;
		public const int dataEntryLength = 8;

		public override int GetLength() {
			return headerLength + (dataEntryLength * entries.Count) + 2;
		}
	}
}

