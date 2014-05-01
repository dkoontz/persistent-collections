using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PersistentCollections {
	public class Program {
		public static void Main(string[] args) {
//			var list = PersistentList<int>.EMPTY;

//			for (var i = 0; i < 32 * 32 * 32; ++i) {
//				list = list.With(i);
//				Console.WriteLine("i: " + i + " = "+ list[i]);
//			}

//			Console.WriteLine("count: " + list.Count);

//			var list = new PersistentList<int>(Enumerable.Range(0, 1057).ToArray());
			var list = new PersistentList<int>(Enumerable.Range(0, 32).ToArray());
			list = list.Without(11);
			Console.WriteLine(list.Count);
//			var baseNum = 32;
//			var maxPower = 6;
//			var sw = new Stopwatch();
//			int pow;
//
//			sw.Reset();
//			sw.Start();
//			for (var i = 0; i < 1000000; ++i) {
//				for (var p = 0; p <= maxPower; ++p) {
//					pow = (int)IntPow(i % baseNum, p);
//				}
//			}
//			sw.Stop();
//			Console.WriteLine(sw.ElapsedMilliseconds);
//
//			sw.Reset();
//			sw.Start();
//			for (var i = 0; i < 1000000; ++i) {
//				for (uint p = 0; p <= maxPower; ++p) {
//					pow = (int)IntPow2(i % baseNum, p);
//				}
//			}
//			sw.Stop();
//			Console.WriteLine(sw.ElapsedMilliseconds);
//
//			sw.Reset();
//			sw.Start();
//			for (var i = 0; i < 10000000; ++i) {
//				for (var p = 0; p <= maxPower; ++p) {
//					pow = (int)Convert.ToInt64(Math.Pow(i % baseNum, p));
//				}
//			}
//			sw.Stop();
//			Console.WriteLine(sw.ElapsedTicks);
//
//			sw.Reset();
//			sw.Start();
//			for (var i = 0; i < 10000000; ++i) {
//				for (short p = 0; p <= maxPower; ++p) {
//					pow = (int)IntPow3(i % baseNum, p);
//				}
//			}
//			sw.Stop();
//			Console.WriteLine(sw.ElapsedTicks);
//
//
//			Console.WriteLine(IntPow(baseNum, maxPower));
//			Console.WriteLine(IntPow2(baseNum, (uint)maxPower));
//			Console.WriteLine(Convert.ToInt64(Math.Pow(baseNum, maxPower)));
//			Console.WriteLine(IntPow3(baseNum, (short)maxPower));

		}
	}
}