using System;
using System.Collections.Generic;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class UVData {
		public ushort unk0;
		public ushort unk2;
		public Vector2 cUV;
		public Vector2 vec2;
	}

	public class UVTable : ScriptTable {
		public ushort unk0;
		public ushort unk2;
		public ushort unk4;
		public ushort unk6;
		public ushort unk8;
		public List<UVData> entries = new List<UVData>();

		public const int headerLength = 10;
		public const int dataEntryLength = 20;

		public override int GetLength() {
			return headerLength + (dataEntryLength * entries.Count) + 2;
		}
	}
}

