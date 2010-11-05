﻿using System;
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

        public void Update(Ship s, float elapsedTime)
        {
            Vector2 temp = Vector2.Zero;
            KeyboardState kb = Keyboard.GetState();
            MouseState ms = Mouse.GetState();
            s.Rotation = (float)Math.Atan2(ms.Y - s.Position.Y, ms.X - s.Position.X);
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

        }
    }
}
