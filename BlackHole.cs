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
		BlackHole wormHole;

		bool didTeleport = false;
		Entity teleportingEntity;
		Vector2 exitDirection;
		private List<Entity> waitingToTeleport = new List<Entity>();

		public BlackHole(GameEnvironment e, bool isWormHole)
		{
			env = e;

			LoadTexture(env.contentManager, "black_hole_small");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			Zindex = 0.0f;
			
			CreateCollisionBody(env.CollisionWorld, FarseerPhysics.Dynamics.BodyType.Static, CollisionFlags.DisableSleep);
			var circle = AddCollisionCircle(Texture.Height / 12, Vector2.Zero); // Using 12 here as an arbitrary value. Reason: Want the black hole to have a small collis
			circle.IsSensor = true; 

			env.BlackHoleController.AddBody(CollisionBody);

			if(!isWormHole) {
					wormHole = new BlackHole(env, true);
					wormHole.Position = new Vector2(150.0f, 150.0f);
					wormHole.wormHole = this;
					AddChild(wormHole);
			}
		}

		public BlackHole(GameEnvironment e, SpawnPoint sp)
				: this(e, false) {
			Position = sp.Position;
		}

		public override void Destroy()
		{
			env.BlackHoleController.RemoveBody(CollisionBody);
			base.Destroy();
		}


		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if(entB is SputnikShip || entB is TriangulusShip || entB is Bullet) {
				if (!didTeleport && entB.TimeSinceTeleport > 0.75f) {
					waitingToTeleport.Add(entB);
					entB.TimeSinceTeleport = 0.0f;
				}
			} else if (entB is TakesDamage) {
				((TakesDamage) entB).InstaKill();
			}

			base.OnCollide(entB, contact);
		}

		public override void Update(float elapsedTime)
		{
			waitingToTeleport.RemoveAll((Entity teleportingEntity) => {
				Vector2 offset = (teleportingEntity.Position - Position);
				teleportingEntity.Position = wormHole.Position;

				teleportingEntity.TimeSinceTeleport = 0.0f;
				teleportingEntity.TeleportInertiaDir = -offset;
				teleportingEntity.TeleportInertiaDir.Normalize();

				return true;
			});
			
			base.Update(elapsedTime);
		}
	}
}
