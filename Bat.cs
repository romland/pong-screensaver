using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Spong
{
    public class Bat : Visual
    {
        Keys moveUpKey = Keys.W;
        Keys moveDownKey = Keys.S;
        string name = "Unnamed Bat";
        bool autoPilot = true;
        bool missNext = false;
        Ball ball = null;
        float speed = 4f;

        #region Properties
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }


        public bool AutoPilot
        {
            get { return autoPilot; }
            set { autoPilot = value; }
        }


        public bool MissNext
        {
            get { return missNext; }
            set { missNext = value; }
        }


        public Ball Ball
        {
            get { return ball; }
            set { ball = value; }
        }


        public Keys MoveUpKey
        {
            get { return moveUpKey; }
            set { moveUpKey = value; }
        }

        
        public Keys MoveDownKey
        {
            get { return moveDownKey; }
            set { moveDownKey = value; }
        }
        
        
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        #endregion


        public Bat()
            : base()
        {
            base.Position = new Vector2(0f, 0f);
            Initialize();
        }


        public Bat(Vector2 startPosition)
            : base()
        {
            base.Position = startPosition;
            Initialize();
        }


        public void Initialize()
        {
            base.Color = System.Drawing.Color.White;
        }


        public void KeyUp(KeyEventArgs e)
        {
            base.Direction.Y = 0f;
        }


        public void KeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == MoveDownKey) 
                base.Direction.Y = 4f;
            else if (e.KeyCode == MoveUpKey) 
                base.Direction.Y = -4f;
        }


        public override void PerformMove()
        {
            if (AutoPilot == true)
            {
                Direction.Y = Ball.Direction.Y;

                if (Ball.isHeadingTowards(this))
                {
					// Ball is heading towards us, track it.
                    if (Ball.Position.Y > (Position.Y + (Height / 3)))
                    {
                        // ball's position is below us
                        Direction.Y = Speed;
                    }
                    else if (Ball.Position.Y < (Position.Y + (Height / 3)))
                    {
                        // ball's position is above us
                        Direction.Y = -Speed;
                    }

                }
                else
                {
                    // Move to middle when ball is not heading towards us
                    Vector2 home = new Vector2(0, (Arena.Y / 2) - (Height / 2));
                    if (Position.Y == home.Y)
					{
                        Direction.Y = 0f;
					}
                    else
					{
                        if (Position.Y > home.Y)
                            Direction.Y = -Speed;
                        else
                            Direction.Y = Speed;
					}
                }

            }

            // We're told to miss next move so mess us up in some way.
            if (MissNext)
            {
                //_direction.Y = -_direction.Y;         // Reverse my direction
                Direction.Y = (Direction.Y / (Speed - 1));       // Slow me down
            }

            base.PerformMove();

        } // PerformMove

    }
}
