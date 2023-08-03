using System;
using System.Collections.Generic;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class AngleData {
		public ushort unk0;
		public ushort unk2;
		public Vector3 angle;
	}

	public class AngleTable : ScriptTable {
		public ushort unk0;
		public ushort unk2;
		public ushort unk4;
		public Vector3 unk8;
		public Vector3 unk14;
		public List<AngleData> entries = new List<AngleData>();

		public const int headerLength = 30;
		public const int dataEntryLength = 16;

		public override int GetLength() {
			return headerLength + (dataEntryLength * entries.Count) + 2;
		}
	}
}

