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

		public Vector2 BottomRight {
			get {
				return Position + Size / 2;
			}
		}

		// Name.
		public string Name;

		// Type, used for spawning with reflection.
		public string EntityType;

		// Should the entity respawn again after being culled?
		public bool AllowRespawn = true;

		// Time in seconds until SpawnPoint can trigger again.
		public float RespawnCooldown = 0.0f;

		// Arbitrary data added by level editor to initialize object.
		public SortedList<string, string> Properties;

		#endregion

		internal SpawnPoint(SpawnController spawner, Squared.Tiled.Object obj) {
			SpawnController = spawner;
			Size = new Vector2(obj.Width, obj.Height);
			Position = new Vector2(obj.X, obj.Y) + Size / 2;

			Name = obj.Name;
			Properties = obj.Properties;
			EntityType = obj.Type;
		}

		internal Entity Spawn() {
			Entity ent;

			switch (EntityType) {
				case "spawn":
					ent = new SputnikShip(SpawnController.Environment, this);
					break;
				case "triangulus":
					ent = new TriangulusShip(SpawnController.Environment, this);
					break;
				case "squaretopia":
					ent = new SquaretopiaShip(SpawnController.Environment, this);
					break;
				case "circloid":
					ent = new CircloidShip(SpawnController.Environment, this);
					break;
				case "blackhole":
					ent = new BlackHole(SpawnController.Environment, this);
					break;
				default:
					throw new InvalidOperationException("Invalid entity type.");
			}

			SpawnController.Environment.AddChild(ent);
			return ent;
		}
	}
}
