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
	class CircloidShip : Ship, Tractorable
	{
		private bool m_tractored;
		public bool IsTractored { get { return m_tractored; } set { m_tractored = value; } }

		public CircloidShip(GameEnvironment env, Vector2 pos, SpawnPoint sp) 
			: base(env, pos)
		{
			Initialize(sp);
			env.circles.Add(this);
		}

		private void Initialize(SpawnPoint sp) {
			shooter = new BulletEmitter(Environment, this, BulletEmitter.BulletStrength.Medium);
			AddChild(shooter);
			RelativeShooterPos = new Vector2(65.0f, -5.0f);

			ai = new AIController(sp, Environment);
			LoadTexture(Environment.contentManager, "circloid");

			Registration = new Vector2(117.0f, 101.0f);
			CreateCollisionBody(Environment.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(60.0f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f;
			CollisionBody.IgnoreGravity = true; // The circloid will not be affected by its own black hole. 
			CollisionBody.FixedRotation = true;
		}

		public CircloidShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;
			Initialize(sp); // FIXME: Find a better way to get positions.
			env.circles.Add(this);
		}

		public override void Update(float elapsedTime) {
			// Thruster particle.
			if (DesiredVelocity.LengthSquared() > (maxSpeed / 6) * (maxSpeed / 6)) {
				Matrix rotMatrix = Matrix.CreateRotationZ(Rotation);
				Environment.ThrusterEffect.Trigger(Position + Vector2.Transform(new Vector2(-90.0f, 0.0f), rotMatrix));
			}

			base.Update(elapsedTime);
		}

		public override bool ShouldCull() {
			if (IsTractored) return false;
			return base.ShouldCull();
		}

		public void Tractored(Ship shipTractoring)
		{
			tractoringShip = shipTractoring;
			IsTractored = true;
			ai.GotTractored();
		}

		public void TractorReleased()
		{
			IsTractored = false;
		}
		
		public void UpdateTractor(Vector2 position) {
			Position = position;
		}

		public override void OnCull()
		{
			IsTractored = false;
			Environment.circles.Remove(this);
			base.OnCull();
		}

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if (entB is Bullet && attachedShip == null)
			{
				//Horrible Casting makes me sad.
				foreach (CircloidShip c in Environment.circles)
					c.ai.GotShotBy(c, (GameEntity)((Bullet)entB).owner);
			}
			base.OnCollide(entB, contact);
		}
	}
}
