using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectMercury;
using ProjectMercury.Modifiers;
using Microsoft.Xna.Framework;

[Serializable]
public sealed class TractorBeamModifier : Modifier {
	[NonSerialized]
	public static Vector2 Position = new Vector2(400.0f, 400.0f);

	/// <summary>
	/// Construct a modifier for use within the mercury editor.
	/// </summary>
	/// <param name="editorPosition"></param>
	public TractorBeamModifier() {
	}

	public override Modifier DeepCopy() {
		return new TractorBeamModifier();
	}

	protected override unsafe void Process(float elapsedSeconds, Particle * particle, int count) {
		// Apply a force towards the ship's position.
		float k_moveSpeed = 1000.0f * elapsedSeconds;
		float k_moveSpeedSq = k_moveSpeed * k_moveSpeed;

		// For each particle.
		for (Particle * end = (particle + count); particle != end; ++particle) {
			Vector2 offset = (Position - particle->Position);
			float len = offset.LengthSquared();

			if (len > k_moveSpeedSq) {
				offset = Vector2.Normalize(offset) * k_moveSpeed;
			}

			if (len < 900.0f) { // 30 pixels away.
				particle->Age = 1.0f;
				particle->Scale = 0.01f;
			} else {
				particle->Momentum = offset / elapsedSeconds;
				particle->Rotation = (float) Math.Atan2(offset.Y, offset.X) + MathHelper.PiOver2;
			}
		}
	}
}
