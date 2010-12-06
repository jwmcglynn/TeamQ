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

				Texture = m_normal = menu.contentManager.Load<Texture2D>(normal);
				m_over = menu.contentManager.Load<Texture2D>(over);

				OnMouseOver += () => { Texture = m_over; };
				OnMouseOut += () => { Texture = m_normal; VertexColor = Color.White; };
				OnMouseDown += () => { Texture = m_over; VertexColor = Color.Gray; };
				OnMouseUp += () => { Texture = m_normal; };
			}
		}

		private void CreateButton(Widget widget) {
			widget.Registration = new Vector2(widget.Texture.Width * 0.5f, widget.Texture.Height * 0.5f);

			int width = (int) Math.Round((double) widget.Texture.Width);
			int height = (int) Math.Round((double) widget.Texture.Height);
			widget.CreateButton(new Rectangle((int) Math.Round(-widget.Registration.X), (int) Math.Round(-widget.Registration.Y), width, height));

			widget.Zindex = 0.5f;
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
			m_logoText.Registration = new Vector2(m_logo.Texture.Width, m_logo.Texture.Height) * 0.5f;
			m_logoText.Zindex = 0.7f;
			AddChild(m_logoText);

			////
			Vector2 k_buttonPos = new Vector2(0.5f, 0.5f);
			const float k_buttonSpacing = 60.0f;
			float ypos = 50.0f;

			ImageButton startLevel = new ImageButton(this, "main_start_level", "main_start_level1");
			startLevel.PositionPercent = k_buttonPos;
			startLevel.Position = new Vector2(0.0f, ypos);
			CreateButton(startLevel);
			startLevel.OnActivate += () => {
				Controller.ChangeEnvironment(new Level1Environment(Controller));
			};
			AddChild(startLevel);

			ypos += k_buttonSpacing;

			ImageButton credits = new ImageButton(this, "main_credits", "main_credits1");
			credits.PositionPercent = k_buttonPos;
			credits.Position = new Vector2(0.0f, ypos);
			CreateButton(credits);
			credits.OnActivate += () => {
				// TODO.
			};
			AddChild(credits);

			ypos += k_buttonSpacing;

			ImageButton quit = new ImageButton(this, "main_quit", "main_quit1");
			quit.PositionPercent = k_buttonPos;
			quit.Position = new Vector2(0.0f, ypos);
			CreateButton(quit);
			quit.OnActivate += () => {
				Controller.Exit();
			};
			AddChild(quit);
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
