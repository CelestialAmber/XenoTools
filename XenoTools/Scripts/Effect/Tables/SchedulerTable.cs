using System;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class SchedulerTable : ScriptTable {
		public uint structureTableOffset;
		public ushort unk4; //always FFFF?

		public const int headerLength = 6;

		public override int GetLength() {
			return headerLength;
		}
	}
}

