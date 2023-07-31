using System;
using System.Text;

namespace XenoTools.Scripts.SB
{
	public class Instruction
	{
		public string opcodeName;
		public int unk1;
		public Opcode opcode;
		public int param;
		public int paramSize; //0: no param, 1: 8 bit, 2: 16 bit, 4: 32 bit
		public int address;


		public Instruction(byte opcodeNum, int param, int address) {
			OpcodeInfo info = Opcodes.opcodes[opcodeNum];
			this.opcodeName = info.name;
			this.opcode = (Opcode)opcodeNum;
			this.param = param;
			this.paramSize = info.size;
			this.unk1 = info.unk1;
			this.address = address;
		}


		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append(opcodeName + " " + param);
			return sb.ToString();
		}

		//Parses the next instruction from the current position in the array, and returns it.
		public static Instruction Parse(byte[] data, ref int offset)
		{
			int instructionAddress = offset;
			//Get the opcode index
			byte opcodeNum = data[offset++];
			//Get the opcode info
			OpcodeInfo info = Opcodes.opcodes[opcodeNum];
			//Parse the parameter of the current instruction
			int val = ParseParam(data, ref offset, info.size);

			return new Instruction(opcodeNum, val, instructionAddress);
		}


		static int ParseParam(byte[] data, ref int offset, int paramSize) {
			int val = 0;

			switch (paramSize) {
				case 0:
					val = 0;
					break;
				case 1:
					val = data[offset++];
					break;
				case 2:
					val = BitConverter.ToUInt16(data, offset);
					offset += 2;
					break;
				case 4:
					val = BitConverter.ToInt32(data, offset);
					offset += 4;
					break;
			}

			return val;
		}
	}
}

