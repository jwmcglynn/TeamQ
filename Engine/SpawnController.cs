using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik {
	public class SpawnController {
		internal GameEnvironment Environment;
		public List<SpawnPoint> SpawnPoints = new List<SpawnPoint>();

		public SpawnController(GameEnvironment env, IList<Squared.Tiled.ObjectGroup> objectGroupList) {
			Environment = env;

			bool spawnedPlayer = false;
			
			// Load spawn points.
			foreach (Squared.Tiled.ObjectGroup objGroup in objectGroupList) {
				foreach (List<Squared.Tiled.Object> objList in objGroup.Objects.Values) {
					foreach (Squared.Tiled.Object obj in objList) {
						if (obj.Type == "possibleBlackhole") {
							obj.Type = "blackhole";
							Environment.PossibleBlackHoleLocations.Add(new SpawnPoint(this, obj));
							continue;
						} 
						
						SpawnPoint sp = new SpawnPoint(this, obj);
						if (sp.Entity == null) SpawnPoints.Add(sp);

						if (obj.Type == "BossPatrolPoint")
							Environment.SpawnedBossPatrolPoints.Add(sp);
						else if (obj.Type == "blackhole") {
							Environment.SpawnedBlackHoles.Add(sp);
						}

						if (sp.EntityType == "spawn") spawnedPlayer = true;
					}
				}
			}

			if (!spawnedPlayer) throw new InvalidOperationException("Level loaded does not contain player spawn point.");
		}

		public void Update(float elapsedTime) {
			int halfwidth = (int) (GameEnvironment.k_maxVirtualSize.X / 2 + GameEnvironment.k_spawnRadius);
			int halfheight = (int) (GameEnvironment.k_maxVirtualSize.Y / 2 + GameEnvironment.k_spawnRadius);

			int x = (int) Environment.Camera.Position.X;
			int y = (int) Environment.Camera.Position.Y;

			Rectangle spawnRect = new Rectangle(x - halfwidth, y - halfheight, 2 * halfwidth, 2 * halfheight);

			SpawnPoints.ForEach((SpawnPoint sp) => sp.Update(elapsedTime, spawnRect));
		}
	}
}
