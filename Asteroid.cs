using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class Asteroid : GameEntity, Tractorable
	{
		bool isTractored;
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

		public void TractorReleased() {
			isTractored = false;
		}

		public void Tractored(Ship s){
			tractoringShip = s;
			isTractored = true;
		}

		public override void Update(float elapsedTime)
		{
			if(isTractored) {
				Position = tractoringShip.Position + new Vector2(100, 100);
			}
			base.Update(elapsedTime);
		}
	}
}
