using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class SaphereBoss : Boss
	{
		public SaphereBoss(GameEnvironment env) : base(env) 
		{
			//LoadTexture(env.contentManager, "saphere");
			LoadTexture(env.contentManager, "astroid_1");

			this.bm1 = new BulletEmitter(env, this,BulletEmitter.BulletStrength.Medium, false);
			this.bm2 = new BulletEmitter(env, this,BulletEmitter.BulletStrength.Medium, false);
			this.bm3 = new BulletEmitter(env, this,BulletEmitter.BulletStrength.Medium, false);

			AddChild(bm1);
			AddChild(bm2);
			AddChild(bm3);

			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f;
			CollisionBody.IgnoreGravity = true;

			Vector2[] temp = { new Vector2(env.ScreenVirtualSize.X - 50, env.ScreenVirtualSize.Y - 50), new Vector2(50f, 50f) };

			this.ai = new BossAI(env, this, temp);
		}

		public SaphereBoss(GameEnvironment env, SpawnPoint sp) : base(env, sp)
		{
			LoadTexture(env.contentManager, "saphere");
			//Position = sp.Position;

			this.bm1 = new BulletEmitter(env, this,BulletEmitter.BulletStrength.Medium, false);
			this.bm2 = new BulletEmitter(env, this,BulletEmitter.BulletStrength.Medium, false);
			this.bm3 = new BulletEmitter(env, this,BulletEmitter.BulletStrength.Medium, false);

			AddChild(bm1);
			AddChild(bm2);
			AddChild(bm3);

			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f;
			CollisionBody.IgnoreGravity = true;

			Vector2[] temp = {new Vector2(env.ScreenVirtualSize.X - 50, env.ScreenVirtualSize.Y - 50), new Vector2(50f, 50f)};

			this.ai = new BossAI(env, this, temp);

		}

		public override void Update(float elapsedTime)
		{
			base.Update(elapsedTime);
		}
	}
}
