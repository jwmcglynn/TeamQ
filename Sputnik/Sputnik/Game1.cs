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

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Random r;
        List<Ship> aiShips;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 700;
            graphics.PreferredBackBufferWidth = 1100;

            /*if (!graphics.IsFullScreen)
                graphics.ToggleFullScreen();*/
             
            Content.RootDirectory = "Content";
            r = new Random();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            //(float)r.NextDouble() * 2 * MathHelper.Pi
            aiShips = new List<Ship>();
            aiShips.Add(new Ship(Content.Load<Texture2D>("Pawn"),
                    new Vector2(100, 100),
                    new Vector2(100, 100),
                    new Vector2(100, 400),
                    MathHelper.Pi/2));
            aiShips.Add(new Ship(Content.Load<Texture2D>("Pawn"),
                    new Vector2(100, 100),
                    new Vector2(100, 100),
                    new Vector2(400, 100),
                    0));
            aiShips.Add(new Ship(Content.Load<Texture2D>("Pawn"),
                    new Vector2(100, 100),
                    new Vector2(100, 100),
                    new Vector2(312, 312),
                    0));
            aiShips.Add(new Ship(Content.Load<Texture2D>("Pawn"),
                    new Vector2(233, 345),
                    new Vector2(385, 347),
                    new Vector2(153, 285),
                    0));
            for (int i = 0; i < 21; i++)
            {
                aiShips.Add(new Ship(Content.Load<Texture2D>("Pawn"),
                    new Vector2((float)r.NextDouble() * graphics.GraphicsDevice.Viewport.Width, (float)r.NextDouble() * graphics.GraphicsDevice.Viewport.Height),
                    new Vector2((float)r.NextDouble() * graphics.GraphicsDevice.Viewport.Width, (float)r.NextDouble() * graphics.GraphicsDevice.Viewport.Height),
                    new Vector2((float)r.NextDouble() * graphics.GraphicsDevice.Viewport.Width, (float)r.NextDouble() * graphics.GraphicsDevice.Viewport.Height),
                    (float)r.NextDouble() * 2 * MathHelper.Pi));
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Escape))
                this.Exit();

            // TODO: Add your update logic here
            foreach (Ship ship in aiShips)
            {
                AI.updateMe(ship);
                ship.update();
                //Done for my amusement
                if (ship.position.X > graphics.GraphicsDevice.Viewport.Width)
                    ship.position.X = -ship.texture.Width;
                else if (ship.position.X < -ship.texture.Width)
                    ship.position.X = graphics.GraphicsDevice.Viewport.Width;
                if (ship.position.Y > graphics.GraphicsDevice.Viewport.Height)
                    ship.position.Y = -ship.texture.Height;
                else if (ship.position.Y < -ship.texture.Height)
                    ship.position.Y = graphics.GraphicsDevice.Viewport.Height;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            foreach (Ship ship in aiShips)
            {
                spriteBatch.Draw(ship.texture, ship.position, null, Color.White, ship.theta, ship.center, 1.0f, SpriteEffects.None, 0f);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
