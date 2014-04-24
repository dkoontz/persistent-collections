using NUnit.Framework;
using System;
using PersistentCollections;

namespace Tests {
	[TestFixture]
	public class UtilTests {
		[Test]
		public void GetBitsForLayer_returns_5_bits_at_LSB_position_based_on_the_uint_and_group_specified() {
			// Number      0xEEB17EA5
			// Group        6     5     4     3     2     1     0
			// Bit pattern 11 10111 01011 00010 11111 10101 00101
			// Layer value  3    23    11     2    31    21     5
			uint value = 0xEEB17EA5;
			Util.GetBitsForGroup(value, 0).ShouldEqual(5);
			Util.GetBitsForGroup(value, 1).ShouldEqual(21);
			Util.GetBitsForGroup(value, 2).ShouldEqual(31);
			Util.GetBitsForGroup(value, 3).ShouldEqual(2);
			Util.GetBitsForGroup(value, 4).ShouldEqual(11);
			Util.GetBitsForGroup(value, 5).ShouldEqual(23);
			Util.GetBitsForGroup(value, 6).ShouldEqual(3);
		}

		[Test]
		public void GetBitsForLayer_returns_5_bits_at_LSB_position_based_on_the_int_and_group_specified() {
			// Number      0x6EB17EA5
			// Group        6     5     4     3     2     1     0
			// Bit pattern 01 10111 01011 00010 11111 10101 00101
			// Layer value  1    23    11     2    31    21     5
			int value = 0x6EB17EA5;
			Util.GetBitsForGroup(value, 0).ShouldEqual(5);
			Util.GetBitsForGroup(value, 1).ShouldEqual(21);
			Util.GetBitsForGroup(value, 2).ShouldEqual(31);
			Util.GetBitsForGroup(value, 3).ShouldEqual(2);
			Util.GetBitsForGroup(value, 4).ShouldEqual(11);
			Util.GetBitsForGroup(value, 5).ShouldEqual(23);
			Util.GetBitsForGroup(value, 6).ShouldEqual(1);
		}
	}
}