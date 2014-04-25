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

			var list = new PersistentList<int>(Enumerable.Range(0, 1057).ToArray());

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

		public static long IntPow(long a, long b)
		{
			long result = 1;
			for (long i = 0; i < b; i++)
				result *= a;
			return result;
		}

		public static long IntPow2(int x, uint pow)
		{
			long ret = 1;
			while ( pow != 0 )
			{
				if ( (pow & 1) == 1 )
					ret *= x;
				x *= x;
				pow >>= 1;
			}
			return ret;
		}

		public static long IntPow3(int x, short power)
		{
			if (power == 0) return 1;
			if (power == 1) return x;
			// ----------------------
			int n = 15;
			while ((power <<= 1) >= 0) n--;

			long tmp = x;
			while (--n > 0)
				tmp = tmp * tmp * 
					(((power <<= 1) < 0)? x : 1);
			return tmp;
		}
	}
}