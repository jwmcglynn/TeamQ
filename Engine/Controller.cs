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
			m_graphics.PreferMultiSampling = true;
			m_graphics.PreferredBackBufferWidth = 1280; // TODO: We want to go up to 1680x1050.
			m_graphics.PreferredBackBufferHeight = 800;
			m_graphics.ApplyChanges();

			Window.Title = "Sputnik";
			Content.RootDirectory = "Content";

			OldKeyboard.m_state = Keyboard.GetState();
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize() {
			// TODO: Add your initialization logic here

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent() {
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