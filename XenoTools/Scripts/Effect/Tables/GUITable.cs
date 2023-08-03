using System;
using System.Collections.Generic;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class GUIData {
		public ushort unk0;
		public ushort unk2;
		public Vector3 pos;
		public float pow;
	}

	public class GUITable : ScriptTable {
		public byte[] unk0; //18 bytes
		public List<GUIData> entries = new List<GUIData>();

		public const int headerLength = 18;
		public const int dataEntryLength = 20;

		public override int GetLength() {
			return headerLength + (dataEntryLength * entries.Count) + 2;
		}
	}

}

