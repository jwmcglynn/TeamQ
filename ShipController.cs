using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	interface ShipController
	{
		void Update(Ship s, float elapsedTime);
		void GotShotBy(Ship s, GameEntity f);
		void HitWall(Vector2 collidePosition);
		void GotTractored();
		void GotFrozen();
		void DistressCall(Ship s, GameEntity f);
		bool IsAlliedWithPlayer();
		void gotDetached();
	}
}
