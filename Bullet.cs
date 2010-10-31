using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sputnik
{
    class Bullet : Entity
    {
        private int bulletStrength;

        public Bullet(Environment env) : base(env)
        {
        }

        // 1 is for weak bullet, 3 is for strong bullet.
        public bool setBulletStrength(int strength) 
        {
            if (strength == 1 || strength == 2 || strength == 3)
            {
                bulletStrength = strength;
                return true;
            }
            else
            {
                return false;
            }
        }

        public int getBulletStrength()
        {
            return bulletStrength;
        }
    }
}
