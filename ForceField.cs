using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Sputnik
{
	class ForceField : GameEntity
	{
		const float k_speed = 600.0f; // pixels per second
		float m_angle;
		float timeElapsed;
		TakesDamage owner;
		private Texture2D[] m_textures = new Texture2D[10];

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
			// Load textures.
			for (int i = 0; i < m_textures.Length; i++)
			{
				String assetName = "freeze/freeze" + (i + 1);
				m_textures[i] = Environment.contentManager.Load<Texture2D>(assetName);
			}

			Texture = m_textures[0];
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
		private float timeForAnimation = 1.0f;
		private int numberOfFrames = 10;

		public override void Update(float elapsedTime)
		{
			if (timeElapsed > timeForAnimation)
			{
				Texture = m_textures[m_textures.Length - 1];
			} else {
				Texture = m_textures[(int)(timeElapsed / timeForAnimation * numberOfFrames)];
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
			if (entB is SputnikShip)
				return false;
			else if (entB is Boss && (owner.IsFriendly((Boss)entB)))
			{
				return false;
			}
	
			return true;
		}
	}
}
