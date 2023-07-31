using System;

namespace XenoTools.Utils
{
	public class PackFileHashUtil
	{
        
		public static ulong CalculatePackFileHash(string filename, byte[] hashValTable) {
            uint hashLowerHalf = 0;
            uint hashUpperHalf = 0;
	        int length = filename.Length;

            //Return if the string is empty
            if (length == 0) {
                Console.WriteLine("Filename is empty");
                return 0;
            }

    
            for(int i = 0; i < hashValTable.Length; i++){
                int byteIndex = hashValTable[i] / 8;
		        int bitIndex = hashValTable[i] % 8;
		        byte mask = (byte)(1 << bitIndex);

                if (byteIndex <= length - 1) {
                    if (((byte)filename[length - 1 - byteIndex] & mask) > 0) {
						//If the index is more than 32, write to the high 32 bit variable
						if (i >= 32) {
                            hashUpperHalf |= (uint)(1 << (i - 32));
                        } else {
                            hashLowerHalf |= (uint)(1 << i);
                        }
                    }
                }
            }
    
            return (hashUpperHalf << 32) + hashLowerHalf;
        }
	}
}

