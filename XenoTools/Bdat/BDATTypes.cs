using System;
namespace XenoTools.Bdat
{
	public enum BDATValueType {
		None,
		UInt8,
		UInt16,
		UInt32,
		Int8,
		Int16,
		Int32,
		String,
		Float
	}

	public enum BDATMemberType {
		None,
		Regular,
		Array,
		Flags
	}
}

