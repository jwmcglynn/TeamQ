using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik {
	/**
	 * Based on Camera2D class from this stackoverflow post: http://stackoverflow.com/questions/712296/xna-2d-camera-engine-that-follows-sprite
	**/
	public class Camera2D {
		public Camera2D(Environment env) {
			var view = env.Controller.GraphicsDevice.Viewport;

			Origin = new Vector2(view.Width / 2, view.Height / 2);
			MoveSpeed = 1.5f;
		}

		#region Properties

		public Vector2 Position;
		public Vector2 Origin;
		public Matrix Transform;
		public Entity Focus;
		public float MoveSpeed;

		#endregion

		public void Update(float elapsedTime) {
			// Create the Transform used by any
			// spritebatch process
			Transform = Matrix.CreateTranslation(-Position.X + Origin.X, -Position.Y + Origin.Y, 0);

			// Move the Camera to the position that it needs to go.
			if (Focus != null) Position += (Focus.Position - Position) * MoveSpeed * elapsedTime;
		}

		/// <summary>
		/// Determines whether the Entity is in view given the specified position.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="texture">The entity.</param>
		/// <returns>
		/// true if the object is in view.
		/// </returns>
		public bool IsInView(Vector2 position, Entity ent) {
			if (ent.Texture == null) return IsInView(position);

			// If the object is not within the horizontal bounds of the screen.
			if ((position.X + ent.Texture.Width) < (Position.X - Origin.X) || (position.X) > (Position.X + Origin.X))
				return false;

			// If the object is not within the vertical bounds of the screen.
			if ((position.Y + ent.Texture.Height) < (Position.Y - Origin.Y) || (position.Y) > (Position.Y + Origin.Y))
				return false;

			// In View.
			return true;
		}

		/// <summary>
		/// Determines whether the target is in view given the specified position.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <returns>
		///  true if the point is in view.
		/// </returns>
		public bool IsInView(Vector2 position) {
			// If the object is not within the horizontal bounds of the screen.
			if ((position.X) < (Position.X - Origin.X) || (position.X) > (Position.X + Origin.X))
				return false;

			// If the object is not within the vertical bounds of the screen.
			if ((position.Y) < (Position.Y - Origin.Y) || (position.Y) > (Position.Y + Origin.Y))
				return false;

			// In View.
			return true;
		}

		public Vector2 ScreenToWorld(Vector2 screenPos) {
			return screenPos - Origin + Position;
		}
	}
}
