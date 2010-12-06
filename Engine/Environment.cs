using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Sputnik {
	public abstract class Environment : Entity {
		public Controller Controller { get; private set; }

		public Environment(Controller ctrl) {
			Controller = ctrl;
		}

		public ContentManager contentManager {
			get {
				return Controller.Content;
			}
		}

		/// <summary>
		/// Draw the environment.
		/// </summary>
		public abstract void Draw();
	}
}
