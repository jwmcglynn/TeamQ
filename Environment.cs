using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Sputnik {
	abstract class Environment : Entity {
		protected Controller m_controller;

		public Environment(Controller ctrl) {
			m_controller = ctrl;
		}

		public ContentManager contentManager {
			get {
				return m_controller.Content;
			}
		}

		/// <summary>
		/// Draw the environment.
		/// </summary>
		public abstract void Draw();
	}
}
