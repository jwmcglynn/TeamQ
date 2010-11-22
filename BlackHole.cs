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
		public class Pair {
			private GameEnvironment Environment;
			public SpawnPoint First;
			public SpawnPoint Second;

			internal Pair(GameEnvironment _env, SpawnPoint _first, SpawnPoint _second) {
				Environment = _env;
				First = _first;
				Second = _second;
			}

			public SpawnPoint Other(SpawnPoint current) {
				if (current == First) return Second;
				else return First;
			}

			public void Destroy() {
				if (First.Entity != null) First.Entity.Dispose();
				First.HasBeenOffscreen = true;
				First.Properties.Remove("active");

				Environment.SpawnedBlackHoles.Remove(First);
				Environment.SpawnController.SpawnPoints.Remove(First);

				///

				if (Second.Entity != null) Second.Entity.Dispose();
				Second.HasBeenOffscreen = true;
				Second.Properties.Remove("active");

				Environment.SpawnedBlackHoles.Remove(Second);
				Environment.SpawnController.SpawnPoints.Remove(Second);
			}
		}

		private static int s_uniqueId = 1;

		public static Pair CreatePair(GameEnvironment env, Vector2 pos) {
			SpawnPoint sp = new SpawnPoint(env.SpawnController, "blackhole", pos);
			env.SpawnedBlackHoles.Add(sp);
			sp.Name = "__blackhole_" + s_uniqueId;
			++s_uniqueId;

			// Create wormhole.
			Random rand = new Random();
			List<SpawnPoint> locs = env.PossibleBlackHoleLocations.FindAll(x => !x.Properties.ContainsKey("active"));

			SpawnPoint wormHole = locs[rand.Next(0, locs.Count)];
			wormHole.Properties.Add("active", "true");
			if (wormHole.AllowRespawn)
			wormHole.Name = sp.Name;
			env.SpawnedBlackHoles.Add(wormHole);
			env.SpawnController.SpawnPoints.Add(wormHole);

			sp.Spawn();
			return new Pair(env, sp, wormHole);
		}

		///

		SpawnPoint wormHole;

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

		public override void Dispose()
		{
			Environment.BlackHoleController.RemoveBody(CollisionBody);
			base.Dispose();
		}

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if(entB is SputnikShip || entB is TriangulusShip || entB is Bullet) {
				if (entB.TimeSinceTeleport > 0.75f) {
					OnNextUpdate += () => {
						Vector2 dir = Vector2.Normalize(Position - entB.Position);
						entB.Position = wormHole.Position;

						entB.TimeSinceTeleport = 0.0f;
						entB.TeleportInertiaDir = dir;
					};
				}
			} else if (entB is TakesDamage) {
				((TakesDamage) entB).InstaKill();
			}

			base.OnCollide(entB, contact);
		}
	}
}
