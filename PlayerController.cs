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
        /// <summary>
        ///  Creates a new Player
        /// </summary>
        public PlayerController(Environment env)
        {

        }

        /// <summary>
        ///  Updates the State of a ship
        /// </summary>

        public void Update(Ship s)
        {
            Vector2 temp = Vector2.Zero;
            KeyboardState kb = Keyboard.GetState();
            MouseState ms = Mouse.GetState();
            //s.direction = (float)Math.Atan2(ms.Y - s.Position.Y, ms.X - s.Position.X);
            //s.velocity = Vector2.Zero;
            if (kb.IsKeyDown(Keys.W))
            {
                temp.Y = -s.maxSpeed;
            }
            if (kb.IsKeyDown(Keys.A))
            {
                temp.X = -s.maxSpeed;
            }
            if (kb.IsKeyDown(Keys.S))
            {
                temp.Y = s.maxSpeed;
            }
            if (kb.IsKeyDown(Keys.D))
            {
                temp.X = s.maxSpeed;
            }
            s.DesiredVelocity = temp;
        }
    }
}
