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
    class GymEnvironment : GameEnvironment
    {

        BulletEmitter emit;
        Random r = new Random();

        public GymEnvironment(Controller ctrl)
            : base(ctrl)
        {

            Entity[] testBalls = new Entity[3];

            for (int i = 0; i < 3; ++i)
            {
                Entity test = new Entity();
                test.LoadTexture(contentManager, "redball");
                test.Registration = new Vector2(test.Texture.Width, test.Texture.Height) * 0.5f;

                test.CreateCollisionBody(CollisionWorld, BodyType.Dynamic, CollisionFlags.DisableSleep);
                test.AddCollisionCircle(test.Texture.Width * 0.5f, Vector2.Zero);
                AddChild(test);

                testBalls[i] = test;
            }

            testBalls[0].Position = new Vector2(50.0f, 200.0f);
            testBalls[0].SetPhysicsVelocityOnce(new Vector2(50.0f, 0.0f));

            testBalls[1].Position = new Vector2(200.0f, 200.0f);

            testBalls[2].Position = new Vector2(200.0f, 50.0f);
            testBalls[2].SetPhysicsVelocityOnce(new Vector2(0.0f, 75.0f));

            Bullet bulletTest = new Bullet(this, new Vector2(210.0f, 300.0f), 3.14 * 3 / 2, false);
            AddChild(bulletTest);

            SpecialAbility special = new SpecialAbility();
            special.LoadTexture(contentManager, "bullet");
            special.Position = new Vector2(300.0f, 100.0f);
            special.DesiredVelocity = new Vector2(0.0f, 0.0f);
            AddChild(special);

            emit = new BulletEmitter(this, BulletEmitter.BulletStrength.Weak, true);
            emit.Position = new Vector2(210.0f, 300.0f);
            emit.Rotation = (float)3.14 * 3 / 2;
            AddChild(emit);

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

            LoadMap("gym.tmx");
        }

        public override void Update(float elapsedTime)
        {
            KeyboardState kb = Keyboard.GetState();
            emit.IsShooting = kb.IsKeyDown(Keys.Space);

            const float k_cameraVel = 150.0f;
            if (kb.IsKeyDown(Keys.W)) m_viewportPosition.Y -= k_cameraVel * elapsedTime;
            if (kb.IsKeyDown(Keys.A)) m_viewportPosition.X -= k_cameraVel * elapsedTime;
            if (kb.IsKeyDown(Keys.S)) m_viewportPosition.Y += k_cameraVel * elapsedTime;
            if (kb.IsKeyDown(Keys.D)) m_viewportPosition.X += k_cameraVel * elapsedTime;

            base.Update(elapsedTime);
        }
    }
}

