using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sputnik {
	interface TakesDamage {
		bool IsFriendly(Ship s);
		void TakeHit(int damage);
		void InstaKill();
	}
}
