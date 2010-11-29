using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sputnik
{
	interface Tractorable
	{
		void Tractored(Ship shipTractoring);
		void TractorReleased();
	}
}
