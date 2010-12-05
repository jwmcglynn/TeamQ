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
		GameEntity owner;
		private Texture2D[] m_textures = new Texture2D[10];
		private List<Freezable> m_frozen = new List<Freezable>();

		// Create force field dynamically.
		public ForceField(GameEnvironment env, Vector2 pos, float angle, GameEntity o)
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
			Zindex = 0.4f;

			CreateCollisionBody(Environment.CollisionWorld, FarseerPhysics.Dynamics.BodyType.Dynamic, CollisionFlags.Default);
			var circle = AddCollisionCircle(Texture.Height / 3, Vector2.Zero);
		
			CollisionBody.LinearDamping = 0.0f;
			SetPhysicsVelocityOnce(new Vector2(k_speed * (float)Math.Cos(m_angle), k_speed * (float)Math.Sin(m_angle)));
		}

		private bool hasFrozen;
		private float timeForAnimation = 1.0f;
		private int numberOfFrames = 10;
		private float frozenDuration = 0.0f;

		public override void Update(float elapsedTime)
		{
			if (timeElapsed > timeForAnimation)
			{
				Texture = m_textures[m_textures.Length - 1];
			} else {
				Texture = m_textures[(int)(timeElapsed / timeForAnimation * numberOfFrames)];
			}

			timeElapsed += elapsedTime;

			if (hasFrozen) {
				frozenDuration += elapsedTime;
				if (frozenDuration > 5.0f) {
					// Fade out alpha after five seconds.
					if (frozenDuration >= 6.0f) {
						// Unfreeze after six seconds.
						foreach (Freezable f in m_frozen) {
							f.Unfreeze();
						}

						Dispose();
					} else {
						// Alpha will be 1.0f at five seconds, 0.0f at six seconds.
						Alpha = 6.0f - frozenDuration;
					}
				}
			}
			base.Update(elapsedTime);
		}
		
		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact) {
			contact.Enabled = false;

			// Don't collide if the entity is the owner.
			// Don't collide if the entity is not freezable, but still allow collisions with the environment and other
			// force fields if we are not frozen.
			if (entB == owner || !(entB is Freezable || (!hasFrozen && (entB is Environment || entB is ForceField)))) {
				return;
			}


			FarseerPhysics.Collision.WorldManifold manifold;
			contact.GetWorldManifold(out manifold);
			Vector2 posOfCollision = manifold.Points[0]*GameEnvironment.k_invPhysicsScale;


			if (entB is Freezable) {
				if (m_frozen.Contains((Freezable) entB)) {
					return;
				} else {
					m_frozen.Add((Freezable) entB);
				}
			}

			OnNextUpdate += () => {
				if (!hasFrozen) {
					hasFrozen = true;
					CollisionBody.LinearVelocity = Vector2.Zero;
					CollisionBody.IsStatic = true;
					Position = posOfCollision;
				}

				if (entB is Freezable) {
					Sound.PlayCue("freeze_zap", entB);
					((Freezable) entB).Freeze(owner);
					entB.Zindex = Zindex + RandomUtil.NextFloat(0.001f, 0.009f);
				}
			};
		}
	
		// Collide with non-circloid bullets and non-circloid ships.
		public override bool ShouldCollide(Entity entB, FarseerPhysics.Dynamics.Fixture fixture, FarseerPhysics.Dynamics.Fixture entBFixture) {
			if (entB is CircloidShip || entB is Boss || entB is Bullet || entB is SputnikShip) return false;

			if (entB is TakesDamage && ((TakesDamage) entB).IsFriendly()) {
				return false;
			}
	
			return true;
		}
	}
}
