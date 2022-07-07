using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;


namespace Spong
{
    internal class Particle : Visual
    {
        private const float baseAlpha = 20;
        private const float lengthAlpha = 175;
        private const float minTexRRadius = 0.6F;
        private const float energyLoss = 0.8F;

        private const float oTextureRadius = 32;

        long _created = 0;
        long _lifespan = 0;
        long _fadetime = 0;
        float totalFade = 0;
		
        // For how long we will fade after lifespan
        public long Fadetime { get { return _fadetime; } set { _fadetime = value; } }
		
        // How many milliseconds will we live
		public long Lifespan { get { return _lifespan; } set { _lifespan = value; } }


		public Particle()
		{
			Position = new Vector2(0f, 0f);
            Direction = new Vector2(1f, 1f);
            Color = Color.Black;

            _lifespan = 1000;
            _created = DateTime.Now.Ticks / 10000;
            _fadetime = 200;
        }

        public void Reset()
        {
            totalFade = 0;
            _created = DateTime.Now.Ticks / 10000;
        }

        public bool isDead()
        {
            //if (Position.X > Arena.X || Position.X < 0 || Position.Y > Arena.Y || Position.Y < 0)
            //    return true;

            if (totalFade >= 255)
                return true;

            return _created + Lifespan + Fadetime < (DateTime.Now.Ticks / 10000);
        }

        public bool isFading()
        {
            return _created + Lifespan < (DateTime.Now.Ticks / 10000);
        }
		
		public long fadeTimeLeft()
		{
			return _created + Lifespan + Fadetime - (DateTime.Now.Ticks / 10000);
		}

        public override void PerformMove()
        {
            // Mask. We do our moving in Refresh()
        }


        /// <summary>
        /// Draw sprite on canvas.
        /// </summary>
        /// <param name="device">our canvas</param>
        /// <param name="sprite">sprite to use as particle</param>
        /// <param name="texture">texture of particle</param>
        /// <returns>false if the particle should die, otherwise true</returns>
        public bool Refresh(Device device, Sprite sprite, Texture texture)
        {
            int color;

            float length;
            float alpha;
            float angle;
            float width;
            float dlength;

            Vector2 normalized;

            if (isDead() == true)
            {
                return false;
            }

            length = Direction.Length();
#if false
            // Alpha
            alpha = (length / oTextureRadius) * lengthAlpha;
            alpha += baseAlpha;
            if (alpha > lengthAlpha) 
                alpha = lengthAlpha;
#else
            alpha = 200;
#endif
            // Fade on death
            if (isFading() == true)
            {
                totalFade += (alpha / fadeTimeLeft() * 6);
                alpha -= totalFade;
/*
                if(totalFade > 10)
                    System.Diagnostics.Debug.Print("" + alpha + ", fade: " + totalFade);
*/
                if (alpha < 0f)
                {
                    alpha = 0f;
                }
            }

            // Color
            color = Color.FromArgb((int)alpha, Color).ToArgb();

            // Angles
            angle = (float)Math.Atan2(Direction.Y, Direction.X);
            normalized = Direction;
            normalized.Normalize();

            // Width
            width = oTextureRadius / length;
            if (width > 1) 
                width = 1;

            // Length
            dlength = length / oTextureRadius;
            if (dlength < minTexRRadius) 
                dlength = minTexRRadius;

            sprite.Begin(SpriteFlags.SortTexture);
            device.SetTransform
	            (
		            TransformType.View,
		            Matrix.Transformation2D
		            (
			            Vector2.Empty, 0,
                        new Vector2(dlength, width),
			            new Vector2(oTextureRadius, oTextureRadius),
			            angle,
			            Position - new Vector2(oTextureRadius, oTextureRadius) - (Direction - normalized * oTextureRadius)
		            )
	            );

            sprite.Draw2D(texture, Point.Empty, 0, Point.Empty, color);
            sprite.End();

            #region Collision Detection
            if (Direction.Y >= 0)
            {
                if (Position.Y > Arena.Y)
                {
                    Direction.Y = -Direction.Y * energyLoss;
                    Position.Y = Arena.Y - (Position.Y - Arena.Y);
                }
            }

            if (Direction.Y <= 0)
            {
                if (Position.Y < 0)
                {
                    Direction.Y = -Direction.Y * energyLoss;
                    Position.Y = -Position.Y;
                }
            }

            if (Direction.X >= 0)
            {
                if (Position.X > Arena.X)
                {
                    Direction.X = -Direction.X * energyLoss;
                    Position.X = Arena.X - (Position.X - Arena.X);
                }
            }

            if (Direction.X <= 0)
            {
                if (Position.X < 0)
                {
                    Direction.X = -Direction.X * energyLoss;
                    Position.X = -Position.X;
                }
            }
            #endregion

            return true;
        }


    } // class
}
