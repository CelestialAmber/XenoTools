using System;
namespace XenoTools.Scripts.SB
{
	public class Section
	{
		public string name;
		public byte[] bytes;

		public Section(string name, byte[] bytes)
		{
			this.name = name;
			this.bytes = bytes;
		}

		public void PrintSection() {
			Console.WriteLine(name + " section:");
		}

		//public string ToString() {

		//}
	}
}

