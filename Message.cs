using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Spong
{
    public class Message
    {
        public string Text;
        public System.Drawing.Point Location;
        public Font Style;
        public System.Drawing.Color Color;

        public Message(Device dev, System.Drawing.Size size, FontWeight weight, string fontname,
                       string text, System.Drawing.Point loc, System.Drawing.Color col)
        {
            Style = new Font(dev, size.Height, size.Width, weight, 0, false,
                    CharacterSet.Default, Precision.Default, FontQuality.Default,
                    PitchAndFamily.FamilyDoNotCare, fontname);
            Text = text;
            Location = new System.Drawing.Point(loc.X, loc.Y);
            Color = col;
        }


        public void Refresh(Sprite spr)
        {
            Style.DrawText(spr, Text, Location, Color);
        }

    }
}
