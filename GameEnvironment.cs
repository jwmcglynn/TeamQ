﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Physics = FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Tiled = Squared.Tiled;
using System.IO;

using FarseerPhysics.Controllers;

namespace Sputnik {
	public class GameEnvironment : Environment {
		// Spawning/culling constants.
		public const float k_cullRadius = 300.0f; // Must be greater than spawn radius.
		public const float k_spawnRadius = 100.0f; // Must be less than cull radius.


		private SpriteBatch m_spriteBatch;
		private Tiled.Map m_map;
		public Camera2D Camera;
		private SpawnController m_spawnController;

		// Physics.
		private Physics.DebugViewXNA m_debugView;
		public Physics.Dynamics.World CollisionWorld;

		public static float k_physicsScale = 1.0f / 50.0f; // 50 pixels = 1 meter.
		public static float k_invPhysicsScale = 50.0f; // ^ must be inverse.

		private Matrix m_projection;

		// Update loop.
		public float m_updateAccum; // How much time has passed relative to the physics world.

		//FPS Counters
		private float frameTime;
		protected int fps;
		private int frameCounter;

		public BlackHolePhysicsController BlackHoleController;
		public List<Vector2> PossibleBlackHoleLocations = new List<Vector2>();


		public GameEnvironment(Controller ctrl)
				: base(ctrl) {

			Camera = new Camera2D(this);

			// Create a new SpriteBatch, which can be used to draw textures.
			m_spriteBatch = new SpriteBatch(ctrl.GraphicsDevice);

			CollisionWorld = new Physics.Dynamics.World(Vector2.Zero);

			m_debugView = new Physics.DebugViewXNA(CollisionWorld);

			// Create collision notification callbacks.
			CollisionWorld.ContactManager.PreSolve += PreSolve;
			CollisionWorld.ContactManager.BeginContact += BeginContact;
			CollisionWorld.ContactManager.EndContact += EndContact;

			BlackHoleController = new BlackHolePhysicsController(300.0f, 100.0f * k_physicsScale, 9.0f * k_physicsScale); // 300 controls how strong the pull is towards the black hole.
																									// 100.0 determines the radius fore which black hole will have an effect on.
			CollisionWorld.AddController(BlackHoleController);

			// TODO: Make SpriteBatch drawing use this projection too.
			m_projection = Matrix.CreateOrthographicOffCenter(0.0f, ctrl.GraphicsDevice.Viewport.Width, ctrl.GraphicsDevice.Viewport.Height, 0.0f, -1.0f, 1.0f);
		}

		private enum Tile {
			None
			, AsteroidWall
			, WhiteBlock
			, GreyBlock
		};

		public void LoadMap(string filename) {
			m_map = Tiled.Map.Load(Path.Combine(Controller.Content.RootDirectory, filename), Controller.Content);
			
			// Destroy and re-create collision body for map.
			DestroyCollisionBody();
			CreateCollisionBody(CollisionWorld, Physics.Dynamics.BodyType.Static);

			Vector2 tileHalfSize = new Vector2(m_map.TileWidth, m_map.TileHeight) / 2;

			foreach (Tiled.Layer layer in m_map.Layers.Values) {
				for (int x = 0; x < layer.Width; ++x)
				for (int y = 0; y < layer.Height; ++y) {
					Tile tileType = (Tile) layer.GetTile(x, y);
					
					switch (tileType) {
						case Tile.AsteroidWall:
						case Tile.GreyBlock:
							// Create collision.
							AddCollisionRectangle(tileHalfSize, new Vector2(m_map.TileWidth * x, m_map.TileHeight * y) - tileHalfSize);
							break;
					}
				}
			}

			m_spawnController = new SpawnController(this, m_map.ObjectGroups.Values);
		}

