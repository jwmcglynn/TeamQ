using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Sputnik
{
    class Ship : Entity, TakesDamage
    {
        private ShipController ai;
        private ShipController previousAI = null;
        private int health = 10;

        public float direction;
        public float maxSpeed;
        public float maxTurn;

        public Ship(AIController ai) : base() 
        {
            this.ai = ai;
            this.direction = 0.0f;
        }

        new public void Update(float elapsedTime)
        {
            ai.Update(this);
            base.Update(elapsedTime);
        }

        // Attach Sputnik to the ship
        public void Attach(SputnikShip sp)
        {
            this.previousAI = this.ai;
            this.ai = sp.GetAI();
        }

        // An Entity deals damage to the Ship.  Currently, only the 
        // damage from bullets is implemented.
        public void TakeHit(Entity attack)
        {
            if(attack is Bullet)
            {
                this.health -= ((Bullet)attack).getBulletStrength();
                if(this.health < 1)
                {
                    this.KillShip();
                }
            }
        }

        public void Shoot()
        {
            // Perform what ever actions are necessary to 
            // make the ship shoot
        }

        public bool isSputnik()
        {
            if (this.ai is PlayerController)
                return true;
            return false;
        }

        public void KillShip()
        {
            // Perform what ever actions are necessary to 
            // Destory a ship
        }

        public bool IsFriendly()
        {
            return false;
        }
    }
}
