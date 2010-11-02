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

        public State Update(State s)
        {
            KeyboardState kb = Keyboard.GetState();
            MouseState ms = Mouse.GetState();
            s.direction = (float)Math.Atan2(ms.Y - s.position.Y, ms.X - s.position.X);
            s.velocity = Vector2.Zero;
            if (kb.IsKeyDown(Keys.W))
            {
                s.velocity.Y = -s.maxSpeed;
            }
            if (kb.IsKeyDown(Keys.A))
            {
                s.velocity.X = -s.maxSpeed;
            }
            if (kb.IsKeyDown(Keys.S))
            {
                s.velocity.Y = s.maxSpeed;
            }
            if (kb.IsKeyDown(Keys.D))
            {
                s.velocity.X = s.maxSpeed;
            }
            return s;
        }
    }
}
