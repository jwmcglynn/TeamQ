using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik {
	class GameEntity : Entity {
		public GameEnvironment Environment;
		public SpawnPoint SpawnPoint;
		
		public GameEntity(GameEnvironment env) {
			Environment = env;
		}

		public GameEntity(GameEnvironment env, SpawnPoint sp) {
			Environment = env;
			SpawnPoint = sp;
		}

		public override void Update(float elapsedTime) {
			if (ShouldCull()) Dispose();

			base.Update(elapsedTime);
		}

		public override void Dispose() {
			if (SpawnPoint != null) {
				SpawnPoint.Position = Position;
				OnCull();
			}

			base.Dispose();
		}

		/// <summary>
		/// Should this Entity cull right now?  Called within Update.
		/// 
		/// If an Entity should not cull override this and return false.
		/// </summary>
		/// <returns>[true] if Entity should cull, [false] if not.</returns>
		public virtual bool ShouldCull() {
			return !InsideCullRect(VisibleRect);
		}

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
		}
	}
}