using System;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect.Tables
{
	public class ParticleTable : ScriptTable {
		public ushort lifeTableOffset;
		public ushort guiTableOffset;
		public ushort spdTableOffset;
		public ushort unkSubtable1Offset;
		public ushort unkSubtable2Offset;
		public ushort materialTableOffset;
		public ushort angleTableOffset;
		public ushort sizeTableOffset;
		public ushort rgbaTableOffset;
		public ushort unkSubtable3Offset;
		public ParticleTableMode mode;

		public const int headerLength = 22;

		public override int GetLength() {
			return headerLength;
		}
	}
}

