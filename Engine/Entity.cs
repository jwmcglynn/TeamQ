﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Physics = FarseerPhysics;
using System.IO;

namespace Sputnik {
	public class Entity : IDisposable {
		// Position and motion.
		private Vector2 m_position = new Vector2(0, 0); // Intentionally private.  Use Position.
		private Vector2 m_velocity = new Vector2(0, 0); // Intentionally private.  Use DesiredVelocity.
		private float m_rotation = 0.0f;

		// Graphics.
		public Texture2D Texture;
		public Vector2 Registration = new Vector2(0, 0);
		public float Zindex = 1.0f;

		// Entity tree.
		public Entity Parent;
		public List<Entity> Children = new List<Entity>();

		// Collision.
		public Physics.Dynamics.World CollisionWorld;
		public Physics.Dynamics.Body CollisionBody;
		private bool m_applyVelocity = false;
		public bool VisualRotationOnly = false;

		public float TimeSinceTeleport = float.PositiveInfinity;
		public Vector2 TeleportInertiaDir;

		// Sound.
		public AudioEmitter SoundEmitter;

		// Update helper.
		public delegate void UpdateTask();
		public event UpdateTask OnNextUpdate;

		/*************************************************************************/
		// Entity tree.

		/// <summary>
		/// Add a child to this Entity.  Children of this entity will have their
		/// Update() and Draw() methods called after this Entity does them.
		/// </summary>
		/// <param name="child">Entity to add to children.  Entity will be removed from existing
		/// 		parent if one exists.</param>
		public void AddChild(Entity child) {
			// Remove from current parent.
			child.Remove();

			// Add to this entity.
			child.Parent = this;
			Children.Add(child);
		}

		/// <summary>
		/// Remove child.  Stops Update() and Draw() from being called and allows
		/// Entity to be garbage collected.
		/// </summary>
		/// <param name="child">Entity to remove from children.</param>
		public void RemoveChild(Entity child) {
			child.Parent = null;
			Children.Remove(child);
		}

		/// <summary>
		/// Remove entity from parent.  Stops Update() and Draw() from being called and
		/// allows Entity to be garbage collected.
		/// </summary>
		public void Remove() {
			if (Parent != null) Parent.RemoveChild(this);
		}

		/// <summary>
		/// Remove current entity from world and destroy its associated collision body.
		/// </summary>
		public virtual void Dispose() {
			Remove();
			DestroyCollisionBody();

			Children.ForEach((Entity ent) => {
				ent.Dispose();
			});
		}

		/*************************************************************************/
		// State.

		/// <summary>
		/// Load a sprite texture for this entity.
		/// </summary>
		/// <param name="contentManager">ContentManager to use when loading asset.  Found inside of Environment.</param>
		/// <param name="assetName">Filename (without file extension) of the .png to load.
		/// 		Must be added to the TeamQContent project.
		/// </param>
		public void LoadTexture(ContentManager contentManager, string assetName) {
			Texture = contentManager.Load<Texture2D>(assetName);
		}

		/// <summary>
		/// Current position in local space.
		/// </summary>
		public Vector2 Position {
			get {
				if (CollisionBody != null) return CollisionBody.Position * GameEnvironment.k_invPhysicsScale;
				else return m_position;
			}

			set {
				if (CollisionBody != null) {
					CollisionBody.Position = value * GameEnvironment.k_physicsScale;
				} else m_position = value;
			}
		}

		public Rectangle VisibleRect {
			get {
				Rectangle rect = new Rectangle();

				if (Texture != null) {
					rect.Location = new Point((int) -Registration.X, (int) -Registration.Y);
					rect.Width = Texture.Width;
					rect.Height = Texture.Height;
				}

				rect.Offset((int) Position.X, (int) Position.Y);

				return rect;
			}
		}

		/// <summary>
		/// Current velocity space.
		/// </summary>
		public Vector2 ActualVelocity {
			get {
				if (CollisionBody != null) return CollisionBody.LinearVelocity * GameEnvironment.k_invPhysicsScale;
				else return m_velocity;
			}
		}

		/// <summary>
		/// Desired velocity.  Use this accessor to change the velocity of objects.  Returns the same value as ActualVelocity
		/// if not using a collision object.
		/// </summary>
		public Vector2 DesiredVelocity {
			get { return m_velocity; }

			set {
				m_applyVelocity = (value != Vector2.Zero);
				m_velocity = value;
			}
		}

