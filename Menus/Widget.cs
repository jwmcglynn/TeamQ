using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sputnik.Menus {
	public class Widget : Entity {
		public Menu Menu;

		private bool m_hasButton = false;
		private Rectangle m_buttonRect; // In pixel-space relative to position.

		public bool Visible = true;

		/// <summary>
		/// Current position in "percent" from 0 to 1.  Added to Position to determine draw
		/// position.
		/// </summary>
		public Vector2 PositionPercent = Vector2.Zero;

		/// <summary>
		/// Current absolute position, taking PositionPercent into account.
		/// </summary>
		public Vector2 AbsolutePosition {
			get {
				return Menu.ScreenSize * PositionPercent + Position;
			}
		}

		public Widget(Menu menuEnv) {
			Menu = menuEnv;
		}

		public override void Dispose() {
			DestroyButton();
			base.Dispose();
		}

		#region Buttons

		public void CreateButton(Rectangle rect) {
			if (!m_hasButton) Menu.Buttons.Add(this);
			m_buttonRect = rect;
			m_hasButton = true;
		}

		public void DestroyButton() {
			Menu.Buttons.Remove(this);
			m_hasButton = false;
		}

		public bool Collides(Vector2 pos) {
			pos -= AbsolutePosition;
			return (pos.X >= m_buttonRect.Left && pos.X <= m_buttonRect.Right
					&& pos.Y >= m_buttonRect.Top && pos.Y <= m_buttonRect.Bottom);
		}

		// Notification events.
		public delegate void ButtonEvent();

		public event ButtonEvent OnMouseOver;
		public event ButtonEvent OnMouseOut;
		public event ButtonEvent OnMouseDown;
		public event ButtonEvent OnMouseUp;
		public event ButtonEvent OnActivate;

		internal void DispatchOnMouseOver() { if (OnMouseOver != null) OnMouseOver(); }
		internal void DispatchOnMouseOut() { if (OnMouseOut != null) OnMouseOut(); }
		internal void DispatchOnMouseDown() { if (OnMouseDown != null) OnMouseDown(); }
		internal void DispatchOnMouseUp(bool inside) {
			if (inside && OnActivate != null) OnActivate();
			if (!inside && OnMouseUp != null) OnMouseUp();
		}

		#endregion

		/// <summary>
		/// Draw the entity.
		/// </summary>
		/// <param name="spriteBatch">SpriteBatch to render to.</param>
		public override void Draw(SpriteBatch spriteBatch) {
			if (Visible && Texture != null) {
				spriteBatch.Draw(Texture, AbsolutePosition, null, VertexColor * Alpha, Rotation, Registration, 1.0f, SpriteEffects.None, Zindex);
			}

			foreach (Entity ent in Children) {
				ent.Draw(spriteBatch);
			}
		}
	}
}
