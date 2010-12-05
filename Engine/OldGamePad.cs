using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Sputnik {
	class OldGamePad {
		static internal GamePadState m_state;

		/// <summary>
		/// Get current state.
		/// </summary>
		/// <returns></returns>
		public static GamePadState GetState() {
			return m_state;
		}
	}
}
