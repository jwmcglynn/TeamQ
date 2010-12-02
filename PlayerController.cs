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
		private bool specialShot = true;
		private float lastSpace = 0.0f;
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

        public void Update(Ship s, float elapsedTime)
        {
			s.ResetMaxRotVel();

			controlled = s;
			KeyboardState kb = Keyboard.GetState();
			MouseState ms = Mouse.GetState();
			Vector2 mousePos = m_env.Camera.ScreenToWorld(new Vector2(ms.X, ms.Y));
			s.shooterRotation = Angle.Direction(s.Position, mousePos);

			lastSpace -= elapsedTime;
			if (lastSpace < 0)
				lastSpace = 0.0f;


			if (kb.IsKeyDown(Keys.Space) && !OldKeyboard.GetState().IsKeyDown(Keys.Space))
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

			Vector2 temp = Vector2.Zero;
			if (kb.IsKeyDown(Keys.W)) temp.Y -= 1.0f;
			if (kb.IsKeyDown(Keys.A)) temp.X -= 1.0f;
			if (kb.IsKeyDown(Keys.S)) temp.Y += 1.0f;
			if (kb.IsKeyDown(Keys.D)) temp.X += 1.0f;
			s.DesiredVelocity = (temp != Vector2.Zero) ? Vector2.Normalize(temp) * s.maxSpeed : Vector2.Zero;
			if (s.DesiredVelocity != Vector2.Zero) {
				s.DesiredRotation = Angle.Direction(Vector2.Zero, s.DesiredVelocity);
			}

			// need to check if sputnik is in a ship or not before you can shoot.
			if (ms.LeftButton == ButtonState.Pressed)
				s.Shoot(elapsedTime);

			// Will spawn a blackhole when we first pressdown our right mouse button.
			// if a blackhole has already been spawned this way, then the other one will be removed.
			if(ms.RightButton == ButtonState.Pressed && !specialShot) {
				if(s is CircloidShip) {
					if (m_playerBlackHoles != null) m_playerBlackHoles.Destroy();
					m_playerBlackHoles = BlackHole.CreatePair(m_env, m_env.Camera.ScreenToWorld(new Vector2(ms.X, ms.Y)));
				} else if(s is TriangulusShip) {
					
					// if we are tractoring something right now, then we arent allowed to tractor anything else
					// we can shoot now.
					if(!isTractoringItem) {
						List<Entity> list = VisionHelper.FindAll(m_env, s.Position, s.shooterRotation, MathHelper.ToRadians(20.0f), 500.0f);
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
					ForceField ff = new ForceField(m_env, s.Position, s.shooterRotation, controlled);
					m_env.AddChild(ff);
				}

				specialShot = true;
			}
			if(ms.RightButton == ButtonState.Released) {
				specialShot = false;
			}

			// Debug test for VisionHelper.
			if (kb.IsKeyDown(Keys.Q) && !OldKeyboard.GetState().IsKeyDown(Keys.Q)) {
				List<Entity> list = VisionHelper.FindAll(m_env, s.Position, s.shooterRotation, MathHelper.ToRadians(20.0f), 2000.0f);
				IOrderedEnumerable<Entity> sortedList = list.OrderBy(ent => Vector2.DistanceSquared(s.Position, ent.Position));

				Entity collided = sortedList.FirstOrDefault(ent => {
					if (ent is Ship && controlled.IsFriendly((Ship)ent)) return false;
					else if (ent is Boss && controlled.IsFriendly((Boss)ent)) return false;
					if (ent is Environment) return false;
					return true;
				});

				if (collided != null) collided.Dispose();
			}
		}
    }
}
