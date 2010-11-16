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
		public Camera2D(GameEnvironment env) {
			Environment = env;
			MoveSpeed = 1.5f;

			// Set default window size.
			WindowSizeChanged();
		}

		public void WindowSizeChanged() {
			Rectangle rect = Environment.Controller.Window.ClientBounds;
			if (rect.Width == 0 || rect.Height == 0) return; // Do nothing, window was minimized.

			Scale = (float) rect.Height / Environment.ScreenVirtualSize.Y;
			CenterOffset = Environment.ScreenVirtualSize / 2;
		}

		#region Properties

		private GameEnvironment Environment;

		public Vector2 Position;
		public Vector2 CenterOffset { get; private set; }
		public Matrix Transform { get; private set; }
		public Matrix InverseTransform {
			get {
				if (!m_inverseIsValid) {
					m_inverseTransform = Matrix.Invert(Transform);
					m_inverseIsValid = true;
				}
				return m_inverseTransform;
			}
		}
		public Entity Focus;
		public float MoveSpeed;
		public float Scale;

		private bool m_inverseIsValid = false;
		private Matrix m_inverseTransform;

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
			Transform = Matrix.CreateTranslation(-Position.X + CenterOffset.X, -Position.Y + CenterOffset.Y, 0)
							* Matrix.CreateScale(Scale);
			m_inverseIsValid = false;

			// Move the Camera to the position that it needs to go.
			if (Focus != null) Position += (Focus.Position - Position) * MoveSpeed * elapsedTime;

			Sound.CameraPos = Position;
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
			return Vector2.Transform(screenPos, InverseTransform);
		}

		public void TeleportAndFocus(Entity ent) {
			Focus = ent;
			Position = ent.Position;
		}
	}
}
