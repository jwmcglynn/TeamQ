using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Physics = FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Tiled = Squared.Tiled;
using System.IO;

using FarseerPhysics.Controllers;

namespace Sputnik {
	class GameEnvironment : Environment {
		private SpriteBatch m_spriteBatch;
		private Tiled.Map m_map;
		public Camera2D Camera;

		// Physics.
		private Physics.DebugViewXNA m_debugView;
		public Physics.Dynamics.World CollisionWorld;

		public static float k_physicsScale = 1.0f / 50.0f; // 50 pixels = 1 meter.
		public static float k_invPhysicsScale = 50.0f; // ^ must be inverse.

		private Matrix m_projection;

		// Update loop.
		public float m_updateAccum; // How much time has passed relative to the physics world.

		//FPS Counters
		private float frameTime;
		protected int fps;
		private int frameCounter;

		public BlackHolePhysicsController physicsController;

		public GameEnvironment(Controller ctrl)
				: base(ctrl) {

			Camera = new Camera2D(this);

			// Create a new SpriteBatch, which can be used to draw textures.
			m_spriteBatch = new SpriteBatch(ctrl.GraphicsDevice);

			CollisionWorld = new Physics.Dynamics.World(Vector2.Zero);

			m_debugView = new Physics.DebugViewXNA(CollisionWorld);
			m_debugView.AppendFlags(Physics.DebugViewFlags.DebugPanel);

			// Create collision notification callbacks.
			CollisionWorld.ContactManager.PreSolve += PreSolve;
			CollisionWorld.ContactManager.BeginContact += BeginContact;

			physicsController = new BlackHolePhysicsController(300.0f, 100.0f * k_physicsScale, 9.0f * k_physicsScale); // 300 controls how strong the pull is towards the black hole.
																									// 100.0 determines the radius fore which black hole will have an effect on.
			CollisionWorld.AddController(physicsController);

			// TODO: Make SpriteBatch drawing use this projection too.
			m_projection = Matrix.CreateOrthographicOffCenter(0.0f, ctrl.GraphicsDevice.Viewport.Width, ctrl.GraphicsDevice.Viewport.Height, 0.0f, -1.0f, 1.0f);
		}

		private enum Tile {
			None
			, AsteroidWall
			, WhiteBlock
			, GreyBlock
		};

		public void LoadMap(string filename) {
			m_map = Tiled.Map.Load(Path.Combine(Controller.Content.RootDirectory, filename), Controller.Content);
			
			// Destroy and re-create collision body for map.
			DestroyCollisionBody();
			CreateCollisionBody(CollisionWorld, Physics.Dynamics.BodyType.Static);

			Position = new Vector2(-Controller.GraphicsDevice.Viewport.Width + m_map.TileWidth, -Controller.GraphicsDevice.Viewport.Height + m_map.TileHeight);
			Vector2 tileHalfSize = new Vector2(m_map.TileWidth, m_map.TileHeight) / 2;

			foreach (KeyValuePair<string, Tiled.Layer> i in m_map.Layers) {
				Tiled.Layer layer = i.Value;

				for (int x = 0; x < layer.Width; ++x)
				for (int y = 0; y < layer.Height; ++y) {
					Tile tileType = (Tile) layer.GetTile(x, y);
					
					switch (tileType) {
						case Tile.AsteroidWall:
						case Tile.GreyBlock:
							// Create collision.
							AddCollisionRectangle(tileHalfSize, new Vector2(m_map.TileWidth * x, m_map.TileHeight * y) - tileHalfSize);
							break;
					}
				}
			}
		}

		public override void Update(float elapsedTime) {
			m_updateAccum += elapsedTime;

			// Update physics.
			const float k_physicsStep = 1.0f / 60.0f;
			while (m_updateAccum > k_physicsStep) {
				m_updateAccum -= k_physicsStep;
				CollisionWorld.Step(k_physicsStep);

				// Update entities.
				base.Update(k_physicsStep);
				Camera.Update(k_physicsStep);
			}

			//FPS counter
			frameCounter++;
			frameTime += elapsedTime;
			if (frameTime >= 1.0f)
			{
				fps = frameCounter;
				frameTime = 0;
				frameCounter = 0;
				Controller.Window.Title = "Sputnik (" + fps + " fps)";
			}
		}

		/// <summary>
		/// Draw the world.
		/// </summary>
		public override void Draw() {
			m_spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

			// Draw map.
			int offsetX = (int) (Camera.Position.X - Camera.Origin.X);
			int offsetY = (int) (Camera.Position.Y - Camera.Origin.Y);
			if (m_map != null) m_map.Draw(m_spriteBatch, new Rectangle(
					(int) -Camera.Origin.X
					, (int) -Camera.Origin.Y
					, Controller.GraphicsDevice.Viewport.Width + (int) Camera.Origin.X
					, Controller.GraphicsDevice.Viewport.Height + (int) Camera.Origin.Y
				), Camera.Position
			);
			m_spriteBatch.End();

			// Draw entities.
			m_spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, Camera.Transform);
			Draw(m_spriteBatch);
			m_spriteBatch.End();

			// Debug drawing.
			Matrix debugMatrix = Matrix.CreateScale(k_invPhysicsScale) * Camera.Transform;
			m_debugView.RenderDebugData(ref m_projection, ref debugMatrix);
		}

		public void BeginContact(Physics.Dynamics.Contacts.Contact contact) {
			// PreSolve performs the same function as this, only continue if one is a sensor.
			if (!contact.FixtureA.IsSensor && !contact.FixtureB.IsSensor) return;
			
			HandleContact(contact);
		}

		/// <summary>
		/// Farseer Physics callback.  Handles the case where two objects collide.
		/// </summary>
		/// <param name="contact">Contact point.</param>
		/// <param name="oldManifold">Manifold from last update.</param>
		protected void PreSolve(Physics.Dynamics.Contacts.Contact contact, ref Physics.Collision.Manifold oldManifold) {
			HandleContact(contact);
		}

		/// <summary>
		/// Handle contact interactions.
		/// </summary>
		/// <param name="contact">Contact point.</param>
		private void HandleContact(Physics.Dynamics.Contacts.Contact contact) {
			if (!contact.IsTouching()) return;

			// Attempt to get Entities from both shapes.
			Entity entA = (Entity) contact.FixtureA.Body.UserData;
			Entity entB = (Entity) contact.FixtureB.Body.UserData;

			// Determine if shapes agree to collide.
			bool shouldCollide = entA.ShouldCollide(entB);
			shouldCollide &= entB.ShouldCollide(entA);

			contact.Enabled = shouldCollide;

			if (shouldCollide) {
				entA.OnCollide(entB, contact);
				entB.OnCollide(entA, contact);
			}
		}

		/// <summary>
		/// Current FPS.
		/// </summary>
		public int FPS
		{
			get
			{
				return fps;
			}
		}
	}
}
