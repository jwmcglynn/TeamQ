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
	class Ship : Entity, TakesDamage
	{
		protected ShipController ai;
		private ShipController previousAI = null;
		private int health = 10;
		private bool m_shouldCull = false;

		protected BulletEmitter shooter = null;

		public float maxSpeed;
		public float maxTurn;

		public Ship(float x, float y, float vx, float vy, float sx, float sy, float fx, float fy, GameEnvironment env) : base() 
		{
			Position = new Vector2(x, y);
			DesiredVelocity = new Vector2(vx, vy);

			this.maxSpeed = 100.0f;
			this.maxTurn = 0.025f;
		}

		public override void Update(float elapsedTime)
		{
			if(ai != null) 
			{
				 ai.Update(this, elapsedTime);
			}

			if (m_shouldCull) Destroy();

			//ai.Update(this, elapsedTime);

			// Update emitter position.
			shooter.Rotation = Rotation;
			shooter.Position = Position;

			base.Update(elapsedTime);
		}

		// Attach Sputnik to the ship
		public void Attach(SputnikShip sp)
		{
			this.previousAI = this.ai;
			this.ai = sp.GetAI();
		}

		public override bool ShouldCollide(Entity entB) {
			return !(entB is Ship);
		}

		// An Entity deals damage to the Ship.  Currently, only the 
		// damage from bullets is implemented.
		public virtual void TakeHit(int damage)
		{
			this.health -= damage;
			if(this.health < 1) {
				this.InstaKill();
			}
		}

		public void Shoot(float elapsedTime)
		{
			shooter.Shoot(elapsedTime);
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
			m_shouldCull = true;
		}

		public virtual bool IsFriendly()
		{
			return false;
		}
	}
}
