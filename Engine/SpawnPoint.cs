﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace Sputnik {
	public class SpawnPoint {
		#region Properties

		private SpawnController SpawnController;

		// Position.
		public Vector2 Position;
		private Vector2 OriginalPosition;

		// Size.
		public Vector2 Size;

		public Vector2 TopLeft {
			get {
				return Position - Size / 2;
			}
			set
			{
				Position = value + (Size / 2);
			}
		}

		public Vector2 BottomLeft
		{
			get
			{
				return Position + new Vector2(-Size.X,Size.Y)/2;
			}
			set
			{
				Position = value - (new Vector2(-Size.X, Size.Y) / 2);
			}
		}

		public Vector2 TopRight
		{
			get
			{
				return Position + new Vector2(Size.X, -Size.Y)/2;
			}
			set
			{
				Position = value - (new Vector2(Size.X, -Size.Y) / 2);
			}
		}

		public Vector2 BottomRight {
			get {
				return Position + Size / 2;
			}
			set
			{
				Position = value - (Size / 2);
			}
		}

		public Rectangle Rect {
			get {
				return new Rectangle((int) TopLeft.X, (int) TopLeft.Y, (int) Size.X, (int) Size.Y);
			}
		}

		// Name.
		public string Name;

		// Type, used for spawning with reflection.
		public string EntityType;

		// Entity once spawned.
		public Entity Entity;

		// Should the entity respawn again after being culled?
		public bool AllowRespawn = true;

		// Time in seconds until SpawnPoint can trigger again.
		public float RespawnCooldown;

		// Arbitrary data added by level editor to initialize object.
		public SortedList<string, string> Properties = new SortedList<string, string>();


		// Current progress on respawn time.
		private float m_currentCooldown;

		// Has the entity been offscreen since it was culled?
		public bool HasBeenOffscreen = true;

		// Should always be spawned?  Used for player ship so that schlee respawns when dying.
		public bool AlwaysSpawned = false;

		#endregion

		public SpawnPoint(SpawnController spawner, string type, Vector2 position) {
			SpawnController = spawner;
			EntityType = type;
			Position = position;
			OriginalPosition = position;
		}

		internal SpawnPoint(SpawnController spawner, Squared.Tiled.Object obj) {
			SpawnController = spawner;
			Size = new Vector2(obj.Width, obj.Height);
			Position = new Vector2(obj.X, obj.Y) + Size / 2;
			OriginalPosition = new Vector2(obj.X, obj.Y) + Size / 2;

			Name = obj.Name;
			Properties = obj.Properties;
			EntityType = obj.Type;

			// Immediately spawn some entities.
			switch (EntityType) {
				case "spawn":
				case "boss":
					AlwaysSpawned = true;
					Spawn();
					break;
			}
		}

		internal void Update(float elapsedTime, Rectangle spawnRect) {
			m_currentCooldown += elapsedTime;

			if (!HasBeenOffscreen) {
				HasBeenOffscreen = !spawnRect.Intersects(Rect);
			} else if (AlwaysSpawned || (m_currentCooldown >= RespawnCooldown && spawnRect.Intersects(Rect))) {
				Spawn();
			}
		}

		internal Entity Spawn() {
			SpawnController.SpawnPoints.Remove(this);

			// Update SpawnPoint in case it gets triggered again.
			HasBeenOffscreen = false;

			switch (EntityType) {
				case "spawn":
					Entity = new SputnikShip(SpawnController.Environment, this);
					break;
				case "triangulus":
					Entity = new TriangulusShip(SpawnController.Environment, this);
					break;
				case "squaretopia":
					Entity = new SquaretopiaShip(SpawnController.Environment, this);
					break;
				case "circloid":
					Entity = new CircloidShip(SpawnController.Environment, this);
					break;
				case "blackhole":
					Entity = new BlackHole(SpawnController.Environment, this);
					break;
				case "boss":
					Entity = new SaphereBoss(SpawnController.Environment, this);
					break;
				case "asteroid":
				case "asteroid1":
				case "asteroid2":
				case "asteroid3":
					Entity = new Asteroid(SpawnController.Environment, this);
					break;
				case "forcefield":
					Entity = new EnvironmentalForceField(SpawnController.Environment, this);
					break;
				default:
					throw new InvalidOperationException("Invalid entity type.");
			}

			SpawnController.Environment.AddChild(Entity);

			return Entity;
		}

		public void Reset() {
			if (AllowRespawn) {
				Position = OriginalPosition;
				m_currentCooldown = 0.0f;
				SpawnController.SpawnPoints.Add(this);
			}

			Entity = null;
		}
	}
}
