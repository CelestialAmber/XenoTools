using System;
using System.Linq;
using System.Text;
using XenoTools.Utils;

namespace XenoTools.Scripts.SB
{
	public class SBDisassembler
	{

		ScriptData scriptData;

		public SBDisassembler()
		{
		}


		public void DisassembleSBScript(byte[] data) {
			scriptData = new ScriptData(data);
		}
	}
}

