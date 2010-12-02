using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class ForceField : GameEntity
	{
		const float k_speed = 600.0f; // pixels per second
		float m_angle;
		float timeElapsed;
		TakesDamage owner;

		// Create force field dynamically.
		public ForceField(GameEnvironment env, Vector2 pos, float angle, TakesDamage o)
			: base(env)
		{
			Position = pos;
			m_angle = angle;
			Initialize();
			owner = o;
		}

		private void Initialize()
		{
			LoadTexture(Environment.contentManager, "freeze/freeze1");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			Zindex = 0.0f;

			CreateCollisionBody(Environment.CollisionWorld, FarseerPhysics.Dynamics.BodyType.Dynamic, CollisionFlags.Default);
			var circle = AddCollisionCircle(Texture.Height / 3, Vector2.Zero);
		
			CollisionBody.LinearDamping = 0.0f;
			SetPhysicsVelocityOnce(new Vector2(k_speed * (float)Math.Cos(m_angle), k_speed * (float)Math.Sin(m_angle)));
			//DesiredVelocity = new Vector2(k_speed * (float)Math.Cos(m_angle), k_speed * (float)Math.Sin(m_angle));
		}

		private Entity entityCollidedWith = null;
		private Vector2 posOfCollision;
		private bool hasFrozen;

		public override void Update(float elapsedTime)
		{
			if(timeElapsed > 0.9 ) {
				LoadTexture(Environment.contentManager, "freeze/freeze10");
			} else if(timeElapsed > 0.8) {
				LoadTexture(Environment.contentManager, "freeze/freeze9");
			} else if (timeElapsed > 0.7) {
				LoadTexture(Environment.contentManager, "freeze/freeze8");
			} else if (timeElapsed > 0.6) {
				LoadTexture(Environment.contentManager, "freeze/freeze7");
			} else if (timeElapsed > 0.5) {
				LoadTexture(Environment.contentManager, "freeze/freeze6");
			} else if (timeElapsed > 0.4) {
				LoadTexture(Environment.contentManager, "freeze/freeze5");
			} else if (timeElapsed > 0.3) {
				LoadTexture(Environment.contentManager, "freeze/freeze4");
			} else if (timeElapsed > 0.2) {
				LoadTexture(Environment.contentManager, "freeze/freeze3");
			} else if (timeElapsed > 0.1) {
				LoadTexture(Environment.contentManager, "freeze/freeze2");
			}

			if(entityCollidedWith != null && !hasFrozen) {
				CollisionBody.LinearVelocity = Vector2.Zero;
				Position = posOfCollision;
				hasFrozen = true;

				if (entityCollidedWith is Freezable)
				{
					((Freezable)entityCollidedWith).Freeze();
				}
			}
			timeElapsed += elapsedTime;

			base.Update(elapsedTime);
		}
		
		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact) {
			contact.Enabled = false;
			entityCollidedWith = entB;			
			FarseerPhysics.Collision.WorldManifold manifold;
			contact.GetWorldManifold(out manifold);
			posOfCollision = manifold.Points[0]*GameEnvironment.k_invPhysicsScale;
		} 
	
		// Collide with non-circloid bullets and non-circloid ships.
		public override bool ShouldCollide(Entity entB, FarseerPhysics.Dynamics.Fixture fixture, FarseerPhysics.Dynamics.Fixture entBFixture) {
			if(entB is CircloidShip || entB is Boss || entB is Bullet || entB is SputnikShip) return false;
			if(entB is Ship && (owner.IsFriendly((Ship)entB))) {
				return false;
			}
	
			return true;
		}
	}
}
