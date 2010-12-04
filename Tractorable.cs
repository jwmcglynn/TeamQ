using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	interface Tractorable
	{
		bool IsTractored { get; set; }

		void Tractored(Ship shipTractoring);
		void TractorReleased();

		void UpdateTractor(Vector2 targetPosition);
	}
}