		/// <summary>
		/// Current rotation in radians.
		/// </summary>
		public float Rotation {
			get {
				if (CollisionBody != null && !VisualRotationOnly) return CollisionBody.Rotation;
				else return m_rotation;
			}

			set {
				if (CollisionBody != null && !VisualRotationOnly) CollisionBody.Rotation = value;
				else m_rotation = value;
			}
		}

		/// <summary>
		/// Forces this object to move at the desired velocity this frame but does not continue
		/// to apply the velocity.
		/// </summary>
		/// <param name="velocity"></param>
		public void SetPhysicsVelocityOnce(Vector2 velocity) {
			if (CollisionBody == null) throw new ArgumentException("No collision body exists.");
			CollisionBody.LinearVelocity = velocity * GameEnvironment.k_physicsScale;
		}

		/*************************************************************************/
		// Collision.

		/// <summary>
		/// Collision flags to determine how object behaves in the physics engine.
		/// </summary>
		public enum CollisionFlags {
			Default = 0					// Default options.
			, FixedRotation = (1 << 0)	// Object cannot rotate.
			, IsBullet = (1 << 1)		// Object is small and moves at high velocity.  This enables continuous collision detection and prevents tunneling.
			, DisableSleep = (1 << 2)	// Prevent the object from "sleeping" when not moving.  Warning: Only use this if the object will never stop moving.
		};

		/// <summary>
		/// Create and attach a new collision body to this Entity.
		/// </summary>
		/// <param name="world">CollisionWorld instance.</param>
		/// <param name="type">Type of collision body.  Dynamic bodies receive responses, kinematic bodies do not.  Static bodies cannot move.</param>
		/// <param name="flags">Flags.</param>
		public void CreateCollisionBody(Physics.Dynamics.World world, Physics.Dynamics.BodyType type, CollisionFlags flags = CollisionFlags.Default) {
			if (CollisionBody != null) throw new ArgumentException("CreateCollisionBody called on Entity where collision body already exists.");
			CollisionWorld = world;

			Physics.Dynamics.Body body = world.CreateBody();
			
			body.BodyType = type;
			body.Position = m_position * GameEnvironment.k_physicsScale;
			body.LinearVelocity = m_velocity * GameEnvironment.k_physicsScale;
			body.FixedRotation = flags.HasFlag(CollisionFlags.FixedRotation);
			body.SleepingAllowed = !flags.HasFlag(CollisionFlags.DisableSleep);
			body.IsBullet = flags.HasFlag(CollisionFlags.IsBullet);
			body.UserData = this;

			CollisionBody = body;
		}

		/// <summary>
		/// Destroy the collision body attached to this body.
		/// </summary>
		public void DestroyCollisionBody() {
			if (CollisionBody == null) return;

			CollisionWorld.RemoveBody(CollisionBody);
			CollisionWorld = null;
			CollisionBody = null;
		}

		/*************************************************************************/
		// Shape creators.

		/// <summary>
		/// Add an arbitrary Farseer collision shape to the body.
		/// </summary>
		/// <param name="shape">Shape.</param>
		/// <param name="density">Density of the shape.  Mass will be computed automatically.</param>
		/// <returns>Fixture definition.</returns>
		public Physics.Dynamics.Fixture AddCollisionShape(Physics.Collision.Shapes.Shape shape, float density = 1.0f) {
			if (CollisionBody == null) throw new ArgumentException("Cannot add collision shape until collision body is created.");
			return CollisionBody.CreateFixture(shape, density);
		}

		/// <summary>
		/// Add a circle collision shape to the body.
		/// </summary>
		/// <param name="radius">Radius of the circle in body space.</param>
		/// <param name="center">Center of the circle in body space.</param>
		/// <param name="density">Density of shape, used to compute mass.</param>
		/// <returns>Fixture definition.</returns>
		public Physics.Dynamics.Fixture AddCollisionCircle(float radius, Vector2 center, float density = 1.0f) {
			Physics.Collision.Shapes.CircleShape circle = new Physics.Collision.Shapes.CircleShape(radius * GameEnvironment.k_physicsScale);
			circle.Position = center * GameEnvironment.k_physicsScale;
			return AddCollisionShape(circle, density);
		}

