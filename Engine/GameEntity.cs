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
			if (ShouldCull()) Destroy();

			base.Update(elapsedTime);
		}

		public override void Destroy() {
			if (SpawnPoint != null) {
				SpawnPoint.Position = Position;
				OnCull();
			}

			base.Destroy();
		}

		/// <summary>
		/// Should this Entity cull right now?  Called within Update.
		/// 
		/// If an Entity should not cull override this and return false.
		/// </summary>
		/// <returns>[true] if Entity should cull, [false] if not.</returns>
		public virtual bool ShouldCull() {
			Rectangle cullRect = VisibleRect;
			cullRect.Inflate((int) GameEnvironment.k_cullRadius, (int) GameEnvironment.k_cullRadius);

			return !Environment.Camera.IsInView(cullRect);
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
