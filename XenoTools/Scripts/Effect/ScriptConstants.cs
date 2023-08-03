using System;
namespace XenoTools.Scripts.Effect
{

	public enum TableType {
		Schedule,
		Structure,
		Clip,
		Emitter,
		Position,
		Goal,
		Cre,
		Particle,
		Life,
		GUI,
		Speed,
		Unknown1,
		Unknown2,
		Material,
		UV,
		Angle,
		Size,
		RGBA,
		Unknown3
	}

	public enum MatMode {
		billboard,
		billboardY,
		fit,
		polXY,
		polXZ,
		chain
	}

	public enum ParticleTableMode {
		NORMAL,
		MODE_1,
		MODE_2,
		MODE_3,
		BLUR2,
		BLUR3,
		BLUR4,
		MONO,
		MODE_8,
		PASS_CURVE,
		MODE_10,
		MODE_11,
		DISTORTION,
		LIGHT
	}

	public class ScriptConstants
	{

		public static string[] particleTableModeNames = {
			"PASS_CURVE",
			"",
			"",
			"DISTORTION"
		};

		public static string[] clipTableParams = {
			"near_start",
			"near_end",
			"far_start",
			"far_end"
		};

		public static string[] tableNames = {
			"sch",
			"str",
			"clip",
			"emitter",
			"pos",
			"goal",
			"cre",
			"particle",
			"life",
			"gui",
			"spd",
			"",
			"",
			"mat",
			"uv",
			"ang",
			"size",
			"rgba"
		};



	}
}

