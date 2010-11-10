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

			LoadTexture(env.contentManager, "Sputnik_Old");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;

			CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f; // This value causes a small amount of slowing before stop which looks nice.

			ai = new PlayerController(env);

			// Adjust camera.
			env.Camera.Position = new Vector2(x, y);
			env.Camera.Focus = this;
		}

		public SputnikShip(GameEnvironment env, SpawnPoint sp)
				: this(sp.Position.X, sp.Position.Y, 0.0f, 0.0f, env) {
			Position = sp.Position;
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

		public override bool IsFriendly() {
			return true;
		}

		public override bool ShouldCollide(Entity entB) {
			return !(entB is Environment) && !(entB is Bullet);
		}

		public override void TakeHit(int damage) {
			// Do nothing.
		}
	}
}
