using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;

namespace Sputnik {
	class GymEnvironment : GameEnvironment {
		public GymEnvironment(Controller ctrl)
				: base(ctrl) {

			Entity[] testBalls = new Entity[3];

			for (int i = 0; i < 3; ++i) {
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

			Bullet bulletTest = new Bullet();
			bulletTest.LoadTexture(contentManager, "bullet");
			bulletTest.Position = new Vector2(300.0f, 0.0f);
			bulletTest.DesiredVelocity = new Vector2(-50.0f, 0.0f);
			AddChild(bulletTest);

			SpecialAbility special = new SpecialAbility();
			special.LoadTexture(contentManager, "bullet");
			special.Position = new Vector2(300.0f, 100.0f);
			special.DesiredVelocity = new Vector2(0.0f, 0.0f);
			AddChild(special);
		}
	}
}
