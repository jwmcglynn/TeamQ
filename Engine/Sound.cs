using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Sputnik {
	class Sound {
		private static AudioEngine m_audioEngine;
		private static SoundBank m_soundBank;
		private static List<WaveBank> m_waveBank = new List<WaveBank>();

		public static AudioListener Listener = new AudioListener();

		internal static void Initialize() {
			m_audioEngine = new AudioEngine(@"Content\gameplay.xgs");

			// Load up the banks
			m_soundBank = new SoundBank(m_audioEngine, @"Content\Sound Bank.xsb");
			m_waveBank.Add(new WaveBank(m_audioEngine, @"Content\Music.xwb", 0, 16));
			m_waveBank.Add(new WaveBank(m_audioEngine, @"Content\SFX.xwb"));

			// Prime the audio engine for use
			m_audioEngine.Update();

			Listener.Forward = new Vector3(0.0f, 0.0f, -1.0f);
			Listener.Up = new Vector3(0.0f, 0.0f, 1.0f);
		}

		internal static void Update() {
			m_audioEngine.Update();
		}

		public static Vector2 CameraPos {
			get {
				return new Vector2(Listener.Position.X, Listener.Position.Y);
			}
			
			set {
				Listener.Position = new Vector3(value.X, value.Y, 50.0f);
			}
		}

		public static Cue Cue(string name) {
			return m_soundBank.GetCue(name);
		}

		public static void PlayCue(string name) {
			m_soundBank.PlayCue(name);
		}

		public static void PlayCue(string name, Entity source) {
			if (source.SoundEmitter == null) source.SoundEmitter = new AudioEmitter();
			source.SoundEmitter.Position = new Vector3(source.Position.X, source.Position.Y, 0.0f);

			m_soundBank.PlayCue(name, Listener, source.SoundEmitter);
		}
	}
}
