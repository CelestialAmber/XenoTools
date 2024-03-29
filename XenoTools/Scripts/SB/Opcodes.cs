﻿using System;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace XenoTools.Scripts.SB {

	public class OpcodeInfo {
		public string name;
		public short size;
		public short unk1;

		public OpcodeInfo(string name, short size, short unk1) {
			this.name = name;
			this.size = size;
			this.unk1 = unk1;
		}
	}

	public class Opcodes {

		public static OpcodeInfo[] opcodes = {
		new OpcodeInfo("NOP", 0, 0),
		new OpcodeInfo("CONST_0", 0, 1),
		new OpcodeInfo("CONST_1", 0, 1),
		new OpcodeInfo("CONST_2", 0, 1),
		new OpcodeInfo("CONST_3", 0, 1),
		new OpcodeInfo("CONST_4", 0, 1),
		new OpcodeInfo("CONST_I", 1, 1),
		new OpcodeInfo("CONST_I_W", 2, 1),
		new OpcodeInfo("POOL_INT", 1, 1),
		new OpcodeInfo("POOL_INT_W", 2, 1),
		new OpcodeInfo("POOL_FIXED", 1, 1),
		new OpcodeInfo("POOL_FIXED_W", 2, 1),
		new OpcodeInfo("POOL_STR", 1, 1),
		new OpcodeInfo("POOL_STR_W", 2, 1),
		new OpcodeInfo("LD", 1, 1),
		new OpcodeInfo("ST", 1, -1),
		new OpcodeInfo("LD_ARG", 1, 1),
		new OpcodeInfo("ST_ARG", 1, -1),
		new OpcodeInfo("ST_ARG_OMIT", 1, -1),
		new OpcodeInfo("LD_0", 0, 1),
		new OpcodeInfo("LD_1", 0, 1),
		new OpcodeInfo("LD_2", 0, 1),
		new OpcodeInfo("LD_3", 0, 1),
		new OpcodeInfo("ST_0", 0, -1),
		new OpcodeInfo("ST_1", 0, -1),
		new OpcodeInfo("ST_2", 0, -1),
		new OpcodeInfo("ST_3", 0, -1),
		new OpcodeInfo("LD_ARG_0", 0, 1),
		new OpcodeInfo("LD_ARG_1", 0, 1),
		new OpcodeInfo("LD_ARG_2", 0, 1),
		new OpcodeInfo("LD_ARG_3", 0, 1),
		new OpcodeInfo("ST_ARG_0", 0, -1),
		new OpcodeInfo("ST_ARG_1", 0, -1),
		new OpcodeInfo("ST_ARG_2", 0, -1),
		new OpcodeInfo("ST_ARG_3", 0, -1),
		new OpcodeInfo("LD_STATIC", 1, 1),
		new OpcodeInfo("LD_STATIC_W", 2, 1),
		new OpcodeInfo("ST_STATIC", 1, -1),
		new OpcodeInfo("ST_STATIC_W", 2, -1),
		new OpcodeInfo("LD_AR", 0, -1),
		new OpcodeInfo("ST_AR", 0, -3),
		new OpcodeInfo("LD_NIL", 0, 1),
		new OpcodeInfo("LD_TRUE", 0, 1),
		new OpcodeInfo("LD_FALSE", 0, 1),
		new OpcodeInfo("LD_FUNC", 1, 1),
		new OpcodeInfo("LD_FUNC_W", 2, 1),
		new OpcodeInfo("LD_PLUGIN", 1, 1),
		new OpcodeInfo("LD_PLUGIN_W", 2, 1),
		new OpcodeInfo("LD_FUNC_FAR", 1, 1),
		new OpcodeInfo("LD_FUNC_FAR_W", 2, 1),
		new OpcodeInfo("MINUS", 0, 0),
		new OpcodeInfo("NOT", 0, -1),
		new OpcodeInfo("L_NOT", 0, -1),
		new OpcodeInfo("ADD", 0, -1),
		new OpcodeInfo("SUB", 0, -1),
		new OpcodeInfo("MUL", 0, -1),
		new OpcodeInfo("DIV", 0, -1),
		new OpcodeInfo("MOD", 0, -1),
		new OpcodeInfo("OR", 0, -1),
		new OpcodeInfo("AND", 0, -1),
		new OpcodeInfo("R_SHIFT", 0, -1),
		new OpcodeInfo("L_SHIFT", 0, -1),
		new OpcodeInfo("EQ", 0, -1),
		new OpcodeInfo("NE", 0, -1),
		new OpcodeInfo("GT", 0, -1),
		new OpcodeInfo("LT", 0, -1),
		new OpcodeInfo("GE", 0, -1),
		new OpcodeInfo("LE", 0, -1),
		new OpcodeInfo("L_OR", 0, -1),
		new OpcodeInfo("L_AND", 0, -1),
		new OpcodeInfo("JMP", 2, 0),
		new OpcodeInfo("JPF", 2, -1),
		new OpcodeInfo("CALL", 1, 0),
		new OpcodeInfo("CALL_W", 2, 0),
		new OpcodeInfo("CALL_IND", 0, 0),
		new OpcodeInfo("RET", 0, 0),
		new OpcodeInfo("NEXT", 0, 0),
		new OpcodeInfo("PLUGIN", 1, 0),
		new OpcodeInfo("PLUGIN_W", 2, 0),
		new OpcodeInfo("CALL_FAR", 1, 0),
		new OpcodeInfo("CALL_FAR_W", 2, 0),
		new OpcodeInfo("GET_OC", 1, 0),
		new OpcodeInfo("GET_OC_W", 2, 0),
		new OpcodeInfo("GETTER", 1, 0),
		new OpcodeInfo("GETTER_W", 2, 0),
		new OpcodeInfo("SETTER", 1, -1),
		new OpcodeInfo("SETTER_W", 2, -1),
		new OpcodeInfo("SEND", 1, 0),
		new OpcodeInfo("SEND_W", 2, 0),
		new OpcodeInfo("TYPEOF", 0, 0),
		new OpcodeInfo("SIZEOF", 0, 0),
		new OpcodeInfo("SWITCH", 1, 0),
		new OpcodeInfo("INC", 0, 0),
		new OpcodeInfo("DEC", 0, 0),
		new OpcodeInfo("EXIT", 0, 0),
		new OpcodeInfo("BP", 0, 0)
	};
	}

	public enum Opcode {
		NOP,
		CONST_0,
		CONST_1,
		CONST_2,
		CONST_3,
		CONST_4,
		CONST_I,
		CONST_I_W,
		POOL_INT,
		POOL_INT_W,
		POOL_FIXED,
		POOL_FIXED_W,
		POOL_STR,
		POOL_STR_W,
		LD,
		ST,
		LD_ARG,
		ST_ARG,
		ST_ARG_OMIT,
		LD_0,
		LD_1,
		LD_2,
		LD_3,
		ST_0,
		ST_1,
		ST_2,
		ST_3,
		LD_ARG_0,
		LD_ARG_1,
		LD_ARG_2,
		LD_ARG_3,
		ST_ARG_0,
		ST_ARG_1,
		ST_ARG_2,
		ST_ARG_3,
		LD_STATIC,
		LD_STATIC_W,
		ST_STATIC,
		ST_STATIC_W,
		LD_AR,
		ST_AR,
		LD_NIL,
		LD_TRUE,
		LD_FALSE,
		LD_FUNC,
		LD_FUNC_W,
		LD_PLUGIN,
		LD_PLUGIN_W,
		LD_FUNC_FAR,
		LD_FUNC_FAR_W,
		MINUS,
		NOT,
		L_NOT,
		ADD,
		SUB,
		MUL,
		DIV,
		MOD,
		OR,
		AND,
		R_SHIFT,
		L_SHIFT,
		EQ,
		NE,
		GT,
		LT,
		GE,
		LE,
		L_OR,
		L_AND,
		JMP,
		JPF,
		CALL,
		CALL_W,
		CALL_IND,
		RET,
		NEXT,
		PLUGIN,
		PLUGIN_W,
		CALL_FAR,
		CALL_FAR_W,
		GET_OC,
		GET_OC_W,
		GETTER,
		GETTER_W,
		SETTER,
		SETTER_W,
		SEND,
		SEND_W,
		TYPEOF,
		SIZEOF,
		SWITCH,
		INC,
		DEC,
		EXIT,
		BP
	}
}

