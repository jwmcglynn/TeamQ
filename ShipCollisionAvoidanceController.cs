using System;
using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik {
	public class ShipCollisionAvoidanceController : FarseerPhysics.Controllers.Controller {
		public List<Body> Bodies = new List<Body>();
		public List<Vector2> Points = new List<Vector2>();

		public ShipCollisionAvoidanceController(float strength) {
			Strength = strength;
			MaxRadius = float.MaxValue;
		}

		public float MaxRadius { get; set; }
		public float Strength { get; set; }

		public override void Update(float dt) {
			Vector2 f = Vector2.Zero;

			foreach (Body body1 in World.BodyList) {
				if (!(body1.UserData is Ship) || (body1.UserData is SputnikShip)) continue;

				foreach (Body body2 in World.BodyList) {
					if ((body1 == body2) || !(body2.UserData is Ship) || (body2.UserData is SputnikShip)) continue;

					Vector2 d = body2.Position - body1.Position;
					float r2 = d.LengthSquared();

					if (r2 < Settings.Epsilon)
						continue;

					float r = d.Length();

					if (r >= MaxRadius)
						continue;

					f = -Strength / r2 * body1.Mass * body2.Mass * d;

					body1.ApplyForce(ref f);
					Vector2.Negate(ref f, out f);
					body2.ApplyForce(ref f);
				}
			}
		}
	}
}