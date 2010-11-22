using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Physics = FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Tiled = Squared.Tiled;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;
using ProjectMercury.Renderers;
using System.IO;
using FarseerPhysics.Controllers;


namespace Sputnik {
	public class GameEnvironment : Environment {
		// Spawning/culling.
		public const float k_cullRadius = 300.0f; // Must be greater than spawn radius.
		public const float k_spawnRadius = 100.0f; // Must be less than cull radius.
		public SpawnController SpawnController { get; private set; }

		// Camera.
		public static Vector2 k_maxVirtualSize { get { return new Vector2(1680, 1050) * 1.25f; } }
		public Vector2 ScreenVirtualSize = new Vector2(1680, 1050);
		public Camera2D Camera;

		// Drawing.
		private SpriteBatch m_spriteBatch;
		private Tiled.Map m_map;

		private Matrix m_projection;

		// Particles.
		public SpriteBatchRenderer ParticleRenderer;

		// Physics.
		private Physics.DebugViewXNA m_debugView;

		public static float k_physicsScale = 1.0f / 50.0f; // 50 pixels = 1 meter.
		public static float k_invPhysicsScale = 50.0f; // ^ must be inverse.

		// Update loop.
		public float m_updateAccum; // How much time has passed relative to the physics world.

		// Black holes.
		public BlackHolePhysicsController BlackHoleController;
		public List<SpawnPoint> PossibleBlackHoleLocations = new List<SpawnPoint>();
		public List<SpawnPoint> SpawnedBlackHoles = new List<SpawnPoint>();

		// TEMP: Level scale.
		public const float k_levelScale = 2.0f;

		//Shiplists
		internal List<SquaretopiaShip> squares = new List<SquaretopiaShip>();
		
		internal List<TriangulusShip> triangles = new List<TriangulusShip>();
		internal List<CircloidShip> circles = new List<CircloidShip>();
		internal SputnikShip sputnik = null;


		public GameEnvironment(Controller ctrl)
				: base(ctrl) {
			
			CollisionWorld = new Physics.Dynamics.World(Vector2.Zero);
			Controller.Window.ClientSizeChanged += WindowSizeChanged;
			Camera = new Camera2D(this);
			WindowSizeChanged(null, null);

			// Create a new SpriteBatch which can be used to draw textures.
			m_spriteBatch = new SpriteBatch(ctrl.GraphicsDevice);

			ParticleRenderer = new SpriteBatchRenderer {
				GraphicsDeviceService = ctrl.Graphics
			};

			m_debugView = new Physics.DebugViewXNA(CollisionWorld);

			// Create collision notification callbacks.
			CollisionWorld.ContactManager.PreSolve += PreSolve;
			CollisionWorld.ContactManager.BeginContact += BeginContact;
			CollisionWorld.ContactManager.EndContact += EndContact;
			CollisionWorld.ContactManager.ContactFilter += ContactFilter;

			BlackHoleController = new BlackHolePhysicsController(300.0f, 100.0f * k_physicsScale, 9.0f * k_physicsScale); // 300 controls how strong the pull is towards the black hole.
																									// 100.0 determines the radius fore which black hole will have an effect on.
			CollisionWorld.AddController(BlackHoleController);

			// Farseer freaks out unless we call Update here when changing Environments.  FIXME: Why?
			Update(0.0f);
		}

		/// <summary>
		/// Called when the window size changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WindowSizeChanged(object sender, EventArgs e) {
			Rectangle rect = Controller.Window.ClientBounds;
			if (rect.Width == 0 || rect.Height == 0) return; // Do nothing, window was minimized.

			Controller.Graphics.PreferredBackBufferWidth = rect.Width;
			Controller.Graphics.PreferredBackBufferHeight = rect.Height;
			Controller.Graphics.ApplyChanges();

			// Correct virtual screen aspect ratio.
			float ratio = (float) rect.Width / rect.Height;
			ScreenVirtualSize = k_maxVirtualSize;
			if (ratio <= 16.0f / 10.0f) ScreenVirtualSize.X = ScreenVirtualSize.Y * ratio;
			else ScreenVirtualSize.Y = ScreenVirtualSize.X / ratio;

			// TODO: Make SpriteBatch drawing use this projection too.
			m_projection = Matrix.CreateOrthographicOffCenter(0.0f, rect.Width, rect.Height, 0.0f, -1.0f, 1.0f);

			Camera.WindowSizeChanged();
		}

		private enum Tile {
			None
			, AsteroidWall
			, WhiteBlock
			, GreyBlock
		};

		/// <summary>
		/// Load a map from file and create collision objects for it.
		/// </summary>
		/// <param name="filename">File to load map from.</param>
		public void LoadMap(string filename) {
			m_map = Tiled.Map.Load(Path.Combine(Controller.Content.RootDirectory, filename), Controller.Content);
			
			// Destroy and re-create collision body for map.
			DestroyCollisionBody();
			CreateCollisionBody(CollisionWorld, Physics.Dynamics.BodyType.Static);

			Vector2 tileHalfSize = new Vector2(m_map.TileWidth, m_map.TileHeight) / 2 * k_levelScale;
			Vector2 tileSize = new Vector2(m_map.TileWidth, m_map.TileHeight) * k_levelScale;

			foreach (Tiled.Layer layer in m_map.Layers.Values) {
				for (int x = 0; x < layer.Width; ++x)
				for (int y = 0; y < layer.Height; ++y) {
					Tile tileType = (Tile) layer.GetTile(x, y);
					
					switch (tileType) {
						case Tile.AsteroidWall:
						case Tile.GreyBlock:
							// Create collision.
							AddCollisionRectangle(tileHalfSize, new Vector2(tileSize.X * x, tileSize.Y * y) + tileHalfSize);
							break;
					}
				}
			}

			SpawnController = new SpawnController(this, m_map.ObjectGroups.Values);
		}

