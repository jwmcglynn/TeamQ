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
		}

		public SaphereBoss(GameEnvironment env, SpawnPoint sp) : base(env, sp)
		{
		}

		protected override void initialize(GameEnvironment env)
		{
			LoadTexture(env.contentManager, "saphere");

			base.initialize(env);

			Vector2[] temp = { new Vector2(env.ScreenVirtualSize.X, env.ScreenVirtualSize.Y), new Vector2(0f, env.ScreenVirtualSize.Y), new Vector2(0, 0), new Vector2(env.ScreenVirtualSize.X, 0) };

			this.ai = new BossAI(env, this, temp);
		}

		public override void Update(float elapsedTime)
		{
			base.Update(elapsedTime);
		}

		protected override void ShootSpecial(Vector2 position)
		{
			useSpecial = false;

			BlackHole b = new BlackHole(this.env, position);
			this.env.AddChild(b);

			base.ShootSpecial(position);
		}
	}
}
