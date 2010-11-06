using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Sputnik {
	class OldKeyboard {
		static internal KeyboardState m_state;
		
		/// <summary>
		/// Get current state.
		/// </summary>
		/// <returns></returns>
		public static KeyboardState GetState() {
			return m_state;
		}
	}
}
