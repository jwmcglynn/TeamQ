using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sputnik.Menus {
	class RectangleWidget : Widget {
		public float Height;
		public float FullWidth;

		public float FillPercent = 1.0f;

		public RectangleWidget(Menu env, float width, float height)
			: base(env) {

			Height = height;
			FullWidth = width;

			Texture2D dummyTexture = new Texture2D(env.Controller.GraphicsDevice, 1, 1);
			dummyTexture.SetData(new Color[] { Color.White });

			Texture = dummyTexture;
		}

		public override void Draw(SpriteBatch spriteBatch) {
			spriteBatch.Draw(Texture, AbsolutePosition, new Rectangle(
				(int) Math.Round(-FullWidth / 2)
				, (int) Math.Round(-Height / 2)
				, (int) Math.Round(FullWidth * FillPercent)
				, (int) Math.Round(Height)
			), VertexColor * Alpha, 0.0f, new Vector2(FullWidth / 2, Height / 2), 1.0f, SpriteEffects.None, Zindex);
		}
	}

	public class HUD : Menu {
		private GameEnvironment Environment;

		public Widget Cursor;
		public Widget BossDirection;

		private RectangleWidget BossHealth;
		private RectangleWidget BossHealthBG;

		private RectangleWidget ShipHealth;
		private RectangleWidget ShipHealthShield;
		private RectangleWidget ShipHealthBG;

		private RectangleWidget SputnikTimeLeft;
		private RectangleWidget SputnikTimeLeftBG;

		public HUD(GameEnvironment env)
				: base(env.Controller) {

			Environment = env;

			// Cursor.
			Cursor = new Widget(this);
			Cursor.LoadTexture(contentManager, "bullet_arrow");
			Cursor.Registration = new Vector2(Cursor.Texture.Width, Cursor.Texture.Height) * 0.5f;
			Cursor.Scale = 0.2f;
			AddChild(Cursor);

			// Arrow pointing towards boss.
			BossDirection = new Widget(this);
			BossDirection.LoadTexture(contentManager, "arrow");
			BossDirection.Registration = new Vector2(BossDirection.Texture.Width, BossDirection.Texture.Height) * 0.5f;
			BossDirection.PositionPercent = new Vector2(1.0f, 1.0f);
			BossDirection.Position = new Vector2(-75.0f, -75.0f);
			AddChild(BossDirection);

			/////

			// Boss health meter.
			BossHealth = new RectangleWidget(this, ScreenSize.X * 0.75f - 4.0f, 20.0f - 4.0f);
			BossHealth.PositionPercent = new Vector2(0.5f, 0.0f);
			BossHealth.Position = new Vector2(0.0f, 20.0f);
			BossHealth.VertexColor = Color.DarkRed;
			BossHealth.Zindex = 0.0f;
			AddChild(BossHealth);

			// Boss health background.
			BossHealthBG = new RectangleWidget(this, ScreenSize.X * 0.75f, 20.0f);
			BossHealthBG.PositionPercent = new Vector2(0.5f, 0.0f);
			BossHealthBG.Position = new Vector2(0.0f, 20.0f);
			BossHealthBG.VertexColor = Color.Black;
			BossHealthBG.Zindex = 1.0f;
			AddChild(BossHealthBG);

			/////

			// Ship health meter.
			ShipHealth = new RectangleWidget(this, ScreenSize.X * 0.25f - 4.0f, 20.0f - 4.0f);
			ShipHealth.PositionPercent = new Vector2(0.0f, 1.0f);
			ShipHealth.Position = new Vector2(10.0f + ScreenSize.X * 0.25f / 2, -20.0f);
			ShipHealth.VertexColor = Color.Green;
			ShipHealth.Zindex = 0.5f;
			ShipHealth.Alpha = 0.0f;
			AddChild(ShipHealth);

			// Ship health shield.
			ShipHealthShield = new RectangleWidget(this, ScreenSize.X * 0.25f - 4.0f, 20.0f - 4.0f);
			ShipHealthShield.PositionPercent = new Vector2(0.0f, 1.0f);
			ShipHealthShield.Position = new Vector2(10.0f + ScreenSize.X * 0.25f / 2, -20.0f);
			ShipHealthShield.VertexColor = Color.LightBlue;
			ShipHealthShield.Zindex = 0.0f;
			ShipHealthShield.Alpha = 0.0f;
			AddChild(ShipHealthShield);

			// Ship health background.
			ShipHealthBG = new RectangleWidget(this, ScreenSize.X * 0.25f, 20.0f);
			ShipHealthBG.PositionPercent = new Vector2(0.0f, 1.0f);
			ShipHealthBG.Position = new Vector2(10.0f + ScreenSize.X * 0.25f / 2, -20.0f);
			ShipHealthBG.VertexColor = Color.Black;
			ShipHealthBG.Zindex = 1.0f;
			ShipHealthBG.Alpha = 0.0f;
			AddChild(ShipHealthBG);

			/////

			// Sputnik time left.
			SputnikTimeLeft = new RectangleWidget(this, 100.0f - 4.0f, 10.0f - 4.0f);
			SputnikTimeLeft.VertexColor = Color.White;
			SputnikTimeLeft.Zindex = 0.0f;
			SputnikTimeLeft.Alpha = 0.0f;
			AddChild(SputnikTimeLeft);

			// Sputnik time left background.
			SputnikTimeLeftBG = new RectangleWidget(this, 100.0f, 10.0f);
			SputnikTimeLeftBG.VertexColor = Color.Black;
			SputnikTimeLeftBG.Zindex = 1.0f;
			SputnikTimeLeftBG.Alpha = 0.0f;
			AddChild(SputnikTimeLeftBG);
		}

		public override void Update(float elapsedTime) {
			ShipHealth.VertexColor = Environment.isFrostMode ? Color.HotPink : Color.Green;

			BossDirection.Rotation = Angle.Direction(Environment.Camera.ScreenToWorld(BossDirection.AbsolutePosition), Environment.Boss.Position) + MathHelper.PiOver2;
			BossHealth.FillPercent = Environment.Boss.HealthPercent;

			// Update width of bars if screen was resized.
			BossHealth.FullWidth = ScreenSize.X * 0.75f - 4.0f;
			BossHealthBG.FullWidth = ScreenSize.X * 0.75f;

			ShipHealth.FullWidth = ScreenSize.X * 0.25f - 4.0f;
			ShipHealth.Position = new Vector2(10.0f + ScreenSize.X * 0.25f / 2, -20.0f);
			ShipHealthBG.FullWidth = ScreenSize.X * 0.25f;
			ShipHealthBG.Position = new Vector2(10.0f + ScreenSize.X * 0.25f / 2, -20.0f);

			// Ship and/or sputnik health bar.
			if (Environment.sputnik.controlled != null) {
				ShipHealth.FillPercent = Environment.sputnik.controlled.HealthPercent;
				if (ShipHealth.FillPercent > 1.0f) {
					ShipHealthShield.FillPercent = ShipHealth.FillPercent - 1.0f;
					ShipHealth.FillPercent = 1.0f;
					ShipHealthShield.Alpha = 1.0f;
				} else {
					ShipHealthShield.Alpha = 0.0f;
				}

				ShipHealth.Alpha = 1.0f;
				ShipHealthBG.Alpha = 1.0f;

				SputnikTimeLeft.Alpha = 0.0f;
				SputnikTimeLeftBG.Alpha = 0.0f;
			} else {
				ShipHealth.Alpha = 0.0f;
				ShipHealthShield.Alpha = 0.0f;
				ShipHealthBG.Alpha = 0.0f;

				if (!Environment.sputnik.IsInvulnerable) {
					SputnikTimeLeft.Alpha = 1.0f;
					SputnikTimeLeftBG.Alpha = 1.0f;

					Vector2 offset = new Vector2(0.0f, -100.0f);
					SputnikTimeLeft.Position = Environment.Camera.WorldToScreen(Environment.sputnik.Position + offset);
					SputnikTimeLeftBG.Position = SputnikTimeLeft.Position;

					SputnikTimeLeft.FillPercent = Environment.sputnik.TimerPercent;
				} else {
					SputnikTimeLeft.Alpha = 0.0f;
					SputnikTimeLeftBG.Alpha = 0.0f;
				}
			}

			base.Update(elapsedTime);
		}
	}
}