		/// <summary>
		/// Add a rectangle collision shape to the body.
		/// </summary>
		/// <param name="halfsize">A vector specifying half of the width and height of the rectangle in body space.</param>
		/// <param name="center">The center point of the rectangle in body space.</param>
		/// <param name="rotation">Rotation of the shape relative to the body in radians.</param>
		/// <param name="density">Density of the shape.</param>
		/// <returns>Fixture definition.</returns>
		public Physics.Dynamics.Fixture AddCollisionRectangle(Vector2 halfsize, Vector2 center, float rotation = 0.0f, float density = 1.0f) {
			Physics.Collision.Shapes.PolygonShape poly = new Physics.Collision.Shapes.PolygonShape();
			poly.SetAsBox(halfsize.X * GameEnvironment.k_physicsScale, halfsize.Y * GameEnvironment.k_physicsScale, center * GameEnvironment.k_physicsScale, rotation);
			return AddCollisionShape(poly, density);
		}

		/// <summary>
		/// Add a line to this body's collision shape.
		/// </summary>
		/// <param name="start">Start point in body space.</param>
		/// <param name="end">End point in body space.</param>
		public Physics.Dynamics.Fixture AddCollisionLine(Vector2 start, Vector2 end) {
			Physics.Collision.Shapes.PolygonShape poly = new Physics.Collision.Shapes.PolygonShape();
			poly.SetAsEdge(start * GameEnvironment.k_physicsScale, end * GameEnvironment.k_physicsScale);
			return AddCollisionShape(poly, 0.0f);
		}

		/*************************************************************************/
		// Collision callbacks.

		/// <summary>
		/// Called when two entities collide.
		/// </summary>
		/// <param name="entB">Other entity.</param>
		/// <param name="contact">Contact point.</param>
		public virtual void OnCollide(Entity entB, Physics.Dynamics.Contacts.Contact contact) {
		}

		/// <summary>
		/// Called when two entities separate.
		/// </summary>
		/// <param name="entB">Other entity.</param>
		/// <param name="contact">Contact point.</param>
		public virtual void OnSeparate(Entity entB, Physics.Dynamics.Contacts.Contact contact) {
		}

		/// <summary>
		/// Should this entity collide with another entity?  Used for filtering collisions.
		/// Default: Returns false.
		/// </summary>
		/// <param name="entB">Entity that we are trying to collide with.</param>
		/// <returns></returns>
		public virtual bool ShouldCollide(Entity entB, Physics.Dynamics.Fixture fixture, Physics.Dynamics.Fixture entBFixture) {
			return true;
		}

		/*************************************************************************/
		// Game loop methods.

		/// <summary>
		/// Update the logic of this entity.
		/// </summary>
		/// <param name="elapsedTime">How much time (in seconds) has passed since the last update?</param>
		public virtual void Update(float elapsedTime) {
			if (OnNextUpdate != null) {
				OnNextUpdate();
				OnNextUpdate = null;
			}

			if (m_applyVelocity) {
				if (CollisionBody == null) {
					m_position += m_velocity * elapsedTime;
				} else {
					CollisionBody.LinearVelocity = m_velocity * GameEnvironment.k_physicsScale;
				}
			}

			// Apply blackhole teleportation force.
			if (CollisionBody != null) {
				if (TimeSinceTeleport < 1.0f) {
					CollisionBody.ApplyForce(TeleportInertiaDir * 50.0f * CollisionBody.Mass);
					CollisionBody.IgnoreGravity = true;
				} else {
					CollisionBody.IgnoreGravity = false;
				}
			}

			TimeSinceTeleport += elapsedTime;

			// Use "RemoveAll" function to iterate over a list and handle removals.
			Children.ForEach((Entity ent) => { ent.Update(elapsedTime); });
		}

		/// <summary>
		/// Draw the entity.
		/// </summary>
		/// <param name="spriteBatch">SpriteBatch to render to.</param>
		public virtual void Draw(SpriteBatch spriteBatch) {
			if (Texture != null) {
				spriteBatch.Draw(Texture, Position, null, Color.White, Rotation, Registration, 1.0f, SpriteEffects.None, Zindex);
			}

			foreach (Entity ent in Children) {
				ent.Draw(spriteBatch);
			}
		}
	}
}
