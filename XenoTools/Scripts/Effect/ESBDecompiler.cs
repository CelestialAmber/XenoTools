using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using XenoTools.Scripts.Effect.Tables;
using XenoTools.Utils;

namespace XenoTools.Scripts.Effect
{
	public class ESBDecompiler
	{
		byte[] esbData;
		int offset;
		ESB esb;

		public ESBDecompiler(string path)
		{
			esbData = File.ReadAllBytes(path);
			offset = 0;
			esb = new ESB();
		}

		public void Decompile() {
			//ESB files always start with the scheduler table
			esb.schedulerTable = ReadSchedulerTable();
			//Go to the structure table offset
			offset = (int)esb.schedulerTable.structureTableOffset;
			esb.structureTable = ReadStructureTable();

			//Loop through each structure data entry in the structure table to find subtables
			foreach (StructureData entry in esb.structureTable.entries) {
				TryParseTable(TableType.Emitter, entry.emitterTableOffset);
				TryParseTable(TableType.Particle, entry.particleTableOffset);

				//Check for optional tables
				if (entry.clipTableOffset != 0) TryParseTable(TableType.Clip, entry.clipTableOffset);
			}

			List<EmitterTable> emitterTables = new List<EmitterTable>();

			foreach (ScriptTable table in esb.tables) {
				if (table.GetType() == typeof(EmitterTable)) {
					emitterTables.Add((EmitterTable)table);
				}
			}

			//Loop through each emitter table to find subtables
			foreach (EmitterTable table in emitterTables) {
				if (table.posTableOffset != 0) TryParseTable(TableType.Position, table.posTableOffset);
				if (table.goalTableOffset != 0) TryParseTable(TableType.Goal, table.goalTableOffset);
				if (table.creTableOffset != 0) TryParseTable(TableType.Cre, table.creTableOffset);
			}

			List<ParticleTable> particleTables = new List<ParticleTable>();

			foreach (ScriptTable table in esb.tables) {
				if (table.GetType() == typeof(ParticleTable)) {
					particleTables.Add((ParticleTable)table);
				}
			}

			//Loop through particle tables to find subtables
			foreach (ParticleTable table in particleTables) {
				if (table.lifeTableOffset != 0) TryParseTable(TableType.Life, table.lifeTableOffset);
				if (table.guiTableOffset != 0) TryParseTable(TableType.GUI, table.guiTableOffset);
				if (table.spdTableOffset != 0) TryParseTable(TableType.Speed, table.spdTableOffset);
				//Handle first 2 unknown tables here
				if (table.materialTableOffset != 0) TryParseTable(TableType.Material, table.materialTableOffset);
				if (table.angleTableOffset != 0) TryParseTable(TableType.Angle, table.angleTableOffset);
				if (table.sizeTableOffset != 0) TryParseTable(TableType.Size, table.sizeTableOffset);
				if (table.rgbaTableOffset != 0) TryParseTable(TableType.RGBA, table.rgbaTableOffset);
				//Handle last unknown table here
			}

			List<MaterialTable> materialTables = new List<MaterialTable>();

			foreach (ScriptTable table in esb.tables) {
				if (table.GetType() == typeof(MaterialTable)) {
					materialTables.Add((MaterialTable)table);
				}
			}

			//Finally, loop through material tables to find uv tables
			foreach (MaterialTable table in materialTables) {
				if(table.uvTableOffset != 0) TryParseTable(TableType.UV, table.uvTableOffset);
			}

			//Sort the sections and tables by offset
			esb.sections = esb.sections.OrderBy(o => o.startOffset).ToList();
			esb.tables = esb.tables.OrderBy(o => o.startOffset).ToList();

			PrintData();
		}


		void PrintData() {
			Console.WriteLine("Sections:");
			for(int i = 0; i < esb.sections.Count; i++) {
				Section section = esb.sections[i];
				Console.WriteLine("Section " + i + ": offset range: " + section.startOffset + "-" + section.endOffset
				+ ", type: " + section.table);
			}
		}


