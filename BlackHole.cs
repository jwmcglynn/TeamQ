using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

			public void NotJustCreated() {
				First.Properties.Remove("justCreated");
				Second.Properties.Remove("justCreated");
			}

			public void Destroy() {
				NotJustCreated();

				if (First.Entity != null) ((BlackHole) First.Entity).DissipateAnimation();
				First.HasBeenOffscreen = true;
				First.Properties.Remove("active");

				Environment.SpawnedBlackHoles.Remove(First);
				Environment.SpawnController.SpawnPoints.Remove(First);

				///

				if (Second.Entity != null) ((BlackHole) Second.Entity).DissipateAnimation();
				Second.HasBeenOffscreen = true;
				Second.Properties.Remove("active");

				Environment.SpawnedBlackHoles.Remove(Second);
				Environment.SpawnController.SpawnPoints.Remove(Second);
			}
		}

		private Texture2D[] m_textures = new Texture2D[20];

		private float timeElapsed;

		private static int s_uniqueId = 1;

		public static Pair CreatePair(GameEnvironment env, Vector2 pos) {
			SpawnPoint sp = new SpawnPoint(env.SpawnController, "blackhole", pos);
			sp.Properties.Add("justCreated", "true");
			env.SpawnedBlackHoles.Add(sp);
			sp.Name = "__blackhole_" + s_uniqueId;
			++s_uniqueId;

			// Create wormhole.
			Random rand = new Random();
			List<SpawnPoint> locs = env.PossibleBlackHoleLocations.FindAll(x => !x.Properties.ContainsKey("active"));

			SpawnPoint wormHole = locs[rand.Next(0, locs.Count)];
			wormHole.Properties.Add("active", "true");
			wormHole.Properties.Add("justCreated", "true");
			wormHole.Name = sp.Name;
			env.SpawnedBlackHoles.Add(wormHole);
			env.SpawnController.SpawnPoints.Add(wormHole);

			sp.Spawn();
			return new Pair(env, sp, wormHole);
		}

		SpawnPoint wormHole;
		private bool animate;
		private bool beginDestruction;

		public BlackHole(GameEnvironment e, SpawnPoint sp)
				: base(e, sp) {
			Position = sp.Position;
			initialize();

			// Find where wormhole points.
			wormHole = Environment.SpawnedBlackHoles.Find(spawn => spawn.Name == SpawnPoint.Name && spawn != SpawnPoint);
		}

		private void initialize() {
			if (SpawnPoint.Properties.ContainsKey("justCreated")) {
				// Do animation.
				SpawnPoint.Properties.Remove("justCreated");
				animate = true;
			}

			// Load textures.
			for (int i = 0; i < m_textures.Length; i++)
			{
				String assetName = String.Format("blackhole/black_hole{0:00}", i);
				m_textures[i] = Environment.contentManager.Load<Texture2D>(assetName);
			}

			if(!animate) Texture = m_textures[m_textures.Length-1]; 
			else Texture = m_textures[0];

			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			Zindex = 0.0f;
			VisualRotationOnly = true;

			Random rand = new Random();
			Rotation = (float) (rand.NextDouble() * 2 * Math.PI);

			CreateCollisionBody(Environment.CollisionWorld, FarseerPhysics.Dynamics.BodyType.Static, CollisionFlags.Default);
			var circle = AddCollisionCircle(Texture.Height / 12, Vector2.Zero); // Using 12 here as an arbitrary value. Reason: Want the black hole to have a small collis
			circle.IsSensor = true;
			CollisionBody.Active = false;

			Environment.BlackHoleController.AddBody(CollisionBody);
		}

		public void DissipateAnimation() {
			SpawnPoint.Reset();
			SpawnPoint = null;

			Environment.BlackHoleController.RemoveBody(CollisionBody);
			DestroyCollisionBody();
			timeElapsed = 0;
			beginDestruction = true;
		}

		public override void Dispose() {
			if (CollisionBody != null) Environment.BlackHoleController.RemoveBody(CollisionBody);
			base.Dispose();
		}

		public override void Update(float elapsedTime)
		{
			Rotation -= .005f;
			if(!animate || timeElapsed > 2.0) {
				Texture = m_textures[m_textures.Length-1];
				if (CollisionBody != null) CollisionBody.Active = true;

				if (animate) {
					wormHole.Properties.Remove("justCreated");
					animate = false;
				}
			} else {
				if(animate) Texture = m_textures[(int)(timeElapsed/2 * 20)];
			}

			if(beginDestruction) {
				if(timeElapsed > 2.0) {
					Dispose();
				} else {
					Texture = m_textures[m_textures.Length - 1 - (int)(timeElapsed * 20 / 2)];
				}
			}

			timeElapsed += elapsedTime;
			base.Update(elapsedTime);
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