		/// <summary>
		/// Update the Environment each frame.
		/// </summary>
		/// <param name="elapsedTime">Time since last Update() call.</param>
		public override void Update(float elapsedTime) {
			m_updateAccum += elapsedTime;
			bool didUpdate = false;

			// Update physics.
			const float k_physicsStep = 1.0f / 60.0f;
			while (m_updateAccum > k_physicsStep) {
				m_updateAccum -= k_physicsStep;
				didUpdate = true;

				CollisionWorld.Step(k_physicsStep);

				if (SpawnController != null) SpawnController.Update(k_physicsStep);

				// Update entities.
				base.Update(k_physicsStep);
				Camera.Update(k_physicsStep);
			}

			if (!didUpdate) {
				// Update entities if they did not update above.
				if (SpawnController != null) SpawnController.Update(0.0f);
				base.Update(0.0f);
				Camera.Update(0.0f);
			}

			// Toggle debug view.
			if (Keyboard.GetState().IsKeyDown(Keys.F1) && !OldKeyboard.GetState().IsKeyDown(Keys.F1)) {
				if (m_debugView != null) m_debugView = null;
				else m_debugView = new Physics.DebugViewXNA(CollisionWorld);
			}

			// Main Menu = F2.
			if (Keyboard.GetState().IsKeyDown(Keys.F2) && !OldKeyboard.GetState().IsKeyDown(Keys.F2)) {
				Controller.ChangeEnvironment(new Menus.DebugMenu(Controller));
			}


			// Fullscreen toggle with Alt+Enter.
			if ((Keyboard.GetState().IsKeyDown(Keys.LeftAlt) || Keyboard.GetState().IsKeyDown(Keys.RightAlt))
					&& Keyboard.GetState().IsKeyDown(Keys.Enter) && !(
						(OldKeyboard.GetState().IsKeyDown(Keys.LeftAlt) || OldKeyboard.GetState().IsKeyDown(Keys.RightAlt))
							&& OldKeyboard.GetState().IsKeyDown(Keys.Enter))) {
				Controller.IsFullscreen = !Controller.IsFullscreen;
			}
		}

		/// <summary>
		/// Draw the world.
		/// </summary>
		public override void Draw() {
			// Draw map.
			if (m_map != null) {
				m_spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, Matrix.CreateScale(k_levelScale) * Camera.Transform);
				Camera.Position /= k_levelScale;
				m_map.Draw(m_spriteBatch, Camera.Rect);
				Camera.Position *= k_levelScale;
				m_spriteBatch.End();
			}

			// Draw entities.
			m_spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, Camera.Transform);
			Draw(m_spriteBatch);
			m_spriteBatch.End();

			if (m_debugView != null) {
				// Debug drawing.
				Matrix debugMatrix = Matrix.CreateScale(k_invPhysicsScale) * Camera.Transform;
				m_debugView.RenderDebugData(ref m_projection, ref debugMatrix);
			}
		}

		/// <summary>
		/// Farseer Physics callback.  Called when a contact point is created.
		/// </summary>
		/// <param name="contact">Contact point.</param>
		public void BeginContact(Physics.Dynamics.Contacts.Contact contact) {
			// PreSolve performs the same function as this, only continue if one is a sensor.
			if (!contact.FixtureA.IsSensor && !contact.FixtureB.IsSensor) return;
			
			HandleContact(contact);
		}

		/// <summary>
		/// Farseer Physics callback.  Called when two AABB's overlap to determine if they should collide.
		/// </summary>
		/// <param name="fixtureA">First fixture involved.</param>
		/// <param name="fixtureB">Second fixture involved.</param>
		/// <returns></returns>
		private bool ContactFilter(Physics.Dynamics.Fixture fixtureA, Physics.Dynamics.Fixture fixtureB) {
			// Get Entities from both shapes.
			Entity entA = (Entity) fixtureA.Body.UserData;
			Entity entB = (Entity) fixtureB.Body.UserData;

			// Determine if shapes agree to collide.
			return entA.ShouldCollide(entB, fixtureA, fixtureB) && entB.ShouldCollide(entA, fixtureB, fixtureA);
		}

		/// <summary>
		/// Farseer Physics callback.  Handles the case where two objects collide.
		/// </summary>
		/// <param name="contact">Contact point.</param>
		/// <param name="oldManifold">Manifold from last update.</param>
		private void PreSolve(Physics.Dynamics.Contacts.Contact contact, ref Physics.Collision.Manifold oldManifold) {
			HandleContact(contact);
		}

		/// <summary>
		/// Farseer Physics callback.  Called when a contact is destroyed.
		/// </summary>
		/// <param name="contact">Contact point.</param>
		private void EndContact(Physics.Dynamics.Contacts.Contact contact) {
			// Get Entities from both shapes.
			Entity entA = (Entity) contact.FixtureA.Body.UserData;
			Entity entB = (Entity) contact.FixtureB.Body.UserData;

			entA.OnSeparate(entB, contact);
			entB.OnSeparate(entA, contact);
		}

		/// <summary>
		/// Handle contact interactions.
		/// </summary>
		/// <param name="contact">Contact point.</param>
		private void HandleContact(Physics.Dynamics.Contacts.Contact contact) {
			if (!contact.IsTouching()) return;

			// Get Entities from both shapes.
			Entity entA = (Entity) contact.FixtureA.Body.UserData;
			Entity entB = (Entity) contact.FixtureB.Body.UserData;

			entA.OnCollide(entB, contact);
			entB.OnCollide(entA, contact);
		}
	}
}
