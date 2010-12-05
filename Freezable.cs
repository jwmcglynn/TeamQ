﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sputnik
{
	interface Freezable
	{
		void Freeze(GameEntity s);
		void Unfreeze();
	}
}
