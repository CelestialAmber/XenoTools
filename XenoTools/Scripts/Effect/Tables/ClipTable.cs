using System;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class ClipTable : ScriptTable {
		public ushort near_start;
		public ushort near_end;
		public ushort far_start;
		public ushort far_end;

		public const int headerLength = 8;

		public override int GetLength() {
			return headerLength;
		}
	}
}

