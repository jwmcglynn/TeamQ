using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik
{
    class SputnikShip : Ship
    {

        public PlayerController ai = null;

		public SputnikShip(float x, float y, float vx, float vy, GameEnvironment env) : base(x, y, vx, vy, 0.0f, 0.0f, 0.0f, 0.0f, env) 
		{
			this.shooter = new BulletEmitter(env, BulletEmitter.BulletStrength.Weak, IsFriendly());
			env.AddChild(this.shooter);

			LoadTexture(env.contentManager, "Sputnik");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;

			CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f; // This value causes a small amount of slowing before stop which looks nice.

			ai = new PlayerController(env);
		}

        public override void Update(float elapsedTime)
        {
			ai.Update(this, elapsedTime);
            base.Update(elapsedTime);
        }

        public ShipController GetAI()
        {
            return this.ai;
        }
	}
}
