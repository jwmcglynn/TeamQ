using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Sputnik
{
	class PlayerController : ShipController
	{
		private GameEnvironment m_env;
		private const float timeBetweenControls = 0.0f;
		private BlackHole.Pair m_playerBlackHoles;
		private Tractorable itemBeingTractored;
		private Ship controlled;
		private static bool s_captureMouse = true;
		private bool m_justTeleported = false;
		private Cue m_tractorSound;

		const float k_speed = 600.0f; // pixels per second

		/// <summary>
		///  Creates a new Player
		/// </summary>
		public PlayerController(GameEnvironment env)
		{
			m_env = env;
		}

		public void GotShotBy(Ship s, GameEntity f) 
		{
			if (controlled is TriangulusShip)
			{
				foreach (TriangulusShip t in m_env.triangles)
				{
					t.ai.DistressCall(controlled, f);
				}
			}
			else if (controlled is SquaretopiaShip)
			{
				foreach (SquaretopiaShip sq in m_env.squares)
				{
					sq.ai.DistressCall(controlled, f);
				}
			}
			else if (controlled is CircloidShip)
			{
				foreach (CircloidShip c in m_env.circles)
				{
					c.ai.DistressCall(controlled, f);
				}
			}
		}

		public void GotTractored(GameEntity s)
		{
			//Chaos
		}

		public void GotFrozen(GameEntity s)
		{
			//Chaos
		}

		public void DistressCall(Ship s, GameEntity f)
		{
			//Player doesn't care about distress calls
		}

		public void HitWall(Vector2 collidePosition)
		{
			//Players dont care if they hit the wall
		}

		public bool IsAlliedWithPlayer()
		{
			//Of coruse the player is allied with the player
			return true;
		}

		public void gotDetached()
		{
			//Sputnik no care
		}

		/// <summary>
		/// Ship was just teleported through a blackhole.
		/// </summary>
		/// <param name="blackhole">Start blackhole.</param>
		/// <param name="destination">Position of destination.</param>
		public void Teleport(BlackHole blackhole, Vector2 destination) {
			m_justTeleported = true;
		}

		private void CancelTractorBeam() {
			// Release entity.
			itemBeingTractored.TractorReleased();
			itemBeingTractored = null;

			// Stop Sound.
			if (m_tractorSound != null) m_tractorSound.Stop(AudioStopOptions.AsAuthored);
			m_tractorSound = null;
		}

        /// <summary>
        ///  Updates the State of a ship
        /// </summary>

        public void Update(Ship s, float elapsedTime) {
			const float k_aimRadius = 250.0f;

			s.ResetMaxRotVel();
			controlled = s;

			GamePadState gamepad = GamePad.GetState(PlayerIndex.One);

			// Get keyboard state.
			Vector2 movement = Vector2.Zero;

			Vector2 aimPosition = Vector2.Zero;
			float aimDirection;

			Vector2 specialPosition = Vector2.Zero;
			float specialDirection;

			bool detachPressed = false;

			bool useSpecialPressed = false;
			bool useSpecialHeld = false;

			bool shoot = false;

			if (!gamepad.IsConnected) {
				KeyboardState keyboard = Keyboard.GetState();
				MouseState mouse = Mouse.GetState();
				MouseState oldMouse = OldMouse.GetState();

				if (keyboard.IsKeyDown(Keys.W)) movement.Y -= 1.0f;
				if (keyboard.IsKeyDown(Keys.A)) movement.X -= 1.0f;
				if (keyboard.IsKeyDown(Keys.S)) movement.Y += 1.0f;
				if (keyboard.IsKeyDown(Keys.D)) movement.X += 1.0f;

				// Aiming.
				Vector2 mousePos = m_env.Camera.ScreenToWorld(new Vector2(mouse.X, mouse.Y)) - m_env.Camera.Position;
				if (mousePos.Length() > k_aimRadius) {
					mousePos.Normalize();
					mousePos *= k_aimRadius;
				}

				specialPosition = Vector2.Normalize(mousePos) * k_aimRadius + s.Position;
				aimDirection = Angle.Direction(Vector2.Zero, mousePos);
				specialDirection = aimDirection;

				m_env.HUD.Rotation = specialDirection;
				m_env.HUD.Cursor.Position = mousePos / 3.0f + m_env.Camera.WorldToScreen(s.Position);
				m_env.HUD.Cursor.Visible = true;

				{
					// Reset mouse position to center.
					Vector2 screenCenter = m_env.Camera.WorldToScreen(m_env.Camera.Position + mousePos);
					if (s_captureMouse) Mouse.SetPosition((int) Math.Round(screenCenter.X), (int) Math.Round(screenCenter.Y));
					m_env.Controller.IsMouseVisible = !s_captureMouse;
				}

				// Detach.
				detachPressed = (keyboard.IsKeyDown(Keys.Space) && !OldKeyboard.GetState().IsKeyDown(Keys.Space));

				// Use special.
				useSpecialHeld = (mouse.RightButton == ButtonState.Pressed && oldMouse.RightButton == ButtonState.Pressed);
				useSpecialPressed = (mouse.RightButton == ButtonState.Pressed && oldMouse.RightButton != ButtonState.Pressed);

				// Shoot.
				shoot = (mouse.LeftButton == ButtonState.Pressed);

				// Debug. F3 toggles mouse capture.
				if (keyboard.IsKeyDown(Keys.F3) && !OldKeyboard.GetState().IsKeyDown(Keys.F3)) {
					s_captureMouse = !s_captureMouse;
				}

				if (keyboard.IsKeyDown(Keys.Q)) {
					m_env.ExplosionEffect.Trigger(s.Position);
				}

			} else {
				GamePadState oldGamepad = OldGamePad.GetState();
				Vector2 invertY = new Vector2(1.0f, -1.0f);

				// Gamepad.
				movement = gamepad.ThumbSticks.Left * invertY;
				aimDirection = Angle.Direction(Vector2.Zero, gamepad.ThumbSticks.Right * k_aimRadius * invertY);
				specialPosition = Angle.Vector(s.DesiredRotation) * k_aimRadius + s.Position;
				specialDirection = s.DesiredRotation;

				m_env.HUD.Rotation = aimDirection;
				m_env.HUD.Cursor.Position = Angle.Vector(aimDirection) * k_aimRadius / 3.0f + m_env.Camera.WorldToScreen(s.Position);

				// Detach.
				detachPressed = (gamepad.IsButtonDown(Buttons.B) && !oldGamepad.IsButtonDown(Buttons.B));

				// Use special.
				useSpecialHeld = (gamepad.IsButtonDown(Buttons.LeftShoulder) && oldGamepad.IsButtonDown(Buttons.LeftShoulder))
									|| (gamepad.IsButtonDown(Buttons.RightShoulder) && oldGamepad.IsButtonDown(Buttons.RightShoulder));
				useSpecialPressed = (gamepad.IsButtonDown(Buttons.LeftShoulder) && !oldGamepad.IsButtonDown(Buttons.LeftShoulder))
									|| (gamepad.IsButtonDown(Buttons.RightShoulder) && !oldGamepad.IsButtonDown(Buttons.RightShoulder));

				// Shoot.
				shoot = (gamepad.ThumbSticks.Right.Length() > 0.1f);
				m_env.HUD.Cursor.Visible = shoot;
			}

			// Act on input.
			s.shooterRotation = aimDirection;

			// Detach from ship.
			if (detachPressed)
			{
				s.Detach();
				// If i am tractoring something, and i detach from it, then they should go back to normal.
				if(itemBeingTractored != null) {
					CancelTractorBeam();
				}
			}

			// The item I am tractoring died.
			if (itemBeingTractored != null && !itemBeingTractored.IsTractored) {
				CancelTractorBeam();
			}

			// Update tractored entity's position.
			if (itemBeingTractored != null) {
				if (m_justTeleported) ((Entity) itemBeingTractored).Position = specialPosition;
				itemBeingTractored.UpdateTractor(specialPosition);
			}

			m_justTeleported = false;

			s.DesiredVelocity = (movement != Vector2.Zero) ? Vector2.Normalize(movement) * s.maxSpeed : Vector2.Zero;
			if (s.DesiredVelocity != Vector2.Zero) {
				s.DesiredRotation = Angle.Direction(Vector2.Zero, s.DesiredVelocity);
			}

			// need to check if sputnik is in a ship or not before you can shoot.
			if (shoot) s.Shoot(elapsedTime);

			// Will spawn a blackhole when we first pressdown our right mouse button.
			// if a blackhole has already been spawned this way, then the other one will be removed.
			if(useSpecialPressed) { // See useSpecialHeld for moving the blackhole.
				if(s is CircloidShip) {
					if (m_playerBlackHoles != null) m_playerBlackHoles.Destroy();
					m_playerBlackHoles = BlackHole.CreatePair(m_env, specialPosition);
				} else if(s is TriangulusShip) {
					
					// if we are tractoring something right now, then we arent allowed to tractor anything else
					// we can shoot now.
					if (itemBeingTractored == null) {
						List<Entity> list = VisionHelper.FindAll(m_env, s.Position, specialDirection, MathHelper.ToRadians(20.0f), 500.0f);
						IOrderedEnumerable<Entity> sortedList = list.OrderBy(ent => Vector2.DistanceSquared(s.Position, ent.Position)); 

						Entity collided = sortedList.FirstOrDefault(ent =>
						{
							if (ent is TakesDamage && ((TakesDamage) ent).IsFriendly()) return false;
							return (ent is Tractorable);
						});

						if(collided is Tractorable) {
							((Tractorable)collided).Tractored(s); // Disable ship
							itemBeingTractored = (Tractorable) collided;
							m_tractorSound = Sound.PlayCue("tractor_beam");
						}
					} else {
						CancelTractorBeam();
					}
				} else if(s is SquaretopiaShip) {
					ForceField ff = new ForceField(m_env, s.shooter.Position, specialDirection, controlled);
					m_env.AddChild(ff);
				}
			}
		}
    }
}
