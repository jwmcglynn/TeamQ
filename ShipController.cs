using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sputnik
{
	interface ShipController
	{
		bool Turning();
		void Update(Ship s, float elapsedTime);
		void GotShotBy(Ship s, GameEntity f);
		void HitWall();
	}
}
