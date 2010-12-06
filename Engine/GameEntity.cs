using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik {
	public class GameEntity : Entity {
		public GameEnvironment Environment;
		public SpawnPoint SpawnPoint;

		public float TimeSinceTeleport = float.PositiveInfinity;
		public Vector2 TeleportInertiaDir;
		public bool AllowTeleport = false;
		
		public GameEntity(GameEnvironment env) {
			Environment = env;
		}

		public GameEntity(GameEnvironment env, SpawnPoint sp) {
			Environment = env;
			SpawnPoint = sp;
		}

		public override void Update(float elapsedTime) {
			if (ShouldCull()) Dispose();

			// Apply blackhole teleportation force.
			if (CollisionBody != null) {
				if (TimeSinceTeleport < 1.0f) {
					CollisionBody.ApplyForce(TeleportInertiaDir * 50.0f * CollisionBody.Mass);
					CollisionBody.IgnoreGravity = true;
				} else {
					if (AllowTeleport) CollisionBody.IgnoreGravity = false;
				}
			}

			TimeSinceTeleport += elapsedTime;

			base.Update(elapsedTime);
		}

		public override void Dispose() {
			if (SpawnPoint != null) {
				//Updated spawn points makes me sad
				//If we really want it, I'll update it in the AI
				//SpawnPoint.Position = Position;
				OnCull();
			}

			base.Dispose();
		}

		/// <summary>
		/// Teleport entity through a blackhole.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="exitVelocity"></param>
		public virtual void Teleport(BlackHole blackhole, Vector2 destination, Vector2 exitVelocity) {
			if (AllowTeleport) {
				if (TimeSinceTeleport > 0.75f) {
					// Play sound, unless it is a bullet in which case it just gets annoying.
					if (!(this is Bullet)) Sound.PlayCue("thru_black_hole", blackhole);

					Position = destination;
					TimeSinceTeleport = 0.0f;
					TeleportInertiaDir = exitVelocity;
				}
			} else if (this is TakesDamage) {
				((TakesDamage) this).InstaKill();
			}
		}

		/*********************************************************************/
		// Culling.

		/// <summary>
		/// Should this Entity cull right now?  Called within Update.
		/// 
		/// If an Entity should not cull override this and return false.
		/// </summary>
		/// <returns>[true] if Entity should cull, [false] if not.</returns>
		public virtual bool ShouldCull() {
			return !InsideCullRect(VisibleRect);
		}

		/// <summary>
		/// Does the provided rectangle intersect the cull rect?
		/// </summary>
		/// <param name="rect"></param>
		/// <returns></returns>
		protected bool InsideCullRect(Rectangle rect) {
			int halfwidth = (int) (GameEnvironment.k_maxVirtualSize.X / 2 + GameEnvironment.k_cullRadius);
			int halfheight = (int) (GameEnvironment.k_maxVirtualSize.Y / 2 + GameEnvironment.k_cullRadius);

			int x = (int) Environment.Camera.Position.X;
			int y = (int) Environment.Camera.Position.Y;

			Rectangle cullRect = new Rectangle(x - halfwidth, y - halfheight, 2 * halfwidth, 2 * halfheight);

			return cullRect.Intersects(rect);
		}

		/// <summary>
		/// Called immediately before Entity is culled this and update the SpawnPoint before an object is culled.
		/// </summary>
		public virtual void OnCull() {
			// Currently does nothing.  Update SpawnPoint.
			SpawnPoint.Reset();
			SpawnPoint = null;
		}
	}
}