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

		// Create force field dynamically.
		public ForceField(GameEnvironment env, Vector2 pos, float angle)
			: base(env)
		{
			Position = pos;
			m_angle = angle;
			Initialize();
		}

		private void Initialize()
		{
			LoadTexture(Environment.contentManager, "freeze/freeze10");
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
			if(entityCollidedWith != null && !hasFrozen) {
				CollisionBody.LinearVelocity = Vector2.Zero;
				Position = posOfCollision;
				hasFrozen = true;

				if (entityCollidedWith is Freezable)
				{
					((Freezable)entityCollidedWith).Freeze();
				}
			}
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
			if(entB is CircloidShip || entB is Boss || entB is Bullet) return false;
			if(entB is Ship && ((Ship)entB).IsFriendly()) {
				return false;
			}
	
			return true;
		}
	}
}
