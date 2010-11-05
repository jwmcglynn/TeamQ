using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Controllers;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class BlackHole : SpecialAbility
	{
		GameEnvironment env;

		public BlackHole(GameEnvironment e)
		{
			env = e;

			LoadTexture(env.contentManager, "black_hole_small");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			Zindex = 0.0f;
			
			CreateCollisionBody(env.CollisionWorld, FarseerPhysics.Dynamics.BodyType.Static, CollisionFlags.DisableSleep);
			AddCollisionCircle(Texture.Height/12, Vector2.Zero);
			
			Position = new Vector2(50.0f, 50.0f);

            env.physicsController.AddBody(CollisionBody);
		}

		public override void Update(float elapsedTime)
		{
			base.Update(elapsedTime);
		}
	}
}
