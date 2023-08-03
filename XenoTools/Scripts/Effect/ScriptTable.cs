using System;
using XenoTools.Scripts.Effect.Tables;

namespace XenoTools.Scripts.Effect
{
	//Interface for effect script tables.
	//All tables that have data entries end with a terminator
	//Life tables are an exception
	//The terminator seems to be 40 00
	//All offsets to tables with data entries point to the start of the data entries
	//This excludes: scheduler, structure, clip, and particle tables
	public abstract class ScriptTable {
		public int startOffset;
		public int length;

		public abstract int GetLength();
	}
}

