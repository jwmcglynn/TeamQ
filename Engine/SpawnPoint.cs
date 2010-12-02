using System;
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

		// Size.
		public Vector2 Size;

		public Vector2 TopLeft {
			get {
				return Position - Size / 2;
			}
		}

		public Vector2 BottomLeft
		{
			get
			{
				return Position + new Vector2(-Size.X,Size.Y)/2;
			}
		}

		public Vector2 TopRight
		{
			get
			{
				return Position + new Vector2(Size.X, -Size.Y)/2;
			}
		}

		public Vector2 BottomRight {
			get {
				return Position + Size / 2;
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

		#endregion

		public SpawnPoint(SpawnController spawner, string type, Vector2 position) {
			SpawnController = spawner;
			EntityType = type;
			Position = position;
		}

		internal SpawnPoint(SpawnController spawner, Squared.Tiled.Object obj) {
			SpawnController = spawner;
			Size = new Vector2(obj.Width, obj.Height) * GameEnvironment.k_levelScale;
			Position = new Vector2(obj.X, obj.Y) * GameEnvironment.k_levelScale + Size / 2;

			Name = obj.Name;
			Properties = obj.Properties;
			EntityType = obj.Type;

			// Immediately spawn some entities.
			switch (EntityType) {
				case "spawn":
				case "boss":
					Spawn();
					break;
			}
		}

		internal void Update(float elapsedTime, Rectangle spawnRect) {
			m_currentCooldown += elapsedTime;

			if (!HasBeenOffscreen) {
				HasBeenOffscreen = !spawnRect.Intersects(Rect);
			} else if (m_currentCooldown > RespawnCooldown && spawnRect.Intersects(Rect)) {
				Spawn();
			}
		}

		internal Entity Spawn() {
			SpawnController.SpawnPoints.Remove(this);

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
					Entity = new Asteroid(SpawnController.Environment, this);
					break;
				default:
					throw new InvalidOperationException("Invalid entity type.");
			}

			SpawnController.Environment.AddChild(Entity);
			return Entity;
		}

		public void Reset() {
			HasBeenOffscreen = false;

			if (AllowRespawn) {
				m_currentCooldown = 0.0f;
				SpawnController.SpawnPoints.Add(this);
				System.Console.WriteLine("Reset sp " + Entity);
			} else {
				System.Console.WriteLine("Destroy sp " + Entity);
			}

			Entity = null;
		}
	}
}
