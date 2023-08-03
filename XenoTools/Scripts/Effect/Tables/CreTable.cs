using System;
using System.Collections.Generic;
using System.Linq;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class CreData {
		public ushort unk0;
		public ushort unk2;
		public ushort unk4;
		public ushort unk6;
	}


	public class CreTable : ScriptTable {
		public byte[] unk0; //8 bytes
		public List<CreData> entries = new List<CreData>();

		public const int headerLength = 8;
		public const int dataEntryLength = 8;

		public override int GetLength() {
			return headerLength + (dataEntryLength * entries.Count) + 2;
		}
	}
}

