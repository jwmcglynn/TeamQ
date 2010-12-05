using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Sputnik {
	class OldMouse {
		static internal MouseState m_state;

		/// <summary>
		/// Get current state.
		/// </summary>
		/// <returns></returns>
		public static MouseState GetState() {
			return m_state;
		}
	}
}
