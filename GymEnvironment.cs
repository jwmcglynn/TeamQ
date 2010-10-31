using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;

namespace Sputnik {
    class GymEnvironment : GameEnvironment
    {
        FarseerPhysics.Dynamics.Body m_testBody;

        public GymEnvironment(Controller ctrl)
            : base(ctrl)
        {
            Entity test = new Entity(this);
            test.LoadTexture("redball");
            test.velocity = new Vector2(5.0f, 5.0f);

            Entity test2 = new Entity(this);
            test2.LoadTexture("redball");
            test2.velocity = new Vector2(15.0f, 15.0f);



            Entity target = new Entity(this);
            target.LoadTexture("redball");
            target.velocity = new Vector2(0.0f, 0.0f);

            Bullet bulletTest = new Bullet(this);
            bulletTest.LoadTexture("bullet");
            bulletTest.position = new Vector2(300.0f, 0.0f);
            bulletTest.velocity = new Vector2(-50.0f, 0.0f);

            SpecialAbility special = new SpecialAbility(this);
            special.LoadTexture("bullet");
            special.position = new Vector2(300.0f, 100.0f);
            special.velocity = new Vector2(0.0f, 0.0f);

            ///// Collision testing.
            m_testBody = collisionWorld.CreateBody();
            m_testBody.BodyType = FarseerPhysics.Dynamics.BodyType.Dynamic;
            m_testBody.Position = new Vector2(5.0f, 5.0f);

            //We create a circle shape with a radius of 0.5 meters
            CircleShape circleShape = new CircleShape(0.5f);

            //We fix the body and shape together using a Fixture object
            Fixture fixture = m_testBody.CreateFixture(circleShape);

            m_testBody.ApplyForce(new Vector2(10.0f, 5.0f));
            m_testBody.ApplyTorque(50.0f);
        }
    }
}
