using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

class GymEnvironment : Environment {
	public GymEnvironment(Controller ctrl)
			: base(ctrl) {
		Entity test = new Entity(this);
		test.LoadTexture("redball");
		test.velocity = new Vector2(5.0f, 5.0f);
	}
}
