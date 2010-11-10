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

			CenterOffset = new Vector2(view.Width / 2, view.Height / 2);
			MoveSpeed = 1.5f;
		}

		#region Properties

		public Vector2 Position;
		public Vector2 CenterOffset { get; private set; }
		public Matrix Transform { get; private set; }
		public Entity Focus;
		public float MoveSpeed;

		public Rectangle Rect {
			get {
				return new Rectangle(
					(int) (Position.X - CenterOffset.X), (int) (Position.Y - CenterOffset.Y)
					, (int) (2 * CenterOffset.X), (int) (2 * CenterOffset.Y)
				);
			}
		}

		#endregion

		public void Update(float elapsedTime) {
			// Create the Transform used by any
			// spritebatch process
			Transform = Matrix.CreateTranslation(-Position.X + CenterOffset.X, -Position.Y + CenterOffset.Y, 0);

			// Move the Camera to the position that it needs to go.
			if (Focus != null) Position += (Focus.Position - Position) * MoveSpeed * elapsedTime;
		}

		/// <summary>
		/// Determines whether the Rect is in view.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="target">The Rectangle to test against.</param>
		/// <returns>
		/// true if the rect is in view.
		/// </returns>
		public bool IsInView(Rectangle target) {
			return Rect.Intersects(target);
		}

		/// <summary>
		/// Determines whether the target is in view given the specified position.
		/// </summary>
		/// <param name="pos">The position.</param>
		/// <returns>
		///  true if the point is in view.
		/// </returns>
		public bool IsInView(Vector2 pos) {
			Rectangle cameraRect = Rect;

			return (pos.X >= cameraRect.Left && pos.X <= cameraRect.Right
					&& pos.Y >= cameraRect.Top && pos.Y <= cameraRect.Bottom);
		}

		public Vector2 ScreenToWorld(Vector2 screenPos) {
			return screenPos + Position - CenterOffset;
		}

		public void TeleportAndFocus(Entity ent) {
			Focus = ent;
			Position = ent.Position;
		}
	}
}
