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
		private Environment m_env;
		private Environment m_nextEnv;

		// Windowed/fullscreen.
		private Vector2 m_windowedSize = new Vector2(1280, 800);
		private bool m_fullscreen = false;

		// FPS Counters.
		private double m_frameTime;
		private int m_fps;
		private int m_frameCounter;

		public GraphicsDeviceManager Graphics { get; private set; }

		public Controller() {
			Graphics = new GraphicsDeviceManager(this);

			Window.AllowUserResizing = true;
			Window.Title = "Sputnik";
			Content.RootDirectory = "Content";
			Graphics.SynchronizeWithVerticalRetrace = true;

			Graphics.IsFullScreen = true;
			m_fullscreen = true;

			// Set backbuffer size to screen size.
			Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
			Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

			IsFixedTimeStep = false;

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
					m_windowedSize.X = Graphics.GraphicsDevice.Viewport.Width;
					m_windowedSize.Y = Graphics.GraphicsDevice.Viewport.Height;

					// Set backbuffer size to screen size.
					Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
					Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
				} else {
					// Set backbuffer size to window size.
					Graphics.PreferredBackBufferWidth = (int) m_windowedSize.X;
					Graphics.PreferredBackBufferHeight = (int) m_windowedSize.Y;
				}

				Graphics.ToggleFullScreen();
			}
		}

		public void ChangeEnvironment(Environment env) {
			if (m_nextEnv != null) throw new InvalidOperationException("ChangeEnvironment called again before change occurred.");
			m_nextEnv = env;
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
			m_env = new Menus.MainMenu(this);
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
			// Try changing environment.
			if (m_nextEnv != null) {
				m_env.Dispose();
				m_env = m_nextEnv;
				m_nextEnv = null;
			}

			// Game loop.
			Sound.Update();

			m_env.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
			base.Update(gameTime);

			// Toggle fullscreen toggle with Alt+Enter.
			if ((Keyboard.GetState().IsKeyDown(Keys.LeftAlt) || Keyboard.GetState().IsKeyDown(Keys.RightAlt))
					&& Keyboard.GetState().IsKeyDown(Keys.Enter) && !(
						(OldKeyboard.GetState().IsKeyDown(Keys.LeftAlt) || OldKeyboard.GetState().IsKeyDown(Keys.RightAlt))
							&& OldKeyboard.GetState().IsKeyDown(Keys.Enter))) {
				IsFullscreen = !IsFullscreen;
			}

			// Get the keyboard state for the next pass.
			OldKeyboard.m_state = Keyboard.GetState();
			OldGamePad.m_state = GamePad.GetState(PlayerIndex.One);
			OldMouse.m_state = Mouse.GetState();

			// FPS counter.
			m_frameCounter++;
			m_frameTime += gameTime.ElapsedGameTime.TotalSeconds;
			if (m_frameTime >= 1.0f) {
				m_fps = m_frameCounter;
				m_frameTime -= 1.0f;
				m_frameCounter = 0;
				Window.Title = "Sputnik (" + m_fps + " fps)";
			}
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(new Color(2, 2, 71));
			m_env.Draw();
			base.Draw(gameTime);
		}
	}
}