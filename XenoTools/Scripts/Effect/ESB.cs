using System;
using System.Collections.Generic;
using XenoTools.Scripts.Effect.Tables;

namespace XenoTools.Scripts.Effect
{

	public class Section {
		public int startOffset;
		public int endOffset;
		public TableType table;

		public Section(int startOffset, int endOffset, TableType table) {
			this.startOffset = startOffset;
			this.endOffset = endOffset;
			this.table = table;
		}
	}
	

	public class ESB
	{
		//Tables
		public SchedulerTable schedulerTable;
		public StructureTable structureTable;
		public List<ScriptTable> tables = new List<ScriptTable>();
		public List<Section> sections = new List<Section>();

		public ESB()
		{
		}
	}
}

