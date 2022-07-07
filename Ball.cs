using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Spong
{
    public class Ball : Visual
    {
        // Need to keep references to objects we can collide with. Ugly?
        public List<Visual> movables = new List<Visual>();

        // Event: Triggered when we intersected with one of the movables.
        public delegate void IntersectEventHandler(object source, Vector2 position);
        public event IntersectEventHandler Intersected;

        // Event: Triggered when ball determined something scored.
        public delegate void ScoreEventHandler(object source, Vector2 position);
        public event ScoreEventHandler Scored;


        public Ball()
            : base()
        {
            Initialize();
        }


        public void Initialize()
        {
            base.Color = System.Drawing.Color.White;
        }


        public override void PerformMove()
        {
            Vector2 newPos = Position + Direction;

            if (isScored(newPos))
            {
                if (Scored != null)
                    Scored(this, newPos);
                return;
            }

            if (newPos.X >= 0f && newPos.X <= (Arena.X - Width))
                Position.X = newPos.X;
            else
            {
                Direction.X = -Direction.X;
                if (Intersected != null)
                    Intersected(this, newPos);
            }

            if (newPos.Y >= 0f && newPos.Y <= (Arena.Y - Height))
                Position.Y = newPos.Y;
            else
            {
                Direction.Y = -Direction.Y;
                if (Intersected != null)
                    Intersected(this, newPos);
            }

            if (isIntersected())
            {
                Direction.X = -Direction.X;

                if (Intersected != null)
                    Intersected(this, newPos);
            }

        }


        bool isScored(Vector2 newPos)
        {
            // hit right "wall" (so, left scored)
            if (newPos.X >= (Arena.X - Width))
            {
                return true;
            }

            // hit left "wall" (so, right scored)
            if (newPos.X <= 0)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Determine if we're heading towards 'ob'
        /// </summary>
        /// <param name="ob"></param>
        /// <returns></returns>
        public bool isHeadingTowards(Visual ob)
        {
            if((Direction.X > 0f && ob.Position.X > Position.X) || (Direction.X < 0f && ob.Position.X < Position.X))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Determine if this rectangle intersect with another or not.
        /// Assumes rectangles and that they are aligned in both axes (not rotated)
        /// </summary>
        /// <returns>true if there was an intersection</returns>
        bool isIntersected()
        {
            foreach (Visual ob in movables)
            {
                if (!(Position.X > (ob.Position.X + ob.Width) || (Position.X + Width) < ob.Position.X ||
                     Position.Y > (ob.Position.Y + ob.Height) || (Position.Y + Height) < ob.Position.Y))
                {
                    return true;
                }
            }

            return false;
        }


    }
}
