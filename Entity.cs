using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

class Entity {
	// Position and motion.
	public Vector2 position = new Vector2(0, 0);
	public Vector2 velocity = new Vector2(0, 0);
	public float rotation = 0.0f;

	// Graphics.
	protected Texture2D m_texture;
	public Vector2 registration = new Vector2(0, 0);
	public float zindex = 1.0f;

	// Environment.
	protected Environment m_env;

	/*************************************************************************/
	// Constructors/destructors.

	public Entity(Environment env) {
		m_env = env;
		m_env.RegisterEntity(this);
	}

	public void Destroy() {
		m_env.UnregisterEntity(this);
	}

	/*************************************************************************/

	public void LoadTexture(string assetName) {
		m_texture = m_env.contentManager.Load<Texture2D>(assetName);
	}

	/*************************************************************************/
	// Game loop methods.

	public void Update(float elapsedTime) {
		position += velocity * elapsedTime;
	}

	public void Draw(SpriteBatch spriteBatch) {
		spriteBatch.Draw(m_texture, position, null, Color.White, rotation, registration, 1.0f, SpriteEffects.None, zindex);
	}
}

