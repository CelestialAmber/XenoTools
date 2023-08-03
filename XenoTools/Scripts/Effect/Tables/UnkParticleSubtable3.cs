using System;
using System.Collections.Generic;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class UnkParticleSubtable3Data {
		public ushort unk0;
		public ushort unk2;
		public Vector3 vec;
	}

	public class UnkParticleSubtable3 : ScriptTable {
		public ushort unk0;
		public ushort unk2;
		public ushort unk4;
		public List<UnkParticleSubtable3Data> entries;

		const int headerLength = 6;
		const int dataEntryLength = 16;

		public override int GetLength() {
			return headerLength + (dataEntryLength * entries.Count) + 2;
		}
	}
}

