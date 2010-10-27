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

namespace TeamQ
{

    class AI
    {
        float maxSpeed = 5f;
        float maxTurn = 0.1f;
        bool goingStart;
        Vector2 start, finish;
        Environment environment;
        
        public AI(Vector2 s, Vector2 f, Environment e){
            start = s;
            finish = f;
            environment = e;
        }

        public State update(State s)
        {

            Vector2 destination;
            if (goingStart)
                destination = start;
            else
                destination = finish;
            float wantedDirection = (float)Math.Atan2(destination.Y - s.position.Y, destination.X - s.position.X);
            if (wantedDirection < 0)
                wantedDirection += 2 * MathHelper.Pi;
            float currentDirection = s.direction % (MathHelper.Pi * 2);
            if (Vector2.Distance(s.position, destination) < maxSpeed)
            {
                s.velocity = Vector2.Subtract(destination,s.position);
            }
            else if (Vector2.Distance(s.position, destination) == 0)
            {
                goingStart = !goingStart;
                s.velocity = Vector2.Zero;
            }
            else if (Math.Abs(currentDirection - wantedDirection) < maxTurn)
            {
                //(Math.Abs(wantedDirection - ship.theta) <0.001)
                s.velocity = new Vector2((float)(maxSpeed*Math.Cos(currentDirection)),(float)(maxSpeed*Math.Sin(currentDirection)));
            }
            else
            {
                //I gave up trying to find a good value for tweaking if wantedDirection and theta are equal
                //I hate floats now.
                s.velocity = Vector2.Zero;
               if (currentDirection < wantedDirection)
                {
                    if (s.direction + maxTurn > wantedDirection)
                        s.direction = wantedDirection;
                    else
                        s.direction += maxTurn;
                }
                else
                {
                    if (s.direction - maxTurn < wantedDirection)
                        s.direction = wantedDirection;
                    else
                        s.direction -= maxTurn;
                }
            }
            return s;
        }

    }
}
