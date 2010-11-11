using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sputnik.Menus {
	class TextWidget : Widget {
		private SpriteFont Font;

		public string Text {
			get {
				return m_text;
			}

			set {
				m_text = value;
				m_measurementsValid = true;
			}
		}

		public enum Align {
			TopLeft = (Top | Left)

			, Left = (1 << 0)
			, Right = (1 << 1)
			, HCenter = (Left | Right)

			, Top = (1 << 2)
			, Bottom = (1 << 3)
			, VCenter = (Top | Bottom)

			, VHCenter = (HCenter | VCenter)
		}
		
		public Align Alignment = Align.VHCenter;

		private string m_text;
		private bool m_measurementsValid = false;
		private Vector2 m_textOffset = Vector2.Zero;

		public TextWidget(Menu menuEnv, string fontName, string text = "")
				: base(menuEnv) {
			Text = text;
			SetFont(fontName);
		}

		public void SetFont(string fontName) {
			Font = Menu.contentManager.Load<SpriteFont>(fontName);
			m_measurementsValid = false;
		}

		/// <summary>
		/// Draw the text.  Overrides Widget.
		/// </summary>
		/// <param name="spriteBatch">SpriteBatch to render to.</param>
		public override void Draw(SpriteBatch spriteBatch) {
			if (!m_measurementsValid) {
				Vector2 textSize = Font.MeasureString(Text);

				if ((Alignment & Align.Right) == Align.Right) m_textOffset.X = -textSize.X;
				if ((Alignment & Align.HCenter) == Align.HCenter) m_textOffset.X = -textSize.X / 2;

				if ((Alignment & Align.Bottom) == Align.Bottom) m_textOffset.Y = -textSize.Y;
				if ((Alignment & Align.VCenter) == Align.VCenter) m_textOffset.Y = -textSize.Y / 2;

				m_measurementsValid = true;
			}

			if (Text != null && Text.Length > 0) {
				// Determine aligned text pos and then round it as text looks bad if it is not drawn on pixel boundaries.
				Vector2 drawPos = AbsolutePosition + m_textOffset;
				drawPos.X = (float) Math.Round((double) drawPos.X);
				drawPos.Y = (float) Math.Round((double) drawPos.Y);

				spriteBatch.DrawString(Font, Text, drawPos, VertexColor);
			}

			foreach (Entity ent in Children) {
				ent.Draw(spriteBatch);
			}
		}
	}
}
