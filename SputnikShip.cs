using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik
{
    class SputnikShip : Entity
    {

        public ShipController ai = null;

		public SputnikShip(float x, float y, float vx, float vy, GameEnvironment env) : base() 
		{
			LoadTexture(env.contentManager, "Sputnik");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;

			CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f; // This value causes a small amount of slowing before stop which looks nice.

			Position = new Vector2(x, y);
			DesiredVelocity = new Vector2(vx, vy);
			ai = new PlayerController(env);
		}

        new public void Update(float elapsedTime)
        {
            base.Update(elapsedTime);
        }

        public ShipController GetAI()
        {
            return this.ai;
        }
	}
}
