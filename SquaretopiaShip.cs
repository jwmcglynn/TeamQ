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
			this.health = (int)(this.health * 1.5);
			passiveShield = 20.0f;
		}

		public SquaretopiaShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;
			Initialize(sp); // FIXME: Find a better way to get positions.
			env.squares.Add(this);
		}

		public override void Update(float elapsedTime) {
			// Thruster particle.
			if (DesiredVelocity.LengthSquared() > (maxSpeed / 6) * (maxSpeed / 6)) {
				Matrix rotMatrix = Matrix.CreateRotationZ(Rotation);

				Environment.ThrusterEffect.Trigger(Position + Vector2.Transform(new Vector2(-75.0f, -30.0f), rotMatrix));
				Environment.ThrusterEffect.Trigger(Position + Vector2.Transform(new Vector2(-75.0f, 30.0f), rotMatrix));
			}

			base.Update(elapsedTime);
		}

		public void Freeze(GameEntity s) {
			isFrozen = true;
			ai.GotFrozen(s);
		}

		public void Unfreeze()
		{
			isFrozen = false;
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
