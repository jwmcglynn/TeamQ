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
		private bool isTractoringItem;
		private Entity itemBeingTractored;
		private Ship controlled;

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

		public void GotTractored()
		{
			//Chaos
		}

		public void GotFrozen()
		{
			//Chaos
		}

		public void DistressCall(Ship s, GameEntity f)
		{
			//Player doesn't care about distress calls
		}

		public void HitWall()
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

				// Aiming.  TODO: Direction, not position.
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
					Mouse.SetPosition((int) screenCenter.X, (int) screenCenter.Y);
				}

				// Detach.
				detachPressed = (keyboard.IsKeyDown(Keys.Space) && !OldKeyboard.GetState().IsKeyDown(Keys.Space));

				// Use special.
				useSpecialHeld = (mouse.RightButton == ButtonState.Pressed && oldMouse.RightButton == ButtonState.Pressed);
				useSpecialPressed = (mouse.RightButton == ButtonState.Pressed && oldMouse.RightButton != ButtonState.Pressed);

				// Shoot.
				shoot = (mouse.LeftButton == ButtonState.Pressed);

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
				detachPressed = (gamepad.IsButtonDown(Buttons.LeftShoulder) && !oldGamepad.IsButtonDown(Buttons.LeftShoulder))
									|| (gamepad.IsButtonDown(Buttons.RightShoulder) && !oldGamepad.IsButtonDown(Buttons.RightShoulder));

				// Use special.
				useSpecialHeld = (gamepad.IsButtonDown(Buttons.A) && oldGamepad.IsButtonDown(Buttons.A));
				useSpecialPressed = (gamepad.IsButtonDown(Buttons.A) && !oldGamepad.IsButtonDown(Buttons.A));

				// Shoot.
				shoot = (gamepad.ThumbSticks.Right.Length() > 0.1f);
				m_env.HUD.Cursor.Visible = shoot;
			}

			// Act on input.
			s.shooterRotation = aimDirection;

			if (detachPressed)
			{
				s.Detach();
				// If i am tractoring something, and i detach from it, then they should go back to normal.
				if(isTractoringItem) {
					if(itemBeingTractored is Ship) {
						((Ship) itemBeingTractored).isTractored = false;
						isTractoringItem = false;
					}
				}
			}

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
					if(!isTractoringItem) {
						List<Entity> list = VisionHelper.FindAll(m_env, s.Position, specialDirection, MathHelper.ToRadians(20.0f), 500.0f);
						IOrderedEnumerable<Entity> sortedList = list.OrderBy(ent => Vector2.DistanceSquared(s.Position, ent.Position)); 

						Entity collided = sortedList.FirstOrDefault(ent =>
						{
							if (ent is Ship && controlled.IsFriendly((Ship)ent)) return false;
							else if (ent is Boss && controlled.IsFriendly((Boss)ent)) return false;
							return (ent is Tractorable);
						});

						if(collided is Tractorable) {
							((Tractorable)collided).Tractored(s); // Disable ship
							itemBeingTractored = collided;
							isTractoringItem = true;

							if(itemBeingTractored is Asteroid) {
								((Asteroid)itemBeingTractored).CollisionBody.IsStatic = false;
							}
						}
					} else {
						if(itemBeingTractored.CollisionBody != null) { // case where what is being tractored dies before we can shoot it.
							itemBeingTractored.CollisionBody.LinearDamping = 0.0f;
							itemBeingTractored.SetPhysicsVelocityOnce(new Vector2(k_speed * (float)Math.Cos(s.shooterRotation), k_speed * (float)Math.Sin(s.shooterRotation)));
						
							// add this code after ship collides with a wall?
							if(itemBeingTractored is Ship) {
								((Ship)itemBeingTractored).isTractored = false;
							} else if(itemBeingTractored is Asteroid) {
								((Asteroid) itemBeingTractored).CollisionBody.IsStatic = true;
								((Asteroid)itemBeingTractored).TractorReleased();
							}
						}
						isTractoringItem = false;
					}
				} else if(s is SquaretopiaShip) {
					ForceField ff = new ForceField(m_env, s.Position, specialDirection, controlled);
					m_env.AddChild(ff);
				}
			}
		}
    }
}
