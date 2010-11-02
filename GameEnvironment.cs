using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Physics = FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sputnik {
	class GameEnvironment : Environment {
		private SpriteBatch m_spriteBatch;

		private Physics.DebugViewXNA m_debugView;
		public Physics.Dynamics.World CollisionWorld;
		protected Matrix m_debugPhysicsMatrix;

		public static float k_physicsScale = 1.0f / 50.0f; // 50 pixels = 1 meter.
		public static float k_invPhysicsScale = 50.0f; // ^ must be inverse.

		public float m_updateAccum; // How much time has passed relative to the physics world.


		public GameEnvironment(Controller ctrl)
				: base(ctrl) {

			// Create a new SpriteBatch, which can be used to draw textures.
			m_spriteBatch = new SpriteBatch(ctrl.GraphicsDevice);

			CollisionWorld = new Physics.Dynamics.World(Vector2.Zero);

			m_debugView = new Physics.DebugViewXNA(CollisionWorld);
			m_debugView.AppendFlags(Physics.DebugViewFlags.DebugPanel);

			// Create collision notification callbacks.
			CollisionWorld.ContactManager.PreSolve += PreSolve;
			CollisionWorld.ContactManager.PostSolve += PostSolve;
			CollisionWorld.ContactManager.BeginContact += BeginContact;
			CollisionWorld.ContactManager.EndContact += EndContact;

			// TODO: Scale to physics world.
			m_debugPhysicsMatrix = Matrix.CreateOrthographicOffCenter(0.0f, m_controller.GraphicsDevice.Viewport.Width * k_physicsScale, m_controller.GraphicsDevice.Viewport.Height * k_physicsScale, 0.0f, -1.0f, 1.0f);
		}

		public override void Update(float elapsedTime) {
			m_updateAccum += elapsedTime;

			// Update physics.
			const float k_physicsStep = 1.0f / 60.0f;
			while (m_updateAccum > k_physicsStep) {
				m_updateAccum -= k_physicsStep;
				CollisionWorld.Step(k_physicsStep);

				// Update entities.
				base.Update(elapsedTime);
			}
		}

		public override void Draw() {
			m_spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
			Draw(m_spriteBatch);
			m_spriteBatch.End();
			m_debugView.RenderDebugData(ref m_debugPhysicsMatrix);
		}

		// Callbacks for derived classes.
		protected void BeginContact(Physics.Dynamics.Contacts.Contact contact) {
		}

		protected void EndContact(Physics.Dynamics.Contacts.Contact contact) {
		}

		protected void PreSolve(Physics.Dynamics.Contacts.Contact contact, ref Physics.Collision.Manifold oldManifold) {
		}

		protected void PostSolve(Physics.Dynamics.Contacts.Contact contact, ref Physics.Dynamics.ContactImpulse impulse) {
		}
	}
}
