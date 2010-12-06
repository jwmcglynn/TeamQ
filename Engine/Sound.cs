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
			m_waveBank.Add(new WaveBank(m_audioEngine, @"Content\Looping SFX.xwb"));

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

		/// <summary>
		/// Play a sound.
		/// </summary>
		/// <param name="name"></param>
		public static Cue PlayCue(string name) {
			Cue sound = m_soundBank.GetCue(name);
			sound.Play();
			return sound;
		}

		/// <summary>
		/// Play a locational sound at the Entity's position.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="source"></param>
		public static Cue PlayCue(string name, Entity source) {
			Cue sound = m_soundBank.GetCue(name);
			if (source.SoundEmitter == null) source.SoundEmitter = new AudioEmitter();
			source.SoundEmitter.Position = new Vector3(source.Position.X, source.Position.Y, 0.0f);

			sound.Apply3D(Listener, source.SoundEmitter);
			sound.Play();
			return sound;
		}
	}
}
