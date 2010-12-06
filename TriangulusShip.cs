using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common;
using FarseerPhysics.Collision.Shapes;

namespace Sputnik
{
	class TriangulusShip : Ship, Tractorable, Freezable {
		private bool m_tractored;
		public bool IsTractored { get { return m_tractored; } set { m_tractored = value; } }

		private Vector2 m_tractorTarget;
		private bool m_fling = false;
		private float m_flingTime = 0.0f;


		public TriangulusShip(GameEnvironment env, Vector2 pos, SpawnPoint sp)
			: base(env, pos)
		{
			Initialize(sp);
			env.triangles.Add(this);
		}

		private void Initialize(SpawnPoint sp) {
			shooter = new BulletEmitter(Environment, this,BulletEmitter.BulletStrength.Weak);
			AddChild(shooter);
			RelativeShooterPos = new Vector2(65.0f, 0.0f);

			ai = new AIController(sp, Environment);
			LoadTexture(Environment.contentManager, "triangulus");

			Registration = new Vector2(100.0f, 100.0f);
			CreateCollisionBody(Environment.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(40.0f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f;
			this.maxSpeed *= 1.5f;
			this.health = this.MaxHealth;
			AllowTeleport = true;
		}

		public TriangulusShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;
			Initialize(sp);
			env.triangles.Add(this);
		}

		public override void Update(float elapsedTime) {
			if (m_fling) {
				m_flingTime -= elapsedTime;
				if (m_flingTime < 0.0f) {
					m_fling = false;
					IsTractored = false;
					CollisionBody.LinearDamping = 8.0f;
				}

			} else if (IsTractored) {
				const float moveSpeed = 1000.0f;

				Vector2 dir = (m_tractorTarget - Position);
				float distance = dir.Length();

				if (distance < moveSpeed * elapsedTime) DesiredVelocity = dir * 60.0f;
				else DesiredVelocity = Vector2.Normalize(dir) * moveSpeed;
			}

			// Thruster particle.
			if (!IsTractored && DesiredVelocity.LengthSquared() > (maxSpeed / 6) * (maxSpeed / 6)) {
				Matrix rotMatrix = Matrix.CreateRotationZ(Rotation);

				Environment.ThrusterEffect.Trigger(Position + Vector2.Transform(new Vector2(-45.0f, -10.0f), rotMatrix));
				Environment.ThrusterEffect.Trigger(Position + Vector2.Transform(new Vector2(-45.0f, 10.0f), rotMatrix));
			}

			base.Update(elapsedTime);
		}

		public override bool ShouldCollide(Entity entB, Fixture fixture, Fixture entBFixture)
		{
			if (fixture.IsSensor && !(entB is Tractorable)) return false;
			if(entB is Tractorable && entB is Ship) {
				if (((Tractorable) entB).IsTractored) return false;
			}
			return base.ShouldCollide(entB, fixture, entBFixture);
		}

		public void Freeze(GameEntity s)
		{
			++m_frozenCount;
			if (m_frozenCount == 1) ai.GotFrozen(s);
			CollisionBody.AngularVelocity = 0.0f;
		}

		public void Unfreeze()
		{
			--m_frozenCount;
			if (m_frozenCount < 0) m_frozenCount = 0;
		}

		public void Tractored(Ship shipTractoring)
		{
			tractoringShip = shipTractoring;
			IsTractored = true;
			m_fling = false;
			ai.GotTractored(shipTractoring);

			// Give a random amount of angular impulse for cool unstable rotating!
			CollisionBody.ApplyAngularImpulse(CollisionBody.Mass * RandomUtil.NextFloat(-5.0f, 5.0f));
		}

		public void TractorReleased() {
			m_tractored = false;
			m_fling = true;
			m_flingTime = 1.0f;

			if (CollisionBody == null) return;
			CollisionBody.LinearDamping = 0.0f;
			CollisionBody.ApplyAngularImpulse(CollisionBody.Mass * RandomUtil.NextFloat(-5.0f, 5.0f));
		}

		public void UpdateTractor(Vector2 position) {
			m_tractorTarget = position;
		}

		public override void Teleport(BlackHole blackhole, Vector2 destination, Vector2 exitVelocity) {
			if (IsTractored && !m_fling) return;
			base.Teleport(blackhole, destination, exitVelocity);
		}

		public override bool ShouldCull() {
			if (IsTractored) return false;
			return base.ShouldCull();
		}

		public override void Dispose() {
			m_tractored = false;
			base.Dispose();
		}

		public override void OnCull() {
			Environment.triangles.Remove(this);
			base.OnCull();
		}

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if (contact.FixtureA.IsSensor && (entB is Tractorable))
			{

			}
			else if (entB is Bullet && attachedShip == null) //Don't make faction ships respond when shot if controlled
			{
				//Horrible Casting makes me sad.
				foreach (TriangulusShip t in Environment.triangles)
				{
					if(t!=this)
						t.ai.GotShotBy(this, (GameEntity)((Bullet)entB).owner);
				}
			}
			base.OnCollide(entB, contact);
		}
	}
}
