using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

// Comment out later
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class Ship : GameEntity, TakesDamage
	{
		protected ShipController ai;
		private ShipController previousAI = null;
		public int health = 10;
		private bool m_shouldCull = false;

		protected BulletEmitter shooter = null;

		public float maxSpeed = 100.0f;
		public float maxTurn = 0.025f;

		public Ship(GameEnvironment env, Vector2 pos)
				: base(env)
		{
			Position = pos;
		}

		public Ship(GameEnvironment env, SpawnPoint sp)
				: base(env, sp)
		{
		}

		public override void Update(float elapsedTime)
		{
			ai.Update(this, elapsedTime);

			if (!(this is SputnikShip))
			{
				// Update emitter position.
				shooter.Rotation = Rotation;
				shooter.Position = Position;
			}

			base.Update(elapsedTime);
		}

		// Attach Sputnik to the ship
		public virtual void Attach(SputnikShip sp)
		{
			this.previousAI = this.ai;
			this.ai = sp.GetAI();
		}

        public virtual void Detatch()
        {
            this.ai = this.previousAI;
        }

		public override bool ShouldCollide(Entity entB) {
			return !(entB is Ship) || (entB is SputnikShip) ;
		}

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if (entB is SputnikShip) ;
				contact.Enabled = false;
			base.OnCollide(entB, contact);
		}

		public override bool ShouldCull() {
			if (m_shouldCull) return true;
			return base.ShouldCull();
		}

		public virtual void TakeHit(int damage)
		{
			this.health -= damage;
			if(this.health < 1) {
				this.InstaKill();
			}
		}

		public virtual void Shoot(float elapsedTime)
		{
			shooter.Shoot(elapsedTime, IsFriendly());
		}

		public bool isSputnik()
		{
			if (this.ai is PlayerController)
				return true;
			return false;
		}

		public void InstaKill()
		{
			// Perform what ever actions are necessary to 
			// Destory a ship
			// TODO: Animations / explosions.
			this.health = 0;
			m_shouldCull = true;
		}

		public virtual bool IsFriendly()
		{
			return (this.ai is PlayerController);
		}
	}
}
