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
	class TriangulusShip : Ship, Tractorable, Freezable
	{
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
			CollisionBody.FixedRotation = true;
		}

		public TriangulusShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;
			Initialize(sp); // FIXME: Find a better way to get positions.
			env.triangles.Add(this);
		}

		public override void Update(float elapsedTime) {
			// Thruster particle.
			if (DesiredVelocity.LengthSquared() > (maxSpeed / 6) * (maxSpeed / 6)) {
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
				if(((Ship) entB).isTractored) return false;
			}
			return base.ShouldCollide(entB, fixture, entBFixture);
		}

		public void Freeze()
		{
			isFrozen = true;
			ai.GotFrozen();
		}

		public void Unfreeze()
		{
			isFrozen = false;
		}

		public void Tractored(Ship shipTractoring)
		{
			tractoringShip = shipTractoring;
			isTractored = true;
			ai.GotTractored();
		}

		public void TractorReleased() {
			isTractored = false;
		}

		public override void OnCull()
		{
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
					t.ai.GotShotBy(t, (GameEntity)((Bullet)entB).owner);
			}
			base.OnCollide(entB, contact);
		}
	}
}
