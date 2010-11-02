using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Sputnik
{
    class Ship : Entity
    {
        private ShipController ai = null;
        private ShipController previousAI = null;
        private int health = 10;

        public Ship() : base() { }

        new public void Update(float elapsedTime)
        {
            State current = new State();

            State state = ai.Update(current);

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
    }
}
