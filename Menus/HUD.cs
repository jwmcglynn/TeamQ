using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik.Menus {
	public class HUD : Menu {
		public Widget Cursor;

		public HUD(Controller ctrl)
				: base(ctrl) {

			Cursor = new Widget(this);
			Cursor.LoadTexture(contentManager, "crosshair");
			Cursor.Registration = new Vector2(Cursor.Texture.Width, Cursor.Texture.Height) * 0.5f;
			AddChild(Cursor);
		}
	}
}
