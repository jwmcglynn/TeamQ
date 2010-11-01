using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sputnik
{
    class SpecialAbility : Entity
    {
        private int typeOfAbility; // 1 = Black Hole, 2 = Force Field, 3 = Tractor Beam

        public SpecialAbility()
        {
        }
        
        public bool setAbilityType(int type) 
        {
            if (type == 1 || type == 2 || type == 3)
            {
                typeOfAbility = type;
                return true;
            }
            else
            {
                return false;
            }
        }

        // The behavior of our special ability will be handled here.
        public void useSpecial()
        {
            if(typeOfAbility == 1)
            {
                // Black Hole will be created in specified area; anything around it will be consumed.
            } 
            else if(typeOfAbility == 2)
            {
                // Force Field will shoot towards the pointed direction.
            } 
            else if(typeOfAbility == 3)
            {
                // Tractor Beam will grasp whatever is in its path (if object is tractorable). 
            }
        }


    }
}
