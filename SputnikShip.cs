﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class SputnikShip : Ship
	{
		public bool attached = false;
		public bool shouldAttach = false;
		private Ship controlled = null;

		public SputnikShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;

			shooter = new BulletEmitter(env, BulletEmitter.BulletStrength.Weak, IsFriendly());
			AddChild(shooter);

			LoadTexture(env.contentManager, "Sputnik_Old");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;

			CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f; // This value causes a small amount of slowing before stop which looks nice.

			ai = new PlayerController(env);

			// Adjust camera.
			env.Camera.TeleportAndFocus(this);
		}

		public override void Update(float elapsedTime)
		{
			if (controlled == null || controlled.health == 0)
			{
				attached = false;
				base.Update(elapsedTime);
			}
			else
			{
				base.Update(elapsedTime);
			}
		}

		public override void Dispose() {
			Environment.Camera.Focus = null;
			base.Dispose();
		}

		public override bool ShouldCull() {
			return false; // No, not Sputnik!  Don't cull him!
		}

		public override void Shoot(float elapsedTime)
		{
			// Do Nothing
		}

		public ShipController GetAI()
		{
			return this.ai;
		}

		public override void Detatch()
		{
			attached = false;
			controlled = null;
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

        public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
			if (entB is Ship && !attached && shouldAttach)
			{
				this.attached = true;
				((Ship)entB).Attach(this);
				controlled = (Ship)entB;
			}
			base.OnCollide(entB, contact);
        }
	}
}
