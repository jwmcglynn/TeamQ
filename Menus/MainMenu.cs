using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

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

		public MainMenu(Controller ctrl)
				: base(ctrl) {
			
			Controller.IsMouseVisible = true;

			TextWidget title = new TextWidget(this, "font", "Sputnik's Great Adventure");
			title.PositionPercent = new Vector2(0.5f, 0.3f);
			AddChild(title);

			float ypos = 50.0f;

			TextButton button = new TextButton(this, "Start Gym");
			button.PositionPercent = title.PositionPercent;
			button.Position = new Vector2(0.0f, ypos);
			button.CreateButton(new Rectangle(-50, -16, 100, 32));
			button.OnActivate += () => {
				Controller.ChangeEnvironment(new GymEnvironment(Controller));
			};
			AddChild(button);

			ypos += 50.0f;
			button = new TextButton(this, "Start Test Level");
			button.PositionPercent = title.PositionPercent;
			button.Position = new Vector2(0.0f, ypos);
			button.CreateButton(new Rectangle(-50, -16, 100, 32));
			button.OnActivate += () => {
				Controller.ChangeEnvironment(new TestLevelEnvironment(Controller));
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
