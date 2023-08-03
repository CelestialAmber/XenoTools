using System;
using System.Collections.Generic;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class LifeData {
		public ushort lifeTime;
		public byte[] unk4; //10 bytes
	}

	public class LifeTable : ScriptTable {
		public ushort unk0;
		public List<LifeData> entries = new List<LifeData>(); //seems to always have 1 entry

		public const int headerLength = 2;
		public const int dataEntryLength = 12;

		public override int GetLength() {
			return headerLength + (dataEntryLength * entries.Count) + 2;
		}
	}
}

