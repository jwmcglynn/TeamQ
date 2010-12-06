using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sputnik.Menus {
	class MainMenu : Menu {
		private Widget m_background;
		private Widget m_logo;
		private Widget m_logoText;

		private class TextButton : TextWidget {
			public TextButton(Menu menu, string text)
					: base(menu, "font", text) {
				OnMouseOver += () => { VertexColor = Color.Aquamarine; };
				OnMouseOut += () => { VertexColor = Color.White; };
				OnMouseDown += () => { VertexColor = Color.Blue; };
			}
		}

		private class ImageButton : Widget {
			private Texture2D m_normal;
			private Texture2D m_over;
			public ImageButton(Menu menu, string normal, string over)
					: base(menu) {

				m_normal = menu.contentManager.Load<Texture2D>(normal);
				m_over = menu.contentManager.Load<Texture2D>(over);

				OnMouseOver += () => { Texture = m_over; };
				OnMouseOut += () => { Texture = m_normal; VertexColor = Color.White; };
				OnMouseDown += () => { Texture = m_over; VertexColor = Color.Gray; };
				OnMouseUp += () => { Texture = m_normal; };
			}
		}

		public MainMenu(Controller ctrl)
			: base(ctrl) {

			Controller.IsMouseVisible = true;

			// Background.
			m_background = new Widget(this);
			m_background.LoadTexture(contentManager, "space-desktop");
			m_background.PositionPercent = new Vector2(0.5f, 0.5f);
			m_background.Zindex = 1.0f;
			m_background.Registration = new Vector2(m_background.Texture.Width, m_background.Texture.Height) * 0.5f;
			AddChild(m_background);

			// Logo.
			m_logo = new Widget(this);
			m_logo.LoadTexture(contentManager, "logo_sputnik");
			m_logo.PositionPercent = new Vector2(0.5f, 0.3f);
			m_logo.Position = new Vector2(-75.0f, 0.0f);
			m_logo.Zindex = 0.8f;
			m_logo.Registration = new Vector2(m_logo.Texture.Width, m_logo.Texture.Height) * 0.5f;
			AddChild(m_logo);

			// Logo text.
			m_logoText = new Widget(this);
			m_logoText.LoadTexture(contentManager, "logo");
			m_logoText.PositionPercent = new Vector2(0.5f, 0.3f);
			m_logoText.Position = new Vector2(-75.0f, 25.0f);
			m_logoText.Zindex = 0.7f;
			m_logoText.Registration = new Vector2(m_logo.Texture.Width, m_logo.Texture.Height) * 0.5f;
			AddChild(m_logoText);


			TextWidget title = new TextWidget(this, "font", "Sputnik's Great Adventure");
			title.PositionPercent = new Vector2(0.5f, 0.3f);
			AddChild(title);

			float ypos = 50.0f;

			TextButton button;
			button = new TextButton(this, "Start Level 1");
			button.PositionPercent = title.PositionPercent;
			button.Position = new Vector2(0.0f, ypos);
			button.CreateButton(new Rectangle(-50, -16, 100, 32));
			button.OnActivate += () => {
				Controller.ChangeEnvironment(new Level1Environment(Controller));
			};
			AddChild(button);

			ypos += 50.0f;
			button = new TextButton(this, "Quit");
			button.PositionPercent = title.PositionPercent;
			button.Position = new Vector2(0.0f, ypos);
			button.CreateButton(new Rectangle(-50, -16, 100, 32));
			button.OnActivate += () => {
				Controller.Exit();
			};
			AddChild(button);
		}

		public override void Update(float elapsedTime) {
			float scale = Math.Max(ScreenSize.X / m_background.Texture.Width, ScreenSize.Y / m_background.Texture.Height) * 1.5f;
			m_background.Scale = scale;
			m_logo.Scale = scale;
			m_logoText.Scale = scale;

			base.Update(elapsedTime);
		}

		public override void Dispose() {
			Controller.IsMouseVisible = false;
			base.Dispose();
		}
	}
}
