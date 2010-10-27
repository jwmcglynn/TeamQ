using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sputnik {
	class GameEnvironment : Environment {
		private DebugViewXNA m_debugView;
		public World collisionWorld;
		protected Matrix m_debugPhysicsMatrix;
		public static float k_physicsScale = 1.0f / 50.0f; // 50 pixels = 1 meter.

		public float m_updateAccum; // How much time has passed relative to the physics world.


		public GameEnvironment(Controller ctrl)
				: base(ctrl) {
			collisionWorld = new World(Vector2.Zero);

			m_debugView = new DebugViewXNA(collisionWorld);
			m_debugView.AppendFlags(DebugViewFlags.DebugPanel);

			// Create collision notification callbacks.
			collisionWorld.ContactManager.PreSolve += PreSolve;
			collisionWorld.ContactManager.PostSolve += PostSolve;
			collisionWorld.ContactManager.BeginContact += BeginContact;
			collisionWorld.ContactManager.EndContact += EndContact;

			// TODO: Scale to physics world.
			m_debugPhysicsMatrix = Matrix.CreateOrthographicOffCenter(0.0f, m_controller.GraphicsDevice.Viewport.Width * k_physicsScale, m_controller.GraphicsDevice.Viewport.Height * k_physicsScale, 0.0f, -1.0f, 1.0f);
		}

		public override void Update(float elapsedTime) {
			m_updateAccum += elapsedTime;

			// Update physics.
			const float k_physicsStep = 1.0f / 60.0f;
			while (m_updateAccum > k_physicsStep) {
				m_updateAccum -= k_physicsStep;
				collisionWorld.Step(k_physicsStep);
			}

			// Update entities.
			base.Update(elapsedTime);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			m_debugView.RenderDebugData(ref m_debugPhysicsMatrix);
		}

		// Callbacks for derived classes.
		protected void BeginContact(Contact contact) {
		}

		protected void EndContact(Contact contact) {
		}

		protected void PreSolve(Contact contact, ref Manifold oldManifold) {
		}

		protected void PostSolve(Contact contact, ref ContactImpulse impulse) {
		}
	}
}
