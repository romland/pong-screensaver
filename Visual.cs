using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Spong
{
    /// <summary>
    /// Movable Windows.Forms.Control.
    /// </summary>
    public class Visual
    {
        internal Vector2 Position = new Vector2(0f, 0f);
        internal Vector2 Direction = new Vector2(0f, 0f);

        private float height = 0f, width = 0f;
        private Color color = Color.Red;
        private Vector2 _arena = new Vector2(0f, 0f);


        #region Properties
        internal Vector2 Arena
        {
            get { return _arena; }
            set { _arena = value; }
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public float Width
        {
            get { return width; }
            set { width = value; }
        }

        public float Height
        {
            get { return height; }
            set { height = value; }
        }

        public float Left
        {
            get { return Position.X; }
            set { Position.X = value; }
        }

        public float Top
        {
            get { return Position.Y; }
            set { Position.Y = value; }
        }
        #endregion


        public Visual()
        {
            Position.X = 0f;
            Position.Y = 0f;
        }


        public Visual(float x, float y)
        {
            Position.X = x;
            Position.Y = y;
        }


        /// <summary>
        /// Collision handling here (XXX: not sure if it's the best of places)
        /// Default collision handling is to just 'stop', purposfully virtual.
        /// </summary>
        public virtual void PerformMove()
        {
            Vector2 newPos = Position + Direction;

            if (newPos.X >= 0f && newPos.X <= (Arena.X - Width))
            {
                Position.X = newPos.X;
            } // else TODO: Move to max X?

            if (newPos.Y >= 0f && newPos.Y <= (Arena.Y - Height))
            {
                Position.Y = newPos.Y;
            } // else TODO: Move to max Y?
        }


        public virtual void Refresh(Device device, Texture texture)
        {
            Top = (int)Position.Y;
            Left = (int)Position.X;

            Rectangle rect = new Rectangle(64, 64, (int)Width, (int)Height);

            using (Sprite s = new Sprite(device))
            {
                s.Begin(SpriteFlags.AlphaBlend);
                s.Draw(texture, 
                    rect, 
                    new Vector3(0f, 0f, 0f), 
                    new Vector3(Position.X, Position.Y, 0f),
                    Color.FromArgb(255, 255, 255, 255));
                s.End();
            }
        }


    }
}
