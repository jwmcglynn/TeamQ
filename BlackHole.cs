﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sputnik
{
	public class BlackHole : GameEntity
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

		private Texture2D[] m_textures = new Texture2D[21];
		private float timeElapsed;
		private static int s_uniqueId = 1;
		SpawnPoint wormHole;
		private bool animate;
		private bool beginDestruction;
		private int currentTexture = 12; // So that it will start from blackhole 11
		private float blackholeTimer;
		private bool goBackwards;
		private float timeForAnimation = 2.0f;
		private int numberOfFrames = 20;
		public bool fullyFormed;
		private float timeForFrame;
		private float timeSinceLastFrame;
		private int currentFrame;

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

		public BlackHole(GameEnvironment e, SpawnPoint sp)
				: base(e, sp) {
			Position = sp.Position;
			initialize();
			Environment.blackHoles.Add(this);
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

			timeForFrame = 1/(timeForAnimation * numberOfFrames);

			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			Zindex = 0.4f;
			VisualRotationOnly = true;

			Random rand = new Random();
			Rotation = (float) (rand.NextDouble() * 2 * Math.PI);

			CreateCollisionBody(Environment.CollisionWorld, FarseerPhysics.Dynamics.BodyType.Static, CollisionFlags.Default);
			var circle = AddCollisionCircle(Texture.Height / 12, Vector2.Zero); // Using 12 here as an arbitrary value. Reason: Want the black hole to have a small collis
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
			Environment.blackHoles.Remove(this);
			base.Dispose();
		}

		public override void Update(float elapsedTime)
		{
			Rotation -= .02f;
			if (!animate || timeElapsed > timeForAnimation)
			{
				Texture = m_textures[currentTexture];

				fullyFormed = true;

				if(blackholeTimer > 0.1) {
					if(goBackwards) {
						currentTexture--;
					} else {
						currentTexture++;
					}
					blackholeTimer = 0.0f;
				}
				
				if(currentTexture == 20) {
					goBackwards = true;
				} else if (currentTexture == 12) {
					goBackwards = false;
				}

				if (CollisionBody != null) CollisionBody.Active = true;

				if (animate) {
					wormHole.Properties.Remove("justCreated");
					animate = false;
				}

				Environment.BlackHoleEffect.Trigger(Position);
			} else { // if we are still forming our blackhole
				if(!beginDestruction) {
					if(timeSinceLastFrame > timeForFrame) {
						if(currentFrame < m_textures.Length - 1) {
							currentFrame++;
						}
						timeSinceLastFrame = 0.0f;
					}
					if (animate) Texture = m_textures[currentFrame];
				}
			}

			if(beginDestruction) {
				if (currentFrame == 0) {
					Dispose();
				} else {
					if(timeSinceLastFrame > timeForFrame) {
						if(currentFrame > 0) {
							currentFrame--;
						}
						timeSinceLastFrame = 0.0f;
					}
					Texture = m_textures[currentFrame];
				}
			}

			timeSinceLastFrame += elapsedTime;
			blackholeTimer += elapsedTime;
			timeElapsed += elapsedTime;
			base.Update(elapsedTime);
		}

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			contact.Enabled = false;

			if (entB is GameEntity) {
				OnNextUpdate += () => {
					Vector2 exitVelocity = Vector2.Normalize(Position - entB.Position);
					((GameEntity) entB).Teleport(this, wormHole.Position, exitVelocity);
				};
			}

			base.OnCollide(entB, contact);
		}
	}
}
