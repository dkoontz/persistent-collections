using NUnit.Framework;
using System;
using System.Linq;
using PersistentCollections;

namespace Tests {
	[TestFixture]
	public class PersistentListTests {
		[Test]
		public void PersistentList_EMPTY_field_should_be_empty() {
			PersistentList<int>.EMPTY.Count.ShouldEqual(0);
		}

		[Test]
		public void Appending_to_a_PersistentList_should_return_a_new_list() {
			var list1 = PersistentList<int>.EMPTY;
			var list2 = list1.With(5);
			var list3 = list2.With(5);

			list1.ShouldNotEqual(list2);
			list2.ShouldNotEqual(list3);
		}

		[Test]
		public void Appending_to_a_PersistentList_should_increase_the_count_by_1() {
			var list = PersistentList<int>.EMPTY;

			for (var i = 0; i < 10000; ++i) {
				list = list.With(i);
				list.Count.ShouldEqual(i+1);
			}
		}

		[Test]
		public void Appending_to_a_PersistentList_should_not_modify_the_previous_list() {
			var list = PersistentList<int>.EMPTY;
			list = list.With(10);
			list.Count.ShouldEqual(1);
			list[0].ShouldEqual(10);

			var list2 = list.With(20);
			list.Count.ShouldEqual(1);
			list[0].ShouldEqual(10);

			list2.Count.ShouldEqual(2);
			list2[1].ShouldEqual(20);
		}
	}
}