using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sputnik.Menus {
	class MainMenu : Menu {
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

		public override void Dispose() {
			Controller.IsMouseVisible = false;
			base.Dispose();
		}
	}
}
