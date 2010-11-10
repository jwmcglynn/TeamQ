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
		private List<Entity> waitingToTeleport = new List<Entity>();
		
		bool isAWormHole = false;
		bool blackHoleFromMap = false;

		public BlackHole(GameEnvironment e, bool isWormHole)
		{
			env = e;
			isAWormHole = isWormHole;
			initialize();
		}

		public BlackHole(GameEnvironment e, SpawnPoint sp) {
			env = e;
			blackHoleFromMap = true;
			initialize();
			Position = sp.Position;
		}

		private void initialize() {
			LoadTexture(env.contentManager, "black_hole_small_old");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			Zindex = 0.0f;

			CreateCollisionBody(env.CollisionWorld, FarseerPhysics.Dynamics.BodyType.Static, CollisionFlags.DisableSleep);
			var circle = AddCollisionCircle(Texture.Height / 12, Vector2.Zero); // Using 12 here as an arbitrary value. Reason: Want the black hole to have a small collis
			circle.IsSensor = true;

			env.BlackHoleController.AddBody(CollisionBody);

			if (!isAWormHole && !blackHoleFromMap)
			{
				wormHole = new BlackHole(env, true);
				Random rand = new Random();
				wormHole.Position = env.PossibleBlackHoleLocations[rand.Next(0, env.PossibleBlackHoleLocations.Count)];
				wormHole.wormHole = this;
				AddChild(wormHole);
			} else if (!isAWormHole) {
				wormHole = new BlackHole(env, true);
				Random rand = new Random();
				wormHole.Position = new Vector2(500.0f, 500.0f);
				wormHole.wormHole = this;
				AddChild(wormHole);
			}
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
