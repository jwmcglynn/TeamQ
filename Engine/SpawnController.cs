using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik {
	class SpawnController {
		internal GameEnvironment Environment;
		internal List<SpawnPoint> SpawnPoints = new List<SpawnPoint>();

		public SpawnController(GameEnvironment env, IList<Squared.Tiled.ObjectGroup> objectGroupList) {
			Environment = env;

			bool spawnedPlayer = false;

			// Load spawn points.
			foreach (Squared.Tiled.ObjectGroup objGroup in objectGroupList) {
				foreach (List<Squared.Tiled.Object> objList in objGroup.Objects.Values) {
					foreach (Squared.Tiled.Object obj in objList) {
						if (obj.Type == "possibleBlackhole") {
							Environment.PossibleBlackHoleLocations.Add(new Vector2(obj.X, obj.Y));
							continue;
						}

						SpawnPoint sp = new SpawnPoint(this, obj);
						SpawnPoints.Add(sp);

						if (!spawnedPlayer && sp.EntityType == "spawn") {
							spawnedPlayer = true;
							sp.Spawn();
						} else {
							// TEMP.
							sp.Spawn(); // TEMP.
						}
						
					}
				}
			}

			if (!spawnedPlayer) throw new InvalidOperationException("Level loaded does not contain player spawn point.");
		}

		public void Update(float elapsedTime) {
			// TODO
		}
	}
}
