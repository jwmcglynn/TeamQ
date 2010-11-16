using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Controllers;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class BlackHole : GameEntity
	{
		SpawnPoint wormHole;

		bool didTeleport = false;
		private List<Entity> waitingToTeleport = new List<Entity>();
		
		public BlackHole(GameEnvironment e, Vector2 pos)
				: base(e)
		{
			SpawnPoint = new SpawnPoint(e.SpawnController, "blackhole", pos);
			Environment.SpawnedBlackHoles.Add(SpawnPoint);
			Environment.PlayerCreatedBlackHoles.Add(SpawnPoint);
			SpawnPoint.Entity = this;
			SpawnPoint.Name = "__player_created__";

			Position = pos;
			Initialize();

			// Create wormhole.
			Random rand = new Random();
			wormHole = Environment.PossibleBlackHoleLocations[rand.Next(0, Environment.PossibleBlackHoleLocations.Count)];
			wormHole.Name = "__player_created__";
			Environment.SpawnedBlackHoles.Add(wormHole);
			Environment.PlayerCreatedBlackHoles.Add(wormHole);
			Environment.SpawnController.SpawnPoints.Add(wormHole);
		}

		public BlackHole(GameEnvironment e, SpawnPoint sp)
				: base(e, sp) {
			Position = sp.Position;
			Initialize();

			// Find where wormhole points.
			wormHole = Environment.SpawnedBlackHoles.Find(spawn => spawn.Name == SpawnPoint.Name && spawn != SpawnPoint);
		}

		private void Initialize() {
			LoadTexture(Environment.contentManager, "black_hole_small_old");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			Zindex = 0.0f;

			CreateCollisionBody(Environment.CollisionWorld, FarseerPhysics.Dynamics.BodyType.Static, CollisionFlags.Default);
			var circle = AddCollisionCircle(Texture.Height / 12, Vector2.Zero); // Using 12 here as an arbitrary value. Reason: Want the black hole to have a small collis
			circle.IsSensor = true;

			Environment.BlackHoleController.AddBody(CollisionBody);
		}

		public static void RemovePlayerCreatedBlackHoles(GameEnvironment env) {
			env.PlayerCreatedBlackHoles.RemoveAll(sp => {
				if (sp.Entity != null) sp.Entity.Dispose();

				env.SpawnedBlackHoles.Remove(sp);
				env.SpawnController.SpawnPoints.Remove(sp);
				return true;
			});
		}

		public override void Dispose()
		{
			Environment.BlackHoleController.RemoveBody(CollisionBody);
			base.Dispose();
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
				Vector2 dir = Vector2.Normalize(Position - teleportingEntity.Position);
				teleportingEntity.Position = wormHole.Position;

				teleportingEntity.TimeSinceTeleport = 0.0f;
				teleportingEntity.TeleportInertiaDir = dir;
				return true;
			});
			
			base.Update(elapsedTime);
		}
	}
}
