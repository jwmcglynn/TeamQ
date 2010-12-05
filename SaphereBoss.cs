﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class SaphereBoss : Boss
	{
		private BlackHole.Pair m_blackHolePair;

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
		}

		public override void Dispose() {
			if (m_blackHolePair != null) m_blackHolePair.Destroy();
			base.Dispose();
		}

		public override void Update(float elapsedTime)
		{
			base.Update(elapsedTime);
		}

		protected override void ShootSpecial(Vector2 position)
		{
			useSpecial = false;

			position.X += (RandomUtil.Next() % 200) - 100;
			position.Y += (RandomUtil.Next() % 200) - 100;

			if (VisionHelper.ClosestEntity(this.CollisionWorld, this.Position, position) != null)
				return;

			if (m_blackHolePair != null) m_blackHolePair.Destroy();
			m_blackHolePair = BlackHole.CreatePair(this.env, position);

			base.ShootSpecial(position);
		}
	}
}
