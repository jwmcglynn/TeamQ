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

namespace Sputnik {
	class GameEnvironment : Environment {
		private SpriteBatch m_spriteBatch;
		private Tiled.Map m_map;
		protected Vector2 m_viewportPosition = Vector2.Zero;

		private Physics.DebugViewXNA m_debugView;
		public Physics.Dynamics.World CollisionWorld;
		protected Matrix m_debugPhysicsMatrix;

		public static float k_physicsScale = 1.0f / 50.0f; // 50 pixels = 1 meter.
		public static float k_invPhysicsScale = 50.0f; // ^ must be inverse.

		public float m_updateAccum; // How much time has passed relative to the physics world.

		//FPS Counters
		private float frameTime;
		protected int fps;
		private int frameCounter;


		public GameEnvironment(Controller ctrl)
				: base(ctrl) {

			//Frame rate variables initialize
			frameTime = 0;
			fps = 30;
			frameCounter = 0;

			// Create a new SpriteBatch, which can be used to draw textures.
			m_spriteBatch = new SpriteBatch(ctrl.GraphicsDevice);

			CollisionWorld = new Physics.Dynamics.World(Vector2.Zero);

			m_debugView = new Physics.DebugViewXNA(CollisionWorld);
			m_debugView.AppendFlags(Physics.DebugViewFlags.DebugPanel);

			// Create collision notification callbacks.
			CollisionWorld.ContactManager.PreSolve += PreSolve;

			// TODO: Scale to physics world.
			m_debugPhysicsMatrix = Matrix.CreateOrthographicOffCenter(0.0f, m_controller.GraphicsDevice.Viewport.Width * k_physicsScale, m_controller.GraphicsDevice.Viewport.Height * k_physicsScale, 0.0f, -1.0f, 1.0f);
		}

		public void LoadMap(string filename) {
			m_map = Tiled.Map.Load(Path.Combine(m_controller.Content.RootDirectory, filename), m_controller.Content);
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
			}

			//FPS counter
			frameCounter++;
			frameTime += elapsedTime;
			if (frameTime >= 1.0f)
			{
				fps = frameCounter;
				frameTime = 0;
				frameCounter = 0;

			}
		}

		/// <summary>
		/// Draw the world.
		/// </summary>
		public override void Draw() {
			m_spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
			if (m_map != null) m_map.Draw(m_spriteBatch, new Rectangle(0, 0, m_controller.GraphicsDevice.Viewport.Width, m_controller.GraphicsDevice.Viewport.Height), m_viewportPosition);
			Draw(m_spriteBatch);
			m_spriteBatch.End();
			m_debugView.RenderDebugData(ref m_debugPhysicsMatrix);
		}

		/// <summary>
		/// Farseer Physics callback.  Handles the case where two objects collide.
		/// </summary>
		/// <param name="contact">Contact point.</param>
		/// <param name="oldManifold">Manifold from last update.</param>
		protected void PreSolve(Physics.Dynamics.Contacts.Contact contact, ref Physics.Collision.Manifold oldManifold) {
			if (!contact.IsTouching()) return;

			// Attempt to get Entities from both shapes.
			Entity entA = (Entity) contact.FixtureA.Body.UserData;
			Entity entB = (Entity) contact.FixtureB.Body.UserData;

			// Determine if shapes agree to collide.
			bool shouldCollide = entA.ShouldCollide(entB);
			shouldCollide &= entB.ShouldCollide(entA);

			contact.Enabled = shouldCollide;

			if (shouldCollide && contact.IsTouching()) {
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
