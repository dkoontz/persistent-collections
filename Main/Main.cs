using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PersistentCollections {
	public class Program {
		public static void Main(string[] args) {
			var list = PersistentList<int>.EMPTY;

			for (var i = 0; i < 32 * 32 * 32 * 32; ++i) {
				list = list.With(i);
			}

			Console.WriteLine("count: " + list.Count);
		}
	}
}