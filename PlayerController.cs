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
		
		BlackHole spawnedBlackHole;
		private bool specialShot = false;

        /// <summary>
        ///  Creates a new Player
        /// </summary>
        public PlayerController(GameEnvironment env)
        {
			m_env = env;
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
			s.Rotation = (float)Math.Atan2(mousePos.Y - s.Position.Y, mousePos.X - s.Position.X);
            if (kb.IsKeyDown(Keys.W))
            {
                temp.Y = -1;
            }
            if (kb.IsKeyDown(Keys.A))
            {
                temp.X = -1;
            }
            if (kb.IsKeyDown(Keys.S))
            {
                temp.Y = 1;
            }
            if (kb.IsKeyDown(Keys.D))
            {
                temp.X = 1;
            }
            if (temp.X != 0 && temp.Y != 0)
                temp *= (float)Math.Sqrt(Math.Pow(s.maxSpeed, 2) / 2);
            else
                temp *= s.maxSpeed;
            s.DesiredVelocity = temp;

            // need to check if sputnik is in a ship or not before you can shoot.
            if (ms.LeftButton == ButtonState.Pressed)
                s.Shoot(elapsedTime);

			// Will spawn a blackhole when we first pressdown our right mouse button.
			// if a blackhole has already been spawned this way, then the other one will be removed.
			if(ms.RightButton == ButtonState.Pressed && !specialShot) {
				if (spawnedBlackHole != null)
				{
					m_env.physicsController.RemoveBody(spawnedBlackHole.CollisionBody);
					spawnedBlackHole.Destroy();
				}

				spawnedBlackHole = new BlackHole(m_env);
				spawnedBlackHole.Position = m_env.Camera.ScreenToWorld(new Vector2(ms.X, ms.Y));
				m_env.AddChild(spawnedBlackHole);
				specialShot = true;
			}
			if(ms.RightButton == ButtonState.Released) {
				specialShot = false;
			}
		}
    }
}
