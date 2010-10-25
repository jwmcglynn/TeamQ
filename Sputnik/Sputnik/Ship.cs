using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace Sputnik
{
    class Ship
    {
        public Texture2D texture;
        public Vector2 position,center,start,finish;
        public float theta;
        public float speed;
        public bool goingStart;

        public Ship(Texture2D t, Vector2 p, Vector2 s, Vector2 f, float th)
        {
            texture = t;
            position = p;
            center = new Vector2(texture.Width / 2, texture.Height / 2);
            start = s;
            finish = f;
            theta = th;
            goingStart = true;
            speed = 0;
        }

        public void update()
        {
            position.X = (float)(System.Math.Cos(theta) * speed + position.X);
            position.Y = (float)(System.Math.Sin(theta) * speed + position.Y);
            
        }

        public float X()
        {
            return position.X;
        }

        public float Y()
        {
            return position.Y;
        }

        public float Sx()
        {
            return start.X;
        }

        public float Sy()
        {
            return start.Y;
        }

        public float Fx()
        {
            return finish.X;
        }

        public float Fy()
        {
            return finish.Y;
        }
    }
}
