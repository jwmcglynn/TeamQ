using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Sputnik {
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Controller : Microsoft.Xna.Framework.Game {
		private GraphicsDeviceManager m_graphics;
		private Environment m_env;

		public Vector2 WindowedSize = new Vector2(1280, 800);
		private bool m_fullscreen = false;

		/// <summary>
		/// Made this to play with random positions
		/// </summary>
		public GraphicsDeviceManager Graphics {
			get {
				return m_graphics;
			}
		}

		public Controller() {
			m_graphics = new GraphicsDeviceManager(this);

			Window.AllowUserResizing = true;
			Window.Title = "Sputnik";
			Content.RootDirectory = "Content";

			OldKeyboard.m_state = Keyboard.GetState();
		}

		public bool IsFullscreen {
			get {
				return m_fullscreen;
			}

			set {
				if (value == m_fullscreen) return;
				m_fullscreen = value;

				if (m_fullscreen) {
					// Save previous windowed size.
					WindowedSize.X = Graphics.GraphicsDevice.Viewport.Width;
					WindowedSize.Y = Graphics.GraphicsDevice.Viewport.Height;

					Console.WriteLine("Graphics size = " + GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width + ", " + GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);

					// Set backbuffer size to screen size.
					Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
					Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
				} else {
					// Set backbuffer size to window size.
					Graphics.PreferredBackBufferWidth = (int) WindowedSize.X;
					Graphics.PreferredBackBufferHeight = (int) WindowedSize.Y;
					Console.WriteLine("To windowed: " + WindowedSize);
				}

				Graphics.ToggleFullScreen();
			}
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent() {
			Sound.Initialize();

			// Prepare DebugView.
			FarseerPhysics.DebugViewXNA.LoadContent(GraphicsDevice, Content);

			// Create first environment.
			m_env = new GymEnvironment(this);
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent() {
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime) {
			// Allows the game to exit.
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
					|| Keyboard.GetState().IsKeyDown(Keys.Escape)) {
				this.Exit();
			}

			Sound.Update();

			m_env.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
			base.Update(gameTime);

			// Get the keyboard state for the next pass.
			OldKeyboard.m_state = Keyboard.GetState();
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(Color.Black);
			m_env.Draw();
			base.Draw(gameTime);
		}
	}
}