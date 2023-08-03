using System;
using System.Collections.Generic;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class GoalData {
		public ushort unk0;
		public ushort loop;
		public Vector3 pos;
		public Vector3 range;
		public Vector3 unkVec;
	}

	public class GoalTable : ScriptTable {
		public ushort unk0;
		public byte unk2;
		public byte unk3;
		public ushort unk4;
		public ushort unk6;
		public ushort unk8;
		public byte[] unkA; //8 bytes
		public List<GoalData> entries = new List<GoalData>();

		public const int headerLength = 18;
		public const int dataEntryLength = 40;

		public override int GetLength() {
			return headerLength + (dataEntryLength * entries.Count) + 2;
		}
	}
}

