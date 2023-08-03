using System;
using System.Collections.Generic;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class ColorUInt16 {
		public ushort r, g, b, a;
	}

	public class RGBAData {
		public ushort unk0;
		public ushort loopSig;
		public ColorUInt16 color;
		public float unkC;
	}

	public class RGBATable : ScriptTable {
		public ushort unk0;
		public ushort unk2;
		public ushort unk4;
		public Vector3 vec1;
		public Vector3 vec2;
		public List<RGBAData> entries = new List<RGBAData>();

		public const int headerLength = 30;
		public const int dataEntryLength = 16;

		public override int GetLength() {
			return headerLength + (dataEntryLength * entries.Count) + 2;
		}
	}
}

