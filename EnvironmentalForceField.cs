using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class EnvironmentalForceField : GameEntity
	{
		public EnvironmentalForceField(GameEnvironment env, SpawnPoint sp) 
			: base(env, sp) {
			Position = sp.Position;
			initialize();
		}
		
		private void initialize() {
			LoadTexture(Environment.contentManager, "freeze/web_static");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			Zindex = 0.0f;

			CreateCollisionBody(Environment.CollisionWorld, FarseerPhysics.Dynamics.BodyType.Static, CollisionFlags.Default);

			if((SpawnPoint.TopLeft.X - SpawnPoint.TopRight.X) >= (SpawnPoint.TopRight.Y - SpawnPoint.BottomRight.Y)) {
				AddCollisionRectangle(new Vector2(Texture.Width/2, Texture.Height/2), Vector2.Zero, Rotation, 1.0f);
			} else {
				Rotation = 90.0f * (float) Math.PI/180;
				AddCollisionRectangle(new Vector2(Texture.Height / 2, Texture.Width / 2), Vector2.Zero, Rotation, 1.0f);
			}
		}

		public override bool ShouldCollide(Entity entB, FarseerPhysics.Dynamics.Fixture fixture, FarseerPhysics.Dynamics.Fixture entBFixture)
		{
			if(entB is CircloidShip) return false;
			return true;
		}
	}
}
