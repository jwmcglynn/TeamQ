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

        /// <summary>
        ///  Creates a new Player
        /// </summary>
        public PlayerController(GameEnvironment env)
        {
			m_env = env;
        }

		public void GotShotBy(Ship s, GameEntity f) 
		{
 			// Players dont do anything special when shot;
		}

		public void GotTractored()
		{
			//Chaos
		}

		public void GotFrozen()
		{
			//Chaos
		}

		public bool Turning()
		{
			return false;
			// Who cares
		}

		public void HitWall()
		{
			//Players dont care if they hit the wall
		}
        /// <summary>
        ///  Updates the State of a ship
        /// </summary>

        public void Update(Ship s, float elapsedTime)
        {
            Vector2 temp = Vector2.Zero;
            KeyboardState kb = Keyboard.GetState();
            MouseState ms = Mouse.GetState();
			Vector2 mousePos = m_env.Camera.ScreenToWorld(new Vector2(ms.X, ms.Y));
            s.shooterRotation = (float)Math.Atan2(mousePos.Y - s.Position.Y, mousePos.X - s.Position.X);
            bool directionChanged = false;

			lastSpace -= elapsedTime;
			if (lastSpace < 0)
				lastSpace = 0.0f;

			float scaleFactor = 2.0f;


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
			
            if (kb.IsKeyDown(Keys.W))
            {
                temp.Y = -1 * scaleFactor;
                directionChanged = true;
            }
            if (kb.IsKeyDown(Keys.A))
            {
				temp.X = -1 * scaleFactor;
                directionChanged = true;
            }
            if (kb.IsKeyDown(Keys.S))
            {
				temp.Y = 1 * scaleFactor;
                directionChanged = true;
            }
            if (kb.IsKeyDown(Keys.D))
            {
				temp.X = 1 * scaleFactor;
                directionChanged = true;
            }
            if (temp.X != 0 && temp.Y != 0)
                temp *= (float)Math.Sqrt(Math.Pow(s.maxSpeed, 2) / 2);
            else
                temp *= s.maxSpeed;
            s.DesiredVelocity = temp;
            if (directionChanged)
            {
                s.DesiredRotation = (float)Math.Atan2(s.DesiredVelocity.Y, s.DesiredVelocity.X);
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
							if (ent is Ship && ((Ship)ent).IsFriendly()) return false;
							return (ent is Tractorable);
						});

						if(collided is Tractorable) {
							((Tractorable)collided).Tractored(s); // Disable ship
							itemBeingTractored = collided;
							isTractoringItem = true;
						}
					} else {
						// Shoot the tractored item
						// After the ship has reached its destination, then it should proceed as normal.
						
						// add this code after collision with a wall?
						if(itemBeingTractored is Ship) {
							((Ship)itemBeingTractored).isTractored = false;
						}
						isTractoringItem = false;
					}
				} else if(s is SquaretopiaShip) {
					ForceField ff = new ForceField(m_env, s.Position, s.shooterRotation);
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
					if (ent is Ship && ((Ship) ent).IsFriendly()) return false;
					if (ent is Environment) return false;
					return true;
				});

				if (collided != null) collided.Dispose();
			}
		}
    }
}
