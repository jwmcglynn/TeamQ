using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

// Comment out later
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik
{
    class Ship : Entity, TakesDamage
    {
        private ShipController ai;
        private ShipController previousAI = null;
        private int health = 10;

        private BulletEmitter shooter = null;

        public float maxSpeed;
        public float maxTurn;

        public Ship(AIController ai, GameEnvironment env) : base() 
        {
            this.ai = ai;
            this.maxSpeed = 50.0f;
            this.maxTurn = 0.025f;

            shooter = new BulletEmitter(env, BulletEmitter.BulletStrength.Medium, true);
            env.AddChild(shooter);
        }

        public override void Update(float elapsedTime)
        {
            ai.Update(this, elapsedTime);
            this.m_position += this.m_velocity * elapsedTime;
            base.Update(elapsedTime);
        }

        // Attach Sputnik to the ship
        public void Attach(SputnikShip sp)
        {
            this.previousAI = this.ai;
            this.ai = sp.GetAI();
        }

        // An Entity deals damage to the Ship.  Currently, only the 
        // damage from bullets is implemented.
        public void TakeHit(Entity attack)
        {
            if(attack is Bullet)
            {
                this.health -= ((Bullet)attack).getBulletStrength();
                if(this.health < 1)
                {
                    this.KillShip();
                }
            }
        }

        public void Shoot(float elapsedTime)
        {
            shooter.IsShooting = true;
            shooter.Rotation = this.Rotation;
            shooter.Position = this.Position;
            shooter.Update(elapsedTime);
            shooter.IsShooting = false;
        }

        public bool isSputnik()
        {
            if (this.ai is PlayerController)
                return true;
            return false;
        }

        public void KillShip()
        {
            // Perform what ever actions are necessary to 
            // Destory a ship
        }

        public bool IsFriendly()
        {
            return false;
        }

        public void TestShip(float x, float y,float vx, float vy, float sx, float sy, float fx, float fy, GameEnvironment env)
        {
            LoadTexture(env.contentManager, "circloid");
            Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
            CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.DisableSleep);
            AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
            Position = new Vector2(x, y);
            SetPhysicsVelocityOnce(new Vector2(vx, vy));
            ai = new AIController(new Vector2(sx, sy), new Vector2(fx, fy), env);
        }

        public void TestShip(float x, float y, float vx, float vy,GameEnvironment env)
        {
            LoadTexture(env.contentManager, "Sputnik");
            Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
            CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.DisableSleep);
            AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
            Position = new Vector2(x, y);
            SetPhysicsVelocityOnce(new Vector2(vx, vy));
            ai = new PlayerController(env);
        }
    }
}
