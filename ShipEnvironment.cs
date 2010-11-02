using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;

namespace Sputnik
{
    class ShipEnvironment : GameEnvironment
    {
        Random r = new Random();

        public ShipEnvironment(Controller ctrl)
            : base(ctrl)
        {
            for (int i = 0; i < 20; i++)
            {
                AddChild(new TestShip((float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Width, (float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Height,
                    0, 0,
                    (float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Width, (float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Height,
                    (float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Width, (float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Height, 
                    this));
            }
            AddChild(new TestShip(150, 150, 0, 0, this));
            AddChild(new Crosshair(this));
        }

        public override void Update(float elapsedTime)
        {
            base.Update(elapsedTime);
        }
    }
}

