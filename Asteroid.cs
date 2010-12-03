using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class Asteroid : GameEntity, Tractorable
	{
		private bool m_tractored;
		public bool IsTractored { get { return m_tractored; }  set { m_tractored = value; } }

		Ship tractoringShip;

		public Asteroid(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;
			initialize();
		}

		private void initialize() {
			LoadTexture(Environment.contentManager, "astroid_1");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			Zindex = 0.0f;

			CreateCollisionBody(Environment.CollisionWorld, FarseerPhysics.Dynamics.BodyType.Static, CollisionFlags.Default);
			AddCollisionCircle(Texture.Height/3, Vector2.Zero);
		}

		public override bool ShouldCull() {
			if (IsTractored) return false;
			return base.ShouldCull();
		}

		public void TractorReleased() {
			IsTractored = false;
			if (CollisionBody != null) {
				CollisionBody.IsStatic = true;
			}
		}

		public void Tractored(Ship s){
			tractoringShip = s;
			IsTractored = true;
		}

		public void UpdateTractor(Vector2 position) {
			Position = position;
		}
	}
}
