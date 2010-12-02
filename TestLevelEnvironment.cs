﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;

namespace Sputnik {
	class TestLevelEnvironment : GameEnvironment {
		public TestLevelEnvironment(Controller ctrl)
			: base(ctrl) {

			LoadMap("testlevel.tmx");
			Sound.PlayCue("music");
		}
	}
}