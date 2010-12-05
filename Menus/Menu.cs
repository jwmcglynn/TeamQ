using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sputnik.Menus {
	public class Menu : Environment {
		private SpriteBatch m_spriteBatch;
		internal List<Widget> Buttons = new List<Widget>();
		public Vector2 ScreenSize { get; private set; }

		// Mouse tracking.
		private Widget m_activeButton;
		private bool m_mouseWasPressed = false;

		public Menu(Controller ctrl)
				: base(ctrl) {
			Controller.Window.ClientSizeChanged += WindowSizeChanged;
			WindowSizeChanged(null, null); // Prime the ScreenSize.

			// Create a new SpriteBatch which can be used to draw textures.
			m_spriteBatch = new SpriteBatch(ctrl.GraphicsDevice);
		}

		/// <summary>
		/// Called when the window size changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WindowSizeChanged(object sender, EventArgs e) {
			Rectangle rect = Controller.Window.ClientBounds;
			ScreenSize = new Vector2(rect.Width, rect.Height);
		}

		/// <summary>
		/// Update the Environment each frame.
		/// </summary>
		/// <param name="elapsedTime">Time since last Update() call.</param>
		public override void Update(float elapsedTime) {
			MouseState mouse = Mouse.GetState();
			Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
			bool mousePressed = (mouse.LeftButton == ButtonState.Pressed);

			List<Widget> collidingButtons = Buttons.FindAll(b => b.Collides(mousePos));
			Widget button = null;
			if (collidingButtons.Count > 0) button = collidingButtons.OrderByDescending(b => b.Zindex).First();

			// We switched to a new button.
			if (m_activeButton != button) {
				// Unset current button.
				if (m_activeButton != null) {
					m_activeButton.DispatchOnMouseOut();
					if (m_mouseWasPressed) m_activeButton.DispatchOnMouseUp(false);
				}

				// Activate new one.
				m_activeButton = button;
				if (m_activeButton != null) {
					m_activeButton.DispatchOnMouseOver();
					
					if (mousePressed && !m_mouseWasPressed) {
						m_activeButton.DispatchOnMouseDown();
					}
				}
			} else if (m_activeButton != null) {
				// Button is the same.  Update it.
				if (m_mouseWasPressed && !mousePressed) {
					m_activeButton.DispatchOnMouseUp(true);
				} else if (!m_mouseWasPressed && mousePressed) {
					m_activeButton.DispatchOnMouseDown();
				}
			}

			m_mouseWasPressed = mousePressed;
			base.Update(elapsedTime);
		}

		/// <summary>
		/// Draw the world.
		/// </summary>
		public override void Draw() {
			// Draw entities.
			m_spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
			Draw(m_spriteBatch);
			m_spriteBatch.End();
		}
	}
}
