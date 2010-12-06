using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sputnik
{
	class SputnikShip : Ship
	{
		public bool attached = false;
		public bool attaching = false;
		public Ship controlled = null; 
		private Ship recentlyControlled = null;
		private ShipController playerAI = null;
		
		private const float TotalTime = 3.0f;
		private float timer = TotalTime;

		private float m_respawnImmunity = 5.0f;
		private bool m_flashVisibility = true;

		private Texture2D m_awesomeSputnik;
		private Texture2D m_frostSputnik;

		float [] deathWarning = {0.1f, 0.2f, 0.3f, 0.4f, 0.6f, 0.8f, 1.1f, 1.5f};

		public float TimerPercent {
			get {
				return timer / TotalTime;
			}
		}

		public bool IsInvulnerable {
			get {
				return Environment.isFrostMode || m_respawnImmunity > 0.0f;
			}
		}

		public SputnikShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
					
			env.sputnik = this;
			Zindex = 0.25f;
			Position = sp.Position;
			
			this.maxSpeed = 800;

			m_awesomeSputnik = env.contentManager.Load<Texture2D>("Sputnik");
			m_frostSputnik = env.contentManager.Load<Texture2D>("girl_sputnik");
			Texture = (env.isFrostMode) ? m_frostSputnik : m_awesomeSputnik;

			Registration = new Vector2(70.0f, 33.0f);

			SputnikCreateCollision();
			ai = playerAI = new PlayerController(env);

			// Adjust camera.
			env.Camera.TeleportAndFocus(this);

			const float k_immuneTime = 3.0f;
			m_respawnImmunity = k_immuneTime;



			SpawnPoint.RespawnCooldown = 0.0f;
			SpawnPoint.AllowRespawn = true;
			SpawnPoint.HasBeenOffscreen = true;
		}

		public float Timer
		{
			get
			{
				return timer;
			}
		}

		private void SputnikCreateCollision() {
			CreateCollisionBody(Environment.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(20.0f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f; // This value causes a small amount of slowing before stop which looks nice.
		}

		public override void Update(float elapsedTime) {
			Texture = (Environment.isFrostMode) ? m_frostSputnik : m_awesomeSputnik;

			if ((controlled == null || controlled.health == 0) && attached)
			{
				Detach();
			}

			if (m_respawnImmunity > 0.0f) {
				float last = m_respawnImmunity;
				m_respawnImmunity -= elapsedTime;

				// Beep every second while invulnerable.
				if (Math.Floor(last) != Math.Floor(m_respawnImmunity)) {
					Sound.PlayCue("invulnerable_beep", this);
				}
			}

			// Thruster particle.
			if (!attached)
			{
				if (!this.Environment.isFrostMode && m_respawnImmunity <= 0.0f)
				{
					float previousTimer = timer;
					timer -= elapsedTime;

					foreach (float warning in deathWarning) {
						if (previousTimer > warning && timer < warning) {
							Sound.PlayCue("time_warning");
						}
					}

					if (timer < 0)
						InstaKill();
				}

				if (DesiredVelocity.LengthSquared() > (maxSpeed / 4) * (maxSpeed / 4))
				{
					ThrusterEffect().Trigger(Position + Angle.Vector(Rotation + MathHelper.Pi) * 20.0f);
				}
			}
			if (attached) {
				if(!attaching) {
					Rotation = controlled.Rotation;
					Position = controlled.Position;
				} else {
					if (Vector2.Distance(Position, controlled.Position) < 10.0f && Angle.DistanceMag(Rotation, controlled.Rotation) < 0.5f) {
						attaching = false;
						DesiredVelocity = Vector2.Zero;
					} else {
						Vector2 dir = (controlled.Position - Position);
						if (dir.Length() < maxSpeed * elapsedTime) DesiredVelocity = dir * 60.0f;
						else DesiredVelocity = Vector2.Normalize(dir) * maxSpeed;
						DesiredRotation = controlled.Rotation;
					}
				}
			}

			base.Update(elapsedTime);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			// Only draw every other frame when player is invulnerable to make player blink.
			if (m_respawnImmunity > 0.0f) {
				m_flashVisibility = !m_flashVisibility;
				if (m_flashVisibility) base.Draw(spriteBatch);
			} else {
				base.Draw(spriteBatch);
			}
		}

		public override void Dispose() {
			Environment.Camera.Focus = null;
			base.Dispose();
		}

		public override bool ShouldCull() {
			return false; // No, not Sputnik!  Don't cull him!
		}

		public override void Shoot(float elapsedTime)
		{
			// Do Nothing
		}

		public ShipController GetAI()
		{
			return this.ai;
		}

		public override void Attach(SputnikShip sp) {
			// Do nothing.
		}

		public void SputnikAttach(Ship target) {
			DestroyCollisionBody();

			attached = true;
			attaching = true;
			controlled = target;
			this.ai = null;

			Environment.AttachEffect.Trigger(target.Position);

			m_respawnImmunity = 0.0f;

			Sound.PlayCue("attach_success");
		}

		public override void Detach()
		{
			if (!attached) return;

			timer = TotalTime;
			recentlyControlled = controlled;
			attached = false;
			controlled = null;
			ai = playerAI;

			SputnikCreateCollision();

			Sound.PlayCue("detach");
		}

		public override void OnSeparate(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if (entB == recentlyControlled && !attached)
				recentlyControlled = null;
			base.OnSeparate(entB, contact);
		}

		public override bool ShouldCollide(Entity entB, FarseerPhysics.Dynamics.Fixture fixture, FarseerPhysics.Dynamics.Fixture entBFixture) {
			return !(entB is Environment) && !(attached && entB is Bullet);
		}

		public override void TakeHit(int damage) {
			// Disabling for now, it seems cruel by causing near-instant death from boss.
			// timer -= (TotalTime / 0.2f);
		}

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			contact.Enabled = false;
			if (entB is Ship && !attached && entB != recentlyControlled)
			{
				OnNextUpdate += () => ((Ship) entB).Attach(this);
			}
			base.OnCollide(entB, contact);
		}
	}
}
