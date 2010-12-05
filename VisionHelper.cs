using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik {
	class VisionHelper {
		private static List<Entity> QueryAABB(FarseerPhysics.Dynamics.World world, FarseerPhysics.Collision.AABB aabb) {
			List<Entity> all = new List<Entity>();

			world.QueryAABB(proxy => {
				if (proxy.Fixture.Body.UserData is Bullet || proxy.Fixture.Body.UserData is Environment) return true; // Skip bullets/environment.
				if (proxy.Fixture.IsSensor) return true; // Skip sensors.

				all.Add((Entity) proxy.Fixture.Body.UserData);
				return true;
			}, ref aabb);

			return all;
		}

		private static FarseerPhysics.Collision.AABB VisionAABB(Vector2 position, float theta, float spread, float maxDistance) {
			// Triangle extent.
			Vector2 ext1 = (position + Angle.Vector(theta - spread) * maxDistance) * GameEnvironment.k_physicsScale;
			Vector2 ext2 = (position + Angle.Vector(theta + spread) * maxDistance) * GameEnvironment.k_physicsScale;

			Vector2 min = Vector2.Min(ext1, ext2);
			Vector2 max = Vector2.Max(ext1, ext2);

			min = Vector2.Min(min, position * GameEnvironment.k_physicsScale);
			max = Vector2.Max(max, position * GameEnvironment.k_physicsScale);

			return new FarseerPhysics.Collision.AABB(ref min, ref max);
		}

		public static Entity ClosestEntity(FarseerPhysics.Dynamics.World world, Vector2 fromPosition, Vector2 toPosition, out float distance) {
			if (fromPosition == toPosition) {
				distance = 0.0f;
				return null;
			} else {
				Entity closest = null;
				float d = float.PositiveInfinity;

				world.RayCast((FarseerPhysics.Dynamics.Fixture fixture, Vector2 point, Vector2 normal, float fraction) => {
					if (fixture.Body.UserData is Bullet) return -1.0f; // Skip bullets.
					if (fixture.IsSensor) return -1.0f; // Skip sensors.

					closest = (Entity) fixture.Body.UserData;
					d = Vector2.DistanceSquared(fromPosition, point * GameEnvironment.k_invPhysicsScale);
					return fraction;
				}, fromPosition * GameEnvironment.k_physicsScale, toPosition * GameEnvironment.k_physicsScale);

				distance = d;
				return closest;
			}
		}

		public static Entity ClosestEntity(FarseerPhysics.Dynamics.World world, Vector2 fromPosition, Vector2 toPosition) {
			float dist;
			return ClosestEntity(world, fromPosition, toPosition, out dist);
		}

		public static List<Entity> FindAll(GameEnvironment env, Vector2 position, float theta, float spread, float maxDistance) {
			// Find all entities that are within the possible AABB.
			FarseerPhysics.Collision.AABB aabb = VisionAABB(position, theta, spread, maxDistance);
			List<Entity> all = QueryAABB(env.CollisionWorld, aabb);

			float maxDistSq = maxDistance * maxDistance;
			
			// Narrow it down to entities that are directly visible.
			all.RemoveAll(match => {
				// Limit angle.
				float a = Angle.DistanceMag(Angle.Direction(position, match.Position), theta);
				if (a > spread) return true;

				// Verify that nothing is in the way and that the distance fits the threshold.
				float distanceSq;
				Entity closest = ClosestEntity(env.CollisionWorld, position, match.Position, out distanceSq);
				if (closest != match || distanceSq > maxDistSq) return true;

				return false;
			});

			return all;
		}
	}
}
