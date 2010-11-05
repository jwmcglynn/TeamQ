using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Controllers;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class BlackHole : SpecialAbility
	{
		GameEnvironment env;

		public BlackHole(GameEnvironment e)
		{
			env = e;

			LoadTexture(env.contentManager, "black_hole_small");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			Zindex = 0.0f;
			
			CreateCollisionBody(env.CollisionWorld, FarseerPhysics.Dynamics.BodyType.Static, CollisionFlags.DisableSleep);
			var circle = AddCollisionCircle(Texture.Height / 12, Vector2.Zero); // Using 12 here as an arbitrary value. Reason: Want the black hole to have a small collis
			circle.IsSensor = true; 
			
			Position = new Vector2(50.0f, 50.0f);

			//CollisionBody.IgnoreGravity = true;

			env.physicsController.AddBody(CollisionBody);
		}

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			Console.WriteLine("COLLIDED");

			if (entB is TakesDamage)
			{
				((TakesDamage)entB).InstaKill();
			}

			// Disable collision response.
			contact.Enabled = false;

			base.OnCollide(entB, contact);
		}

		public override void Update(float elapsedTime)
		{
			base.Update(elapsedTime);
		}
	}
}
