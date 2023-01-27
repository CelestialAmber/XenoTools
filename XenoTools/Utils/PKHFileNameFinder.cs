using System;
using System.Collections.Generic;
using System.Text;

namespace XenoTools.Utils {

	public class PKHFileNameFinder {
		public static byte[] hashValTable;
		public static char[] characters = {
		'a','b','c','d','e','f','g','h','i','j','k','l','m',
		'n','o','p','q','r','s','t','u','v','w','x','y','z',
		'A','B','C','D','E','F','G','H','I','J','K','L','M',
		'N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
		'0','1','2','3','4','5','6','7','8','9','-','_','(',
		')','.'
	};
		public static HashValueInfo[] hashValueInfoList;

		public class HashValueInfo {
			public List<char> zeroBitChars = new List<char>();
			public List<char> oneBitChars = new List<char>();
			public int charIndex;
			public int charBit;

			public HashValueInfo(int charIndex, int charBit) {
				this.charIndex = charIndex;
				this.charBit = charBit;
			}
		}

		public static void Init(byte[] hashValTable) {
			PKHFileNameFinder.hashValTable = hashValTable;
			CreateHashValueInfoList();
		}

		static int GetMaxHashValueTableCharacterIndex() {
			int maxCharIndex = 0;
			foreach(byte hashVal in hashValTable) {
				int charIndex = hashVal / 8;
				if (charIndex > maxCharIndex) maxCharIndex = charIndex;
			}
			return maxCharIndex;
		}

		public static void FindAllPossibleStringsFromHash(ulong hash) {
			int maxHashStringLength = GetMaxHashValueTableCharacterIndex() + 1;

			/* For each character, determine what the character could be based on information
			derived from each hash value */
			for (int i = 0; i < maxHashStringLength; i++) {
				List<char> possibleCharacters = new List<char>();
				foreach (char c in characters) {
					possibleCharacters.Add(c);
				}

				bool charactersRemoved = false;

				//Narrow down possible characters based on the info for each hash value
				for(int j = 0; j < hashValueInfoList.Length; j++) {
					HashValueInfo hashValInfo = hashValueInfoList[j];
					//Check if the current hash value maps to this character
					if (i == hashValInfo.charIndex) {
						//Remove any characters that aren't possible based on this hash value
						int hashBit = (int)((hash >> j) & 1);
						List<char> charactersToRemove = hashBit == 1 ? hashValInfo.zeroBitChars : hashValInfo.oneBitChars;
						foreach (char c in charactersToRemove) {
							possibleCharacters.Remove(c);
						}
						charactersRemoved = true;
					}
				}

				StringBuilder sb = new StringBuilder();
				int charIndex = i;
				sb.Append("Char " + charIndex + ": ");
				if (charactersRemoved) {
					foreach (char c in possibleCharacters) {
						sb.Append(c + " ");
					}
				} else {
					sb.Append("any");
				}
				sb.AppendLine();
				Console.WriteLine(sb.ToString());
			}
		}

		/* Create a list containing info on each hash value in the table, including potential
		characters based on the resulting value (0 or 1), and what character and bit this
		value maps to. */
		public static void CreateHashValueInfoList() {
			hashValueInfoList = new HashValueInfo[hashValTable.Length];

			for (int i = 0; i < hashValTable.Length; i++) {
				int charIndex = hashValTable[i] / 8;
				int bitIndex = hashValTable[i] % 8;

				hashValueInfoList[i] = new HashValueInfo(charIndex, bitIndex);

				int mask = 1 << bitIndex;
				Console.WriteLine("Char index: " + charIndex + ", bit " + bitIndex);
				//Print the result value for each possible character
				for (int j = 0; j < characters.Length; j++) {
					int bit = ((characters[j] & mask) > 0 ? 1 : 0);
					if (bit == 1) hashValueInfoList[i].oneBitChars.Add(characters[j]);
					else hashValueInfoList[i].zeroBitChars.Add(characters[j]);
				}
			}
		}
	}

}