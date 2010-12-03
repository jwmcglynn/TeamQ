﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;

namespace Sputnik {
	class Level1Environment : GameEnvironment {
		public Level1Environment(Controller ctrl)
			: base(ctrl) {

			LoadMap("Level_1.tmx");
			Sound.PlayCue("music");
		}
	}
}