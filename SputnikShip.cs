using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sputnik
{
    class SputnikShip : Entity
    {

        public ShipController ai = null;

        public SputnikShip() : base() { }

        new public void Update(float elapsedTime)
        {
            base.Update(elapsedTime);
        }

        public ShipController GetAI()
        {
            return this.ai;
        }
    }
}