		void TryParseTable(TableType tableType, int sectionOffset) {
			//Most offsets need to be adjusted because Omiya thought it would be a wonderful idea
			//to have the offset point to the start of data entries :)
			offset = sectionOffset - GetTableOffsetFromData(tableType);

			//Only parse the table if it hasn't already been parsed
			if (CheckIfTableAlreadyParsed(offset) == false) {
				ScriptTable table;

				switch (tableType) {
					case TableType.Emitter:
						table = ReadEmitterTable();
						break;
					case TableType.Clip:
						table = ReadClipTable();
						break;
					case TableType.Position:
						table = ReadPositionTable();
						break;
					case TableType.Goal:
						table = ReadGoalTable();
						break;
					case TableType.Cre:
						table = ReadCreTable();
						break;
					case TableType.Particle:
						table = ReadParticleTable();
						break;
					case TableType.Life:
						table = ReadLifeTable();
						break;
					case TableType.GUI:
						table = ReadGUITable();
						break;
					case TableType.Speed:
						table = ReadSpeedTable();
						break;
					case TableType.Material:
						table = ReadMaterialTable();
						break;
					case TableType.UV:
						table = ReadUVTable();
						break;
					case TableType.Angle:
						table = ReadAngleTable();
						break;
					case TableType.Size:
						table = ReadSizeTable();
						break;
					case TableType.RGBA:
						table = ReadRGBATable();
						break;
					default:
						throw new NotImplementedException("Table type" + tableType + "is not yet supported");
				}

				esb.tables.Add(table);
			}
		}

		bool CheckIfTableAlreadyParsed(int tableOffset) {
			foreach (Section section in esb.sections) {
				if (section.startOffset == tableOffset) {
					return true;
				}
			}

			return false;
		}

		//Why is this even necessary...
		int GetTableOffsetFromData(TableType tableType) {
			switch (tableType) {
				case TableType.Emitter:
					return 0;
				case TableType.Clip:
					return 0;
				case TableType.Position:
					return PositionTable.headerLength;
				case TableType.Goal:
					return GoalTable.headerLength;
				case TableType.Cre:
					return CreTable.headerLength;
				case TableType.Particle:
					return 0;
				case TableType.Life:
					return LifeTable.headerLength;
				case TableType.GUI:
					return GUITable.headerLength;
				case TableType.Speed:
					return SpeedTable.headerLength;
				case TableType.Material:
					return MaterialTable.headerLength;
				case TableType.UV:
					return UVTable.headerLength;
				case TableType.Angle:
					return AngleTable.headerLength;
				case TableType.Size:
					return SizeTable.headerLength;
				case TableType.RGBA:
					return RGBATable.headerLength;
				default:
					throw new NotImplementedException("Table type" + tableType + "is not yet supported");
			}
		}


		//Helper functions to read table data

