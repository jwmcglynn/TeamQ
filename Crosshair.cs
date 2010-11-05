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

namespace Sputnik
{
    class Crosshair : Entity
    {
		private GameEnvironment m_env;

        public Crosshair(GameEnvironment env)
        {
			m_env = env;
            LoadTexture(env.contentManager, "crosshair");
            Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
            Zindex = 0.0f;
        }


        public override void Update(float elapsedTime)
        {
            MouseState ms = Mouse.GetState();
            Position = m_env.Camera.ScreenToWorld(new Vector2(ms.X, ms.Y));
            base.Update(elapsedTime);
        }
    }
}