		public override void Update(float elapsedTime) {
			m_updateAccum += elapsedTime;

			// Update physics.
			const float k_physicsStep = 1.0f / 60.0f;
			while (m_updateAccum > k_physicsStep) {
				m_updateAccum -= k_physicsStep;

				// Update entities.
				m_spawnController.Update(k_physicsStep);
				base.Update(k_physicsStep);
				Camera.Update(k_physicsStep);

				CollisionWorld.Step(k_physicsStep);
			}

			// Toggle debug view.
			if (Keyboard.GetState().IsKeyDown(Keys.F1) && !OldKeyboard.GetState().IsKeyDown(Keys.F1)) {
				if (m_debugView != null) m_debugView = null;
				else m_debugView = new Physics.DebugViewXNA(CollisionWorld);
			}

			//FPS counter
			frameCounter++;
			frameTime += elapsedTime;
			if (frameTime >= 1.0f)
			{
				fps = frameCounter;
				frameTime = 0;
				frameCounter = 0;
				Controller.Window.Title = "Sputnik (" + fps + " fps)";
			}
		}

		/// <summary>
		/// Draw the world.
		/// </summary>
		public override void Draw() {
			m_spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

			Vector2 mapOffset = new Vector2(Controller.GraphicsDevice.Viewport.Width - m_map.TileWidth, Controller.GraphicsDevice.Viewport.Height - m_map.TileHeight);

			// Draw map.
			if (m_map != null) m_map.Draw(m_spriteBatch, new Rectangle(
					(int) -Camera.CenterOffset.X
					, (int) -Camera.CenterOffset.Y
					, Controller.GraphicsDevice.Viewport.Width + (int) Camera.CenterOffset.X
					, Controller.GraphicsDevice.Viewport.Height + (int) Camera.CenterOffset.Y
				), Camera.Position - mapOffset
			);
			m_spriteBatch.End();

			// Draw entities.
			m_spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, Camera.Transform);
			Draw(m_spriteBatch);
			m_spriteBatch.End();

			if (m_debugView != null) {
				// Debug drawing.
				Matrix debugMatrix = Matrix.CreateScale(k_invPhysicsScale) * Camera.Transform;
				m_debugView.RenderDebugData(ref m_projection, ref debugMatrix);
			}
		}

		public void BeginContact(Physics.Dynamics.Contacts.Contact contact) {
			// PreSolve performs the same function as this, only continue if one is a sensor.
			if (!contact.FixtureA.IsSensor && !contact.FixtureB.IsSensor) return;
			
			HandleContact(contact);
		}

		/// <summary>
		/// Farseer Physics callback.  Handles the case where two objects collide.
		/// </summary>
		/// <param name="contact">Contact point.</param>
		/// <param name="oldManifold">Manifold from last update.</param>
		protected void PreSolve(Physics.Dynamics.Contacts.Contact contact, ref Physics.Collision.Manifold oldManifold) {
			HandleContact(contact);
		}

		protected void EndContact(Physics.Dynamics.Contacts.Contact contact) {
			// Get Entities from both shapes.
			Entity entA = (Entity) contact.FixtureA.Body.UserData;
			Entity entB = (Entity) contact.FixtureB.Body.UserData;

			entA.OnSeparate(entB, contact);
			entB.OnSeparate(entA, contact);
		}

		/// <summary>
		/// Handle contact interactions.
		/// </summary>
		/// <param name="contact">Contact point.</param>
		private void HandleContact(Physics.Dynamics.Contacts.Contact contact) {
			if (!contact.IsTouching()) return;

			// Get Entities from both shapes.
			Entity entA = (Entity) contact.FixtureA.Body.UserData;
			Entity entB = (Entity) contact.FixtureB.Body.UserData;

			// Determine if shapes agree to collide.
			bool shouldCollide = entA.ShouldCollide(entB);
			shouldCollide &= entB.ShouldCollide(entA);

			contact.Enabled = shouldCollide;

			if (shouldCollide) {
				entA.OnCollide(entB, contact);
				entB.OnCollide(entA, contact);
			}
		}

		/// <summary>
		/// Current FPS.
		/// </summary>
		public int FPS
		{
			get
			{
				return fps;
			}
		}
	}
}
