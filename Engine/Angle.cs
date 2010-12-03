using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik {
	public class Angle {
		/// <summary>
		/// Normalize the rotation of an angle to [-pi, pi], allowing easier comparison.
		/// </summary>
		/// <param name="rot">Angle in radians.</param>
		/// <returns>Normalized angle within range.</returns>
		public static float Normalize(float rot) {
			while (rot > Math.PI) rot -= 2 * (float) Math.PI;
			while (rot < -Math.PI) rot += 2 * (float) Math.PI;

			return rot;
		}

		/// <summary>
		/// Linear interpolate two rotations.
		/// </summary>
		/// <param name="rot1"></param>
		/// <param name="rot2"></param>
		/// <param name="percent">Percentage to interpolate, [0, 1].</param>
		/// <returns></returns>
		public static float Lerp(float rot1, float rot2, float percent) {
			float distance = Distance(rot1, rot2);
			return rot1 - distance * percent;
		}

		/// <summary>
		/// Find the signed angular difference between two angles.
		/// </summary>
		/// <param name="rot1">Start rotation.</param>
		/// <param name="rot2">End rotation.</param>
		/// <returns></returns>
		public static float Distance(float rot1, float rot2) {
			return Normalize(Normalize(rot1) - Normalize(rot2));
		}

		/// <summary>
		/// Find the magnitude of the difference between two angles.
		/// </summary>
		/// <param name="rot1">Start rotation.</param>
		/// <param name="rot2">End rotation.</param>
		/// <returns></returns>
		public static float DistanceMag(float rot1, float rot2) {
			return Math.Abs(Distance(rot1, rot2));
		}

		/// <summary>
		/// Find the angle between two points.
		/// </summary>
		/// <param name="from">Start position.</param>
		/// <param name="to">End position.</param>
		/// <returns></returns>
		public static float Direction(Vector2 from, Vector2 to) {
			return Normalize((float) Math.Atan2(to.Y - from.Y, to.X - from.X));
		}

		/// <summary>
		/// Return the rotated unit vector for a given angle.
		/// </summary>
		/// <param name="rot"></param>
		/// <returns></returns>
		public static Vector2 Vector(float rot) {
			return new Vector2((float) Math.Cos(rot), (float) Math.Sin(rot));
		}
	}
}
