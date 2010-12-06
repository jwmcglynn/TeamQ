using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sputnik {
	public class RandomUtil {
		private static Random s_rand = new Random();

		/// <summary>
		/// Returns a random float between 0.0f and 1.0f.
		/// </summary>
		/// <returns></returns>
		public static float NextFloat() {
			return (float) s_rand.NextDouble();
		}

		/// <summary>
		/// Returns a random float between min and max.
		/// </summary>
		/// <returns></returns>
		public static float NextFloat(float min, float max) {
			return (float) (s_rand.NextDouble() * (max - min) + min);
		}

		/// <summary>
		/// Returns a random double between 0.0 and 1.0.
		/// </summary>
		/// <returns></returns>
		public static double NextDouble() {
			return s_rand.NextDouble();
		}

		/// <summary>
		/// Returns a random double between min and max.
		/// </summary>
		/// <returns></returns>
		public static double NextDouble(double min, double max) {
			return s_rand.NextDouble() * (max - min) + min;
		}

		/// <summary>
		/// Returns a non-negative random number.
		/// </summary>
		/// <returns></returns>
		public static int Next() {
			return s_rand.Next();
		}

		/// <summary>
		/// Returns a non-negative random number less than the specified maximum.
		/// </summary>
		/// <param name="maxValue"></param>
		/// <returns></returns>
		public static int Next(int maxValue) {
			return s_rand.Next(maxValue);
		}

		/// <summary>
		/// Return a random number within the specified range.
		/// </summary>
		/// <returns></returns>
		public static int Next(int minValue, int maxValue) {
			return s_rand.Next(minValue, maxValue);
		}
	}
}
