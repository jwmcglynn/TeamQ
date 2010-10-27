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
        Vector2 start, finish
        
        AI(Vector2 s, Vector2 f, Environment e){
        }

        public void updateMe(State ship)
        {

            Vector2 destination;
            if (goingStart)
                destination = start;
            else
                destination = finish;
            float wantedDirection = (float)Math.Atan2(destination.Y - ship.position.Y, destination.X - ship.position.X);
            if (wantedDirection < 0)
                wantedDirection += MathHelper.Pi * 2f;
            if (Vector2.Distance(ship.position, destination) < maxSpeed)
            {
                goingStart = !goingStart;
                ship.velocity = Vector2.Zero;
            }
            else if (wantedDirection == ship.rotation)
            {
                //(Math.Abs(wantedDirection - ship.theta) <0.001)
                ship.velocity = new Vector2();
            }
            else
            {
                //I gave up trying to find a good value for tweaking if wantedDirection and theta are equal
                //I hate floats now.
                ship.speed = 0f;
                if (ship.theta < wantedDirection)
                {
                    if (ship.theta + maxTurn > wantedDirection)
                        ship.theta = wantedDirection;
                    else
                        ship.theta += maxTurn;
                }
                else
                {
                    if (ship.theta - maxTurn < wantedDirection)
                        ship.theta = wantedDirection;
                    else
                        ship.theta -= maxTurn;
                }
            }
        }

    }
}
