using System;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class EmitterTable : ScriptTable {
		public ushort posTableOffset;
		public ushort goalTableOffset;
		public ushort creTableOffset;

		public const int headerLength = 6;

		public override int GetLength() {
			return headerLength;
		}
	}
}

