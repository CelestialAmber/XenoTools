using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace XenoTools
{

    class TableCategory
    {
        //Type data (starts at offset 0x20)
        public byte unk1; //usually 1
        public byte valType;
        public short dataIndex;
        //Category data
        public short categoryTypeDataOffset;
        public short unk3;
        public string name;
    }


    class Program
    {
        static void Main(string[] args)
        {
            foreach (string file in Directory.EnumerateFiles("bdat", "*.bdat"))
            {
                Console.WriteLine("Converting " + file);
                string[] lines = DecodeBdat(File.ReadAllBytes(file));
                string path = file.Replace(".bdat", ".csv");
                File.WriteAllLines(path, lines);
            }
        }

        static string[] DecodeBdat(byte[] data)
        {
            int offset = 0;
            string magic = System.Text.Encoding.Default.GetString(data.Take(4).ToArray());
            offset += 4;

            if(magic != "BDAT")
            {
                Console.WriteLine("Error: Invalid BDAT file.");
                return null;
            }

            int tableCategoryDataOffset = ReadInt(offset,data); //0x4
            offset += 4;
            short entrySize = ReadShort(offset, data); //0x8
            offset += 2;
            short entriesDataOffset = ReadShort(offset, data);
            offset += 2;
            short entries = ReadShort(offset, data);
            offset += 2;
            short unk1 = ReadShort(offset, data);
            offset += 2;
            short unk2 = ReadShort(offset, data);
            offset += 2;
            short unk3 = ReadShort(offset, data);
            offset += 2;

            List<TableCategory> categoryList = new List<TableCategory>();

            //The rest of the header seems unused (bytes 0x14-0x1F)

            int categories = (tableCategoryDataOffset - 0x20) / 4;

            offset = tableCategoryDataOffset;

            string tableName = ReadString(offset, data);
            offset += tableName.Length + 1;
            

            for(int i = 0; i < categories; i++)
            {
                TableCategory category = new TableCategory();
                //Read the category type data (starts at 0x20)
                category.unk1 = ReadByte(0x20 + i*4, data);
                category.valType = ReadByte(0x20 + i*4 + 1, data);
                category.dataIndex = ReadShort(0x20 + i * 4 + 2, data);
             
                category.categoryTypeDataOffset = ReadShort(offset, data);
                offset += 2;
                category.unk3 = ReadShort(offset, data);
                offset += 2;
                //Get the category name
                category.name = ReadString(offset, data);
                offset += category.name.Length + 1;
                if (offset % 2 == 1) offset++; //Category name strings are aligned to 2 bytes

                categoryList.Add(category);
            }

            List<byte[]> entriesData = new List<byte[]>();

            offset = entriesDataOffset;

            //Get all the separate entries' data and put them into a list
            for(int i = 0; i < entries; i++)
            {
                entriesData.Add(data.Skip(offset).Take(entrySize).ToArray());
                //Go to the next entry
                offset += entrySize;
            }

            //Convert the bdat file to csv

            List<string> lines = new List<string>();
            string categoryLine = "";

            for(int i = 0; i < categoryList.Count; i++)
            {
                categoryLine += categoryList[i].name + (i < categoryList.Count - 1 ? ", " : "");
            }

            //Write the category line
            lines.Add(categoryLine);

            //Convert each entry
            for(int i = 0; i < entries; i++)
            {
                string line = "";

                //Convert the values for each category/column to strings based on their type, and add them to the line
                for(int j = 0; j < categories; j++)
                {
                    byte[] entryData = entriesData[i];
                    TableCategory category = categoryList[j];
                    //Convert the current value to a string, and add it to the line
                    string s = "";

                    switch (category.valType) {
                        case 0:
                            throw new NotImplementedException();
                        //break;
                        case 1: //byte
                            s = ReadByte(category.dataIndex, entryData).ToString();
                            break;
                        case 2: //ushort
                            s = ReadUInt16(category.dataIndex, entryData).ToString();
                            break;
                        case 3: //uint
                            s = ReadUInt32(category.dataIndex, entryData).ToString();
                            break;
                        case 4: //sbyte
                            s = ReadByte(category.dataIndex, entryData).ToString();
                            break;
                        case 5: //short
                            s = ReadShort(category.dataIndex, entryData).ToString();
                            break;
                        case 6: //int
                            s = ReadInt(category.dataIndex, entryData).ToString();
                            break;
                        case 7: //String pointer (16 bit)
                            ushort stringOffset = ReadUInt16(category.dataIndex, entryData);
                            //In the Japanese version, strings are in Shift-JIS
                            //TODO: make this decode the string with the correct encoding based on region
                            s = ReadShiftJISString(stringOffset, data);
                            break;
                        default:
                            throw new NotImplementedException("Unknown data type id " + category.valType);
                    }

                    line += s;
                    if (j < categories - 1) line += ", ";
                }

                //Write the current line
                lines.Add(line);
            }

            return lines.ToArray();
        }


        static uint ReadUInt32(int offset, byte[] data)
        {
            uint val = BitConverter.ToUInt32(data.Skip(offset).Take(4).Reverse().ToArray());
            return val;
        }

        static int ReadInt(int offset, byte[] data) {
            int val = BitConverter.ToInt32(data.Skip(offset).Take(4).Reverse().ToArray());
            return val;
        }

        static ushort ReadUInt16(int offset, byte[] data) {
            ushort val = BitConverter.ToUInt16(data.Skip(offset).Take(2).Reverse().ToArray());
            return val;
        }

        static short ReadShort(int offset, byte[] data) {
            short val = BitConverter.ToInt16(data.Skip(offset).Take(2).Reverse().ToArray());
            return val;
        }

        static byte ReadByte(int offset, byte[] data) {
            byte val = data[offset];
            return val;
        }

        static float ReadFloat(int offset, byte[] data) {
            float val = BitConverter.ToSingle(data.Skip(offset).Take(4).Reverse().ToArray());
            return val;
        }

        //Reads a zero terminated string at the current offset.
        static string ReadString(int offset, byte[] data) {
            string str = "";
            //Keep going until we reach the terminator byte
            while (data[offset] != 0)
            {
                str += (char)data[offset++];
            }
            offset++; //Increment past the terminator byte;
            return str;
        }

        static string ReadShiftJISString(int offset, byte[] data)
        {
            return ShiftJISDecoder.DecodeShiftJIS(data, offset, true);
        }
    }
}

