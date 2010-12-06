using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace Sputnik.Menus {
	public class LevelEndHUD : Menu {
		private GameEnvironment Environment;

		private RectangleWidget m_background;

		public LevelEndHUD(GameEnvironment env)
			: base(env.Controller) {

			Environment = env;

			Sound.PlayCue("stage_clear");

			/////

			// Background color.
			m_background = new RectangleWidget(this, ScreenSize.X * 0.5f, ScreenSize.Y * 0.5f);
			m_background.PositionPercent = new Vector2(0.5f, 0.5f);
			m_background.Position = new Vector2(0.0f, 20.0f);
			m_background.VertexColor = Color.Black;
			m_background.Alpha = 0.5f;
			m_background.Zindex = 1.0f;
			AddChild(m_background);

			// Title.
			TextWidget winTitle = new TextWidget(this, "font", "You win!");
			winTitle.PositionPercent = new Vector2(0.5f, 0.5f);
			winTitle.Position = new Vector2(0.0f, -50.0f);
			winTitle.Zindex = 0.5f;
			AddChild(winTitle);

			// Score.
			TextWidget score = new TextWidget(this, "font", String.Format("Time: {0:0.00} seconds.", env.LevelTimeSpent));
			score.PositionPercent = new Vector2(0.5f, 0.5f);
			score.Position = new Vector2(0.0f, 0.0f);
			score.Zindex = 0.5f;
			AddChild(score);

			// Press key to exit.
			TextWidget exit = new TextWidget(this, "font", "Press SPACE for Main Menu");
			exit.PositionPercent = new Vector2(0.5f, 0.5f);
			exit.Position = new Vector2(0.0f, 50.0f);
			exit.Zindex = 0.5f;
			AddChild(exit);
		}

		public override void Update(float elapsedTime) {
			m_background.FullWidth = ScreenSize.X * 0.5f;
			m_background.Height = ScreenSize.Y * 0.5f;

			KeyboardState kb = Keyboard.GetState();
			KeyboardState oldKb = OldKeyboard.GetState();

			GamePadState gamepad = GamePad.GetState(PlayerIndex.One);
			GamePadState oldGamepad = GamePad.GetState(PlayerIndex.One);

			if ((kb.IsKeyDown(Keys.Space) && !oldKb.IsKeyDown(Keys.Space))
					|| (kb.IsKeyDown(Keys.Escape) && !oldKb.IsKeyDown(Keys.Escape))
					|| (kb.IsKeyDown(Keys.Enter) && !oldKb.IsKeyDown(Keys.Enter))
					|| (gamepad.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A) && !oldGamepad.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A))
					|| (gamepad.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Start) && !oldGamepad.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Start))) {
				Controller.ChangeEnvironment(new Menus.MainMenu(Controller));
			}

			base.Update(elapsedTime);
		}
	}
}
