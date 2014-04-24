﻿using System;

namespace PersistentCollections {
	public static class Util {
		// This was found at
		// http://stackoverflow.com/a/15979139/498817
		// by vidit
		public static int BitCount(int v) {
			v = v - ((v >> 1) & 0x55555555);                // put count of each 2 bits into those 2 bits
			v = (v & 0x33333333) + ((v >> 2) & 0x33333333); // put count of each 4 bits into those 4 bits  
			return ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
		}

		public static int GetBitsForGroup(int bitmask, int group) {
			// Bit shift operators pad with 1's instead of 0's for signed values
			// so we must do the shift operation on uints
			return GetBitsForGroup((uint)bitmask, group);
		}

		public static int GetBitsForGroup(uint bitmask, int group) {
			return (int)(bitmask >> (group * 5)) & 0x1F;
		}

		public static int NextGroup(int bitmask) {
			// Bit shift operators pad with 1's instead of 0's for signed values
			// so we must do the shift operation on uints
			return NextGroup((uint)bitmask);
		}

		public static int NextGroup(uint bitmask) {
			return (int)(bitmask >> 5);
		}

		public static TValue[] ExtendArrayAndAppend<TValue>(TValue[] originalArray, TValue value) {
			var newArray = new TValue[originalArray.Length + 1];
			originalArray.CopyTo(newArray, 0);
			newArray[newArray.Length - 1] = value;
			return newArray;
		}
	}
}