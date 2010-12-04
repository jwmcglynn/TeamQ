using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sputnik {
	interface TakesDamage {
		void TakeHit(int damage);
		void InstaKill();

		bool IsFriendly();
		bool IsAllied(TakesDamage other);
	}
}
