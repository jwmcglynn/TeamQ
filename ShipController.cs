﻿using System;
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
		void GotTractored(GameEntity s);
		void GotFrozen(GameEntity s);
		void DistressCall(Ship s, GameEntity f);
		bool IsAlliedWithPlayer();
		bool IsDisabled();
		void gotDetached();
		void Teleport(BlackHole blackhole, Vector2 destination);
	}
}
