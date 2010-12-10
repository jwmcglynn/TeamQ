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
		public ParticleEffect ExplosionEffect;
		public ParticleEffect ThrusterEffect;
		public ParticleEffect FrostThrusterEffect;
		public ParticleEffect AttachEffect;
		public ParticleEffect BlackHoleEffect;
		public ParticleEffect AlertEffect;
		public ParticleEffect TractorBeamEffect;

		private List<ParticleEffect> EffectsBelowShip = new List<ParticleEffect>();
		private List<ParticleEffect> EffectsAboveShip = new List<ParticleEffect>();

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

		//Black Hole List
		internal List<BlackHole> blackHoles = new List<BlackHole>();

		// Boss Patrol points.
		public List<Vector2> SpawnedBossPatrolPoints = new List<Vector2>();

		//Shiplists
		internal List<SquaretopiaShip> squares = new List<SquaretopiaShip>();
		internal List<TriangulusShip> triangles = new List<TriangulusShip>();
		internal List<CircloidShip> circles = new List<CircloidShip>();
		internal SputnikShip sputnik = null;
		internal Boss Boss = null;

		// HUD.
		public Menus.HUD HUD;
		public Menus.LevelEndHUD LevelEndHUD;
		public float LevelTimeSpent = 0.0f;
		public bool LevelDone = false;
		
		// Difficulty.
		public bool isFrostMode = false;

		public GameEnvironment(Controller ctrl)
				: base(ctrl) {

			Sound.StopAll(true);

			CollisionWorld = new Physics.Dynamics.World(Vector2.Zero);
			Controller.Window.ClientSizeChanged += WindowSizeChanged;
			Camera = new Camera2D(this);
			WindowSizeChanged(null, null);

			// Create a new SpriteBatch which can be used to draw textures.
			m_spriteBatch = new SpriteBatch(ctrl.GraphicsDevice);

			ParticleRenderer = new SpriteBatchRenderer {
				GraphicsDeviceService = ctrl.Graphics
			};

			ExplosionEffect = contentManager.Load<ParticleEffect>("ExplosionEffect");
			ThrusterEffect = contentManager.Load<ParticleEffect>("ThrusterEffect");
			FrostThrusterEffect = contentManager.Load<ParticleEffect>("FrostThrusterEffect");
			AttachEffect = contentManager.Load<ParticleEffect>("AttachEffect");
			BlackHoleEffect = contentManager.Load<ParticleEffect>("BlackHoleEffect");
			AlertEffect = contentManager.Load<ParticleEffect>("AlertEffect");
			TractorBeamEffect = contentManager.Load<ParticleEffect>("TractorBeamEffect");

			EffectsBelowShip.Add(ThrusterEffect);
			EffectsBelowShip.Add(FrostThrusterEffect);
			EffectsBelowShip.Add(TractorBeamEffect);

			EffectsAboveShip.Add(ExplosionEffect);
			EffectsAboveShip.Add(AttachEffect);
			EffectsAboveShip.Add(BlackHoleEffect);
			EffectsAboveShip.Add(AlertEffect);

			ParticleRenderer.LoadContent(contentManager);

			foreach (var e in EffectsBelowShip) {
				e.Initialise();
				e.LoadContent(contentManager);
			}

			foreach (var e in EffectsAboveShip) {
				e.Initialise();
				e.LoadContent(contentManager);
			}

			// Create collision notification callbacks.
			CollisionWorld.ContactManager.PreSolve += PreSolve;
			CollisionWorld.ContactManager.BeginContact += BeginContact;
			CollisionWorld.ContactManager.EndContact += EndContact;
			CollisionWorld.ContactManager.ContactFilter += ContactFilter;

			// first parameter controls how strong the pull is; the second parameter controls the radius of the pull.
			BlackHoleController = new BlackHolePhysicsController(500.0f, 250.0f * k_physicsScale, 9.0f * k_physicsScale);
			CollisionWorld.AddController(BlackHoleController);
			
			ShipCollisionAvoidanceController shipAvoid = new ShipCollisionAvoidanceController(150.0f * k_physicsScale);
			shipAvoid.MaxRadius = 80.0f * k_physicsScale;
			CollisionWorld.AddController(shipAvoid);

			// HUD.
			HUD = new Menus.HUD(this);

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

		byte[,] collision = {
			{0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0},
			{0, 1, 1, 1,  1, 1, 0, 0,  0, 0, 0, 0,  0, 0, 0},
			{0, 1, 1, 1,  1, 1, 0, 0,  0, 0, 0, 0,  0, 0, 0},
			{0, 1, 1, 1,  1, 1, 0, 0,  0, 0, 0, 0,  0, 0, 0},
			{0, 1, 1, 1,  1, 1, 0, 0,  0, 0, 0, 0,  0, 0, 0},
			{0, 1, 1, 1,  1, 1, 0, 0,  0, 0, 0, 0,  0, 0, 0},
			{0, 0, 0, 0,  1, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0},
			{0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0},
			{0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0},
			{0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0},
			{0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0}
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

			Vector2 tileHalfSize = new Vector2(m_map.TileWidth, m_map.TileHeight) / 2;
			Vector2 tileSize = new Vector2(m_map.TileWidth, m_map.TileHeight);

			bool[,] levelCollision = new bool[m_map.Width, m_map.Height];

			foreach (Tiled.Layer layer in m_map.Layers.Values) {
				for (int x = 0; x < layer.Width; ++x)
				for (int y = 0; y < layer.Height; ++y) {
					int tileId = layer.GetTile(x, y) - 1;
					if (tileId < 0) continue;

					int row = tileId / 15;
					int col = tileId - row * 15;
					if (row >= 11 || col >= 15) continue;

					levelCollision[x, y] = (collision[row, col] != 0);
				}
			}

			// Go through collision and try to create large horizontal collision shapes.
			for (int y = 0; y < m_map.Height; ++y) {
				int firstX = 0;
				bool hasCollision = false;

				for (int x = 0; x < m_map.Width; ++x) {
					if (levelCollision[x, y]) {
						if (hasCollision) continue;
						else {
							hasCollision = true;
							firstX = x;
						}
					} else {
						if (hasCollision) {
							hasCollision = false;
							int tilesWide = x - firstX;
							if (tilesWide == 1) continue;

							for (int i = firstX; i <= x; ++i) levelCollision[i, y] = false;

							AddCollisionRectangle(
								tileHalfSize * new Vector2(tilesWide, 1.0f)
								, new Vector2(tileSize.X * (x - (float) tilesWide / 2), tileSize.Y * (y + 0.5f))
							);
						}
					}
				}

				// Create final collision.
				if (hasCollision) {
					for (int i = firstX; i < m_map.Width; ++i) levelCollision[i, y] = false;

					int tilesWide = m_map.Width - firstX;
					AddCollisionRectangle(
						tileHalfSize * new Vector2(tilesWide, 1.0f)
						, new Vector2(tileSize.X * (m_map.Width - (float) tilesWide / 2), tileSize.Y * (y + 0.5f))
					);
				}
			}

			// Go through collision and try to create large vertical collision shapes.
			for (int x = 0; x < m_map.Width; ++x) {
				int firstY = 0;
				bool hasCollision = false;

				for (int y = 0; y < m_map.Height; ++y) {
					if (levelCollision[x, y]) {
						if (hasCollision) continue;
						else {
							hasCollision = true;
							firstY = y;
						}
					} else {
						if (hasCollision) {
							hasCollision = false;
							int tilesTall = y - firstY;

							AddCollisionRectangle(
								tileHalfSize * new Vector2(1.0f, tilesTall)
								, new Vector2(tileSize.X * (x + 0.5f), tileSize.Y * (y - (float) tilesTall / 2))
							);
						}
					}
				}

				// Create final collision.
				if (hasCollision) {
					int tilesTall = m_map.Height - firstY;
					AddCollisionRectangle(
						tileHalfSize * new Vector2(1.0f, tilesTall)
						, new Vector2(tileSize.X * (x + 0.5f), tileSize.Y * (m_map.Height - (float) tilesTall / 2))
					);
				}
			}

			SpawnController = new SpawnController(this, m_map.ObjectGroups.Values);
		}

		/// <summary>
		/// Update the Environment each frame.
		/// </summary>
		/// <param name="elapsedTime">Time since last Update() call.</param>
		public override void Update(float elapsedTime) {
			// Toggle debug view.
			if (Keyboard.GetState().IsKeyDown(Keys.F1) && !OldKeyboard.GetState().IsKeyDown(Keys.F1)) {
				if (m_debugView != null) m_debugView = null;
				else m_debugView = new Physics.DebugViewXNA(CollisionWorld);
			}

			// Frost mode.
			if (Keyboard.GetState().IsKeyDown(Keys.F4) && !OldKeyboard.GetState().IsKeyDown(Keys.F4)) {
				this.isFrostMode = !this.isFrostMode;
			}

			if (LevelDone) {
				if (LevelEndHUD == null) {
					LevelEndHUD = new Menus.LevelEndHUD(this);
				}

				LevelEndHUD.Update(elapsedTime);
				return;
			}

			// Debug Menu = F10.
			if (Keyboard.GetState().IsKeyDown(Keys.F10) && !OldKeyboard.GetState().IsKeyDown(Keys.F10)) {
				Controller.ChangeEnvironment(new Menus.DebugMenu(Controller));
			}

			// Back to main menu.
			if (Keyboard.GetState().IsKeyDown(Keys.Escape) && !OldKeyboard.GetState().IsKeyDown(Keys.Escape)) {
				Controller.ChangeEnvironment(new Menus.MainMenu(Controller));
			}

			LevelTimeSpent += elapsedTime;
		
			if (elapsedTime > 0.0f) {
				// Update physics.
				CollisionWorld.Step(elapsedTime);

				if (SpawnController != null) SpawnController.Update(elapsedTime);

				// Update entities.
				base.Update(elapsedTime);
				Camera.Update(elapsedTime);
				HUD.Update(elapsedTime);

				// Particles.
				foreach (var effect in EffectsAboveShip) effect.Update(elapsedTime);
				foreach (var effect in EffectsBelowShip) effect.Update(elapsedTime);
			}
		}

		/// <summary>
		/// Draw the world.
		/// </summary>
		public override void Draw() {
			Matrix tform = Camera.Transform;

			// Draw map.
			if (m_map != null) {
				m_map.Draw(m_spriteBatch, Camera.Rect, () => {
					m_spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, tform);
				});
			}

			// Below ship particles.
			foreach (var effect in EffectsBelowShip) {
				ParticleRenderer.RenderEffect(effect, ref tform);
			}

			// Draw entities.
			m_spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, tform);
			Draw(m_spriteBatch);
			m_spriteBatch.End();

			// Above ship particles.
			foreach (var effect in EffectsAboveShip) {
				ParticleRenderer.RenderEffect(effect, ref tform);
			}

			if (m_debugView != null) {
				// Debug drawing.
				Matrix debugMatrix = Matrix.CreateScale(k_invPhysicsScale) * tform;
				m_debugView.RenderDebugData(ref m_projection, ref debugMatrix);
			}

			// Draw HUD.
			HUD.Draw();

			// Draw level end HUD.
			if (LevelEndHUD != null) LevelEndHUD.Draw();
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