		public SchedulerTable ReadSchedulerTable() {
			SchedulerTable table = new SchedulerTable();
			table.startOffset = offset;
			

			table.structureTableOffset = MemoryUtils.ReadUInt32Update(ref offset, esbData);
			table.unk4 = MemoryUtils.ReadUInt16Update(ref offset, esbData);

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.Schedule));
			return table;
		}

		public StructureTable ReadStructureTable() {
			StructureTable table = new StructureTable();
			table.startOffset = offset;

			//Keep reading entries until we hit the terminator value
			while (!ReachedTerminatorByte()) {
				StructureData entry = new StructureData();
				entry.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk2 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.emitterTableOffset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.particleTableOffset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk8 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unkA = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unkC = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unkE = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk10 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.clipTableOffset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk14 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				table.entries.Add(entry);
			}

			offset += 2;

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.Structure));
			return table;
		}


		public ClipTable ReadClipTable() {
			ClipTable table = new ClipTable();
			table.startOffset = offset;

			table.near_start = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.near_end = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.far_start = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.far_end = MemoryUtils.ReadUInt16Update(ref offset, esbData);

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.Clip));
			return table;
		}

		public EmitterTable ReadEmitterTable() {
			EmitterTable table = new EmitterTable();
			table.startOffset = offset;

			table.posTableOffset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.goalTableOffset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.creTableOffset = MemoryUtils.ReadUInt16Update(ref offset, esbData);

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.Emitter));
			return table;
		}

		public PositionTable ReadPositionTable() {
			PositionTable table = new PositionTable();
			table.startOffset = offset;

			table.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk2 = MemoryUtils.ReadByteUpdate(ref offset, esbData);
			table.unk3 = MemoryUtils.ReadByteUpdate(ref offset, esbData);
			table.unk4 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk6 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk8 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unkA = esbData.Skip(offset).Take(8).ToArray();
			offset += 8;

			//Keep reading entries until we hit the terminator value
			while (!ReachedTerminatorByte()) {
				PositionData entry = new PositionData();
				entry.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.loop = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.pos = ReadVector3(esbData, ref offset);
				entry.range = ReadVector3(esbData, ref offset);
				entry.unkVec = ReadVector3(esbData, ref offset);
				table.entries.Add(entry);
			}

			offset += 2;

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.Position));
			return table;
		}

		public GoalTable ReadGoalTable() {
			GoalTable table = new GoalTable();
			table.startOffset = offset;


			table.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk2 = MemoryUtils.ReadByteUpdate(ref offset, esbData);
			table.unk3 = MemoryUtils.ReadByteUpdate(ref offset, esbData);
			table.unk4 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk6 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk8 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unkA = esbData.Skip(offset).Take(8).ToArray();
			offset += 8;

			//Keep reading entries until we hit the terminator value
			while (!ReachedTerminatorByte()) {
				GoalData entry = new GoalData();
				entry.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.loop = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.pos = ReadVector3(esbData, ref offset);
				entry.range = ReadVector3(esbData, ref offset);
				entry.unkVec = ReadVector3(esbData, ref offset);
				table.entries.Add(entry);
			}

			offset += 2;

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.Goal));
			return table;
		}

		public CreTable ReadCreTable() {
			CreTable table = new CreTable();
			table.startOffset = offset;

			table.unk0 = esbData.Skip(offset).Take(8).ToArray();
			offset += 8;

			//Keep reading entries until we hit the terminator value
			while (!!ReachedTerminatorByte()) {
				CreData entry = new CreData();
				entry.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk2 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk4 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk6 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				table.entries.Add(entry);
			}

			offset += 2;

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.Cre));
			return table;
		}


		public ParticleTable ReadParticleTable() {
			ParticleTable table = new ParticleTable();
			table.startOffset = offset;

			table.lifeTableOffset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.guiTableOffset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.spdTableOffset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unkSubtable1Offset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unkSubtable2Offset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.materialTableOffset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.angleTableOffset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.sizeTableOffset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.rgbaTableOffset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unkSubtable3Offset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.mode = (ParticleTableMode)MemoryUtils.ReadUInt16Update(ref offset, esbData);

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.Particle));
			return table;
		}

		public LifeTable ReadLifeTable() {
			LifeTable table = new LifeTable();
			table.startOffset = offset;

			table.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);

			//Keep reading entries until we hit the terminator value
			while (!ReachedTerminatorByte()) {
				LifeData entry = new LifeData();
				entry.lifeTime = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk4 = esbData.Skip(offset).Take(10).ToArray();
				offset += 10;
				table.entries.Add(entry);
			}

			offset += 2;

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.Life));
			return table;
		}

		public GUITable ReadGUITable() {
			GUITable table = new GUITable();
			table.startOffset = offset;

			table.unk0 = esbData.Skip(offset).Take(18).ToArray();
			offset += 18;

			//Keep reading entries until we hit the terminator value
			while (!ReachedTerminatorByte()) {
				GUIData entry = new GUIData();
				entry.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk2 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.pos = ReadVector3(esbData, ref offset);
				entry.pow = MemoryUtils.ReadFloatUpdate(ref offset, esbData);
				table.entries.Add(entry);
			}

			offset += 2;

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.GUI));
			return table;
		}

		public SpeedTable ReadSpeedTable() {
			SpeedTable table = new SpeedTable();
			table.startOffset = offset;

			table.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk2 = MemoryUtils.ReadUInt16Update(ref offset, esbData);

			//Keep reading entries until we hit the terminator value
			while (!ReachedTerminatorByte()) {
				SpeedData entry = new SpeedData();
				entry.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk2 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.spd = MemoryUtils.ReadFloatUpdate(ref offset, esbData);
				table.entries.Add(entry);
			}

			offset += 2;

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.Speed));
			return table;
		}

		public MaterialTable ReadMaterialTable() {
			MaterialTable table = new MaterialTable();
			table.startOffset = offset;

			table.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.uvTableOffset = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.mode = (MatMode)MemoryUtils.ReadUInt16Update(ref offset, esbData);

			//Keep reading entries until we hit the terminator value
			while (!ReachedTerminatorByte()) {
				MaterialData entry = new MaterialData();
				entry.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk2 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk4 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk6 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				table.entries.Add(entry);
			}

			offset += 2;

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.Material));
			return table;
		}

		public UVTable ReadUVTable() {
			UVTable table = new UVTable();
			table.startOffset = offset;

			table.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk2 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk4 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk6 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk8 = MemoryUtils.ReadUInt16Update(ref offset, esbData);

			//Keep reading entries until we hit the terminator value
			while (!ReachedTerminatorByte()) {
				UVData entry = new UVData();
				entry.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk2 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.cUV = ReadVector2(esbData, ref offset);
				entry.vec2 = ReadVector2(esbData, ref offset);
				table.entries.Add(entry);
			}

			offset += 2;

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.UV));
			return table;
		}


		public AngleTable ReadAngleTable() {
			AngleTable table = new AngleTable();
			table.startOffset = offset;

			table.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk2 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk4 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk8 = ReadVector3(esbData, ref offset);
			table.unk14 = ReadVector3(esbData, ref offset);

			//Keep reading entries until we hit the terminator value
			while (!ReachedTerminatorByte()) {
				AngleData entry = new AngleData();
				entry.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk2 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.angle = ReadVector3(esbData, ref offset);
				table.entries.Add(entry);
			}

			offset += 2;

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.Angle));
			return table;
		}

		public SizeTable ReadSizeTable() {
			SizeTable table = new SizeTable();
			table.startOffset = offset;

			table.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk2 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk4 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.baseVec = ReadVector3(esbData, ref offset);
			table.unk10 = ReadVector3(esbData, ref offset);

			//Keep reading entries until we hit the terminator value
			while (!ReachedTerminatorByte()) {
				SizeData entry = new SizeData();
				entry.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.unk2 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.size = ReadVector3(esbData, ref offset);
				table.entries.Add(entry);
			}

			offset += 2;

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.Size));
			return table;
		}

		public RGBATable ReadRGBATable() {
			RGBATable table = new RGBATable();
			table.startOffset = offset;

			table.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk2 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.unk4 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
			table.vec1 = ReadVector3(esbData, ref offset);
			table.vec2 = ReadVector3(esbData, ref offset);

			//Keep reading entries until we hit the terminator value
			while (!ReachedTerminatorByte()) {
				RGBAData entry = new RGBAData();
				entry.unk0 = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.loopSig = MemoryUtils.ReadUInt16Update(ref offset, esbData);
				entry.color = ReadColor(esbData, ref offset);
				entry.unkC = MemoryUtils.ReadFloatUpdate(ref offset, esbData);
				table.entries.Add(entry);
			}

			offset += 2;

			table.length = table.GetLength();
			esb.sections.Add(new Section(table.startOffset, offset - 1, TableType.RGBA));
			return table;
		}

		bool ReachedTerminatorByte() {
			return MemoryUtils.ReadUInt16(offset, esbData) == 0x4000;
		}

		Vector3 ReadVector3(byte[] data, ref int offset) {
			Vector3 vec = new Vector3();
			vec.x = MemoryUtils.ReadFloatUpdate(ref offset, data);
			vec.y = MemoryUtils.ReadFloatUpdate(ref offset, data);
			vec.z = MemoryUtils.ReadFloatUpdate(ref offset, data);
			return vec;
		}

		Vector2 ReadVector2(byte[] data, ref int offset) {
			Vector2 vec = new Vector2();
			vec.x = MemoryUtils.ReadFloatUpdate(ref offset, data);
			vec.y = MemoryUtils.ReadFloatUpdate(ref offset, data);
			return vec;
		}

		ColorUInt16 ReadColor(byte[] data, ref int offset) {
			ColorUInt16 col = new ColorUInt16();
			col.r = MemoryUtils.ReadUInt16Update(ref offset, data);
			col.g = MemoryUtils.ReadUInt16Update(ref offset, data);
			col.b = MemoryUtils.ReadUInt16Update(ref offset, data);
			col.a = MemoryUtils.ReadUInt16Update(ref offset, data);
			return col;
		}
	}
}

