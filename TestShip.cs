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
    //Made to make 
    class TestShip : Entity
    {
        ShipController controller;
        public TestShip(float x, float y,float vx, float vy, float sx, float sy, float fx, float fy, GameEnvironment env)
        {
            LoadTexture(env.contentManager, "circloid");
            Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
            CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.DisableSleep);
            AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
            Position = new Vector2(x, y);
            SetPhysicsVelocityOnce(new Vector2(vx, vy));
            controller = new AIController(new Vector2(sx, sy), new Vector2(fx, fy), env);              
        }

        public TestShip(float x, float y, float vx, float vy,GameEnvironment env)
        {
            LoadTexture(env.contentManager, "Sputnik");
            Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
            CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.DisableSleep);
            AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
            Position = new Vector2(x, y);
            SetPhysicsVelocityOnce(new Vector2(vx, vy));
            controller = new PlayerController(env);
        }

        public override void Update(float elapsedTime)
        {
            State s;
            s.position = Position;
            s.velocity = DesiredVelocity;
            s.direction = Rotation;
            s.maxSpeed = 100.0f;
            s.maxTurn = 0.025f;
            s.shoot = false;
            State newState = controller.Update(s);
            DesiredVelocity = newState.velocity;
            Rotation = newState.direction;
            Position += DesiredVelocity * elapsedTime;
            // Use "RemoveAll" function to iterate over a list and handle removals.
            Children.ForEach((Entity ent) => { ent.Update(elapsedTime); });
        }
    }
}
