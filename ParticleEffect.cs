using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;

namespace Spong
{
    internal class ParticleEffect
    {
        Vector2 _arena = new Vector2();
        int _amount = 0;
        List<Particle> particles = new List<Particle>();
        List<Color> colors = new List<Color>();
        Random r = new Random();
        bool ready = false;

        private const float gravity = 0.05F;
        private const float damping = 0.999F;

        public ParticleEffect(int amount, Vector2 arena)
        {
            _amount = amount;
            _arena = arena;
        }

        public bool Initialize()
        {
            GenerateColorList();

            //
            // Set up initial data for particles
            //
            Particle p;
            for (int i = 0; i < _amount; i++)
            {
                p = new Particle();
                p.Lifespan = 2000;
                p.Fadetime = 500;

                p.Arena = _arena;
                particles.Add(p);
            }

            ready = false;

            return true;
        }

        private void GenerateColorList()
        {
		    for(double i = 3800.0f; i < 7800.0f; i += 3.9f)
		    {
                colors.Add(WavelengthToRGB(i));
		    }
        }

        private Color WavelengthToRGB(Double wl)
        {
            //private const float naturalBase = 2.718282f;

            Double rhi = 0.0;
            Double rlo = 0.0;

            Double r = 0.0;
            Double g = 0.0;
            Double b = 0.0;

            if((wl >= 3800.0) & (wl <= 7800.0))
            {
                rhi = 1.0 / (1.0 + Math.Exp(((5400.0 - wl) / 150.0)));
                rhi = rhi / (1.0 + Math.Exp(((wl - 6700.0) / 200.0)));
                rlo = 0.4 / (1.0 + Math.Exp(((wl - 4400.0) / 300.0)));
                rlo = rlo / (1.0 + Math.Exp(((3900.0 - wl) / 200.0)));
                r = rhi + rlo;

                g = 1.0 / (1.0 + Math.Exp(((wl - 6000.0) / 200.0)));
                g = g / (1.0 + Math.Exp(((4800.0 - wl) / 150.0)));
                g = g * 1.02;

                b = 1.0 / (1.0 + Math.Exp(((wl - 5000.0) / 150.0)));
                b = b / (1.0 + Math.Exp(((4000.0 - wl) / 200.0)));
                b = b * 1.0;
            }

            int mul = 267;
            r *= mul;
            g *= mul;
            b *= mul;

    		int lowcol = 16;
    		if(r < lowcol) r = lowcol;
		    if(g < lowcol) g = lowcol;
    		if(b < lowcol) b = lowcol;

            return Color.FromArgb(255, (int)r, (int)g, (int)b);
        }


        public bool Reset(Vector2 pos, Vector2 dir)
        {
            Particle p;
            Random r = new Random((int)(DateTime.Now.Ticks << 2));

            for (int i = 0; i < particles.Count; i++)
            {
                if (dir.X < 0f)
                {
                    pos.X = (float)(pos.X - 5.0 + r.Next(10));
                    pos.Y = (float)(pos.Y - 5.0 + r.Next(10));
                }
                else
                {
                    pos.X = (float)(pos.X + 5.0 - r.Next(10));
                    pos.Y = (float)(pos.Y + 5.0 - r.Next(10));
                }

                dir.Y *= (float)r.NextDouble();
                dir.X *= (float)r.NextDouble() + (float)r.NextDouble() / 2;

                p = particles[i];
                //p.Color = Color.FromArgb(128 + i, 128, 128, 128 - i);
                p.Color = colors[r.Next(colors.Count)];
                //p.Color = Color.Yellow;
                p.Position = pos;
                p.Direction = dir;
                p.Reset();
            }

            ready = true;
            return true;
        }


        /// <summary>
        /// Checks particles to see if there's any that are still alive.
        /// </summary>
        /// <returns>true</returns>
        public bool isAlive()
        {
            if(particles.Count == 0)
                return false;

            for (int i = 0; i < particles.Count; i++)
                if (!particles[i].isDead())
                    return true;

            return false;
        }


        public void Refresh(Device device, Sprite particleSprite, Texture particleTexture)
        {
            if (!ready)
                return;

            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Direction.Y += gravity;
                particles[i].Position += particles[i].Direction;
                particles[i].Direction *= damping;

                particles[i].Refresh(device, particleSprite, particleTexture);
            }

        }

    }
}
