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
	class SquaretopiaShip : Ship, Freezable
	{
		public Entity shield;
		private float passiveShield;
		private bool destroyShield = false;
		
		public SquaretopiaShip(GameEnvironment env, Vector2 pos, SpawnPoint sp)
			: base(env, pos)
		{
			Initialize(sp);
			env.squares.Add(this);
		}

		private void Initialize(SpawnPoint sp) {
			shooter = new BulletEmitter(Environment, this,BulletEmitter.BulletStrength.Strong);
			AddChild(shooter);
			RelativeShooterPos = new Vector2(50.0f, 0.0f);

			ai = new AIController(sp, Environment);
			LoadTexture(Environment.contentManager, "squaretopia");

			Registration = new Vector2(100.0f, 125.0f);
			CreateCollisionBody(Environment.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(50.0f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f;
			this.health = this.MaxHealth = (int)(this.MaxHealth * 1.5);
			passiveShield = 20.0f;

			shield = new Entity();
			shield.Zindex = 0.0f;
			shield.Registration = new Vector2(125.0f, 115.0f);
			shield.LoadTexture(Environment.contentManager, "shield");
			shield.Position = Position;
			shield.Alpha = 0.0f;
			AddChild(shield);
		}

		public SquaretopiaShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;
			Initialize(sp);
			env.squares.Add(this);
		}

		public override void TakeHit(int damage)
		{
			if (passiveShield > 0)
			{
				passiveShield -= damage;
				shield.Alpha = 0.5f;
			}
			else
			{
				destroyShield = true;
				base.TakeHit(damage);
			}
		}

		public override void Update(float elapsedTime) {
			// Thruster particle.
			if (DesiredVelocity.LengthSquared() > (maxSpeed / 6) * (maxSpeed / 6)) {
				Matrix rotMatrix = Matrix.CreateRotationZ(Rotation);

				Environment.ThrusterEffect.Trigger(Position + Vector2.Transform(new Vector2(-75.0f, -30.0f), rotMatrix));
				Environment.ThrusterEffect.Trigger(Position + Vector2.Transform(new Vector2(-75.0f, 30.0f), rotMatrix));
			}

			// Update shield alpha/position.
			if (shield != null) {
				shield.Position = Position;

				if (shield.Alpha > 0.0f) {
					shield.Alpha -= 2.0f * elapsedTime;
					if (shield.Alpha < 0.0f) shield.Alpha = 0.0f;
				} else if (destroyShield) {
					shield.Dispose();
					shield = null;
				}
			}

			base.Update(elapsedTime);
		}

		public void Freeze(GameEntity s) {
			++m_frozenCount;
			if (m_frozenCount == 1) ai.GotFrozen(s);
			CollisionBody.AngularVelocity = 0.0f;
		}

		public void Unfreeze()
		{
			--m_frozenCount;
			if (m_frozenCount < 0) m_frozenCount = 0;
			Console.WriteLine("Frozen = " + m_frozenCount);
		}

		public override void OnCull()
		{
			Environment.squares.Remove(this);
			base.OnCull();
		}
		

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if (entB is Bullet && attachedShip == null)
			{
				//Horrible Casting makes me sad.
				foreach (SquaretopiaShip s in Environment.squares)
				{
					if(s!= this)
						s.ai.GotShotBy(this, (GameEntity)((Bullet)entB).owner);
				}
			}
			base.OnCollide(entB, contact);
		}
	}
}
