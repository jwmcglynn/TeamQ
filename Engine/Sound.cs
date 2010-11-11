using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace Sputnik {
	class Sound {
		private static AudioEngine m_audioEngine;
		private static SoundBank m_soundBank;
		private static List<WaveBank> m_waveBank = new List<WaveBank>();

		internal static void Initialize() {
			m_audioEngine = new AudioEngine(@"Content\gameplay.xgs");

			// Load up the banks
			m_soundBank = new SoundBank(m_audioEngine, @"Content\Sound Bank.xsb");
			m_waveBank.Add(new WaveBank(m_audioEngine, @"Content\Music.xwb", 0, 16));
			m_waveBank.Add(new WaveBank(m_audioEngine, @"Content\SFX.xwb"));

			// Prime the audio engine for use
			m_audioEngine.Update();
		}

		internal static void Update() {
			m_audioEngine.Update();
		}

		public static Cue Cue(string name) {
			return m_soundBank.GetCue(name);
		}

		public static void PlayCue(string name) {
			m_soundBank.PlayCue(name);
		}
	}
}
