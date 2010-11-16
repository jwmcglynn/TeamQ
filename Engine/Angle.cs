using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik {
	public class Angle {
		public static float Normalize(float rot) {
			while (rot > Math.PI) rot -= 2 * (float) Math.PI;
			while (rot < -Math.PI) rot += 2 * (float) Math.PI;

			return rot;
		}

		public static float Distance(float rot1, float rot2) {
			return Normalize(Normalize(rot1) - Normalize(rot2));
		}

		public static float DistanceMag(float rot1, float rot2) {
			return Math.Abs(Distance(rot1, rot2));
		}

		public static float Direction(Vector2 from, Vector2 to) {
			return Normalize((float) Math.Atan2(to.Y - from.Y, to.X - from.X));
		}

		public static Vector2 Vector(float rot) {
			return new Vector2((float) Math.Cos(rot), (float) Math.Sin(rot));
		}
	}
}
