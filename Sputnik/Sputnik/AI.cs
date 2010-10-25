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
    static class AI
    {
        static float maxSpeed = 4f;
        static float maxTurn = 0.1f;
        static public void updateMe(Ship ship)
        {
            
            Vector2 destination;
            if (ship.goingStart)
                destination = ship.start;
            else
                destination = ship.finish;
            float wantedDirection = (float)Math.Atan2(destination.Y-ship.position.Y,destination.X-ship.position.X);
            if (wantedDirection<0)
                wantedDirection += MathHelper.Pi *2f;
            if (Vector2.Distance(ship.position, destination) < 2f)
            {
                ship.goingStart = !ship.goingStart;
                ship.speed = 0f;
            }
            else if (wantedDirection == ship.theta)
            {
                //(Math.Abs(wantedDirection - ship.theta) <0.001)
                ship.speed = maxSpeed;
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
