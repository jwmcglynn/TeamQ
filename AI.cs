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

    class AI
    {
        Boolean goingStart;
        Vector2 start, finish;
        Environment env;

        /// <summary>
        ///  Creates a new AI with given start and finish positions of patrol path and given environment
        /// </summary>
        public AI(Vector2 s,Vector2 f, Environment e){
            start = s;
            finish = f;
            env = e;
        }

        /// <summary>
        ///  Updates the State of a ship
        /// </summary>

        public State update(State ship)
        {
            Vector2 destination;
            if (goingStart)
                destination = start;
            else
                destination = finish;
            float wantedDirection = (float)Math.Atan2(destination.Y - ship.position.Y, destination.X - ship.position.X);
            if (wantedDirection < 0)
                wantedDirection += MathHelper.Pi * 2f;
            if (Vector2.Distance(ship.position, destination) < ship.maxSpeed)
            {
                goingStart = !goingStart;
                ship.velocity = Vector2.Zero;
            }
            else if (wantedDirection == ship.direction)
            {
                //(Math.Abs(wantedDirection - ship.theta) <0.001)
                ship.velocity = new Vector2((float)(Math.Cos(ship.direction*ship.maxSpeed),(float)(Math.Sin(ship.direction*ship.maxSpeed));
            }
            else
            {
                //I gave up trying to find a good value for tweaking if wantedDirection and theta are equal
                //I hate floats now.
                ship.velocity = Vector2.Zero;
                if (ship.direction < wantedDirection)
                {
                    if (ship.direction + ship.maxTurn > wantedDirection)
                        ship.direction = wantedDirection;
                    else
                        ship.direction += ship.maxTurn;
                }
                else
                {
                    if (ship.direction - ship.maxTurn < wantedDirection)
                        ship.direction = wantedDirection;
                    else
                        ship.direction -= ship.maxTurn;
                }
            }
            return ship;
        }

    }
