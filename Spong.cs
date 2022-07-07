using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Spong
{
    public partial class Spong : Form
    {
        /// <summary>
        /// DirectX globals
        /// </summary>
        private int numVerts = 4;
        private Device device = null;
        private Texture genericTexture;
        private VertexBuffer vertexBuffer = null;
        private VertexFormats customVertexFlags = VertexFormats.Position | VertexFormats.Texture1;

        private struct CustomVertex
        {
            public float X;
            public float Y;
            public float Z;
            public float Tu;
            public float Tv;
            public CustomVertex(float x, float y, float z, float tu, float tv)
            {
                X = x;
                Y = y;
                Z = z;
                Tu = tu;
                Tv = tv;
            }
        }

        private Sprite particleSprite;
        private Texture overlayTexture;
        private Texture particleTexture;
        private List<ParticleEffect> effects = new List<ParticleEffect>();

        private const int ParticleSize = 25;

        /// <summary>
        /// Game state
        /// </summary>
        Bat _leftBat = null;
        Bat _rightBat = null;
        Ball _ball = null;
        Vector2 _arena = new Vector2();
        int leftScore = 0;
        int rightScore = 0;
        bool playing = false;
        System.Timers.Timer playTimer = new System.Timers.Timer();      // use to start/stop gameplay
        Message _score;
        float speed = 4f;
        List<Message> messages = new List<Message>();


        /// <summary>
        /// Screensaver state/mode
        /// </summary>
        Point MouseXY;
        bool screenSaverMode = false;
        bool fullScreen = false;
        int _screenNo = 0;


        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="screenNo">Screen to render to</param>
        public Spong(int screenNo, bool preview)
        {
            _screenNo = screenNo;
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);

            if (preview)
            {
                _arena.X = 100f;
                _arena.Y = 80f;
                this.FormBorderStyle = FormBorderStyle.None;
            }
            else
            {
#if false
                // Disable if we're debugging as this makes us fullscreen etc
                Bounds = Screen.AllScreens[0].Bounds;
                Cursor.Hide();
                TopMost = true;
                fullScreen = true;
                screenSaverMode = true;
                this.FormBorderStyle = FormBorderStyle.None;
#else
                fullScreen = false;
                screenSaverMode = false;
                this.FormBorderStyle = FormBorderStyle.Sizable;
#endif

                //
                // Set up our arena.
                //
                _arena.X = 800f;
                _arena.Y = 600f;
            }

            this.Width = (int)_arena.X;
            this.Height = (int)_arena.Y;
            this.Update();

            InitializeGraphics();

            //
            // Set up left bat.
            //
            _leftBat = new Bat();
            _leftBat.Direction = new Vector2(0f, 0f);
            _leftBat.Arena = _arena;
            _leftBat.MoveUpKey = Keys.W;
            _leftBat.MoveDownKey = Keys.S;
            _leftBat.Name = "Left Bat";
            _leftBat.AutoPilot = true;
            _leftBat.Speed = speed;

            //
            // Set up right bat.
            //
            _rightBat = new Bat();
            _rightBat.Direction = new Vector2(0f, 0f);
            _rightBat.Arena = _arena;
            _rightBat.MoveUpKey = Keys.Up;
            _rightBat.MoveDownKey = Keys.Down;
            _rightBat.Name = "Right Bat";
            _rightBat.AutoPilot = true;
            _rightBat.Speed = speed;

            //
            // Set up ball
            //
            _ball = new Ball();
            _ball.Arena = _arena;
            _ball.movables.Add(_leftBat);
            _ball.movables.Add(_rightBat);

            // Give references to the bats so they can track the ball's position.
            _rightBat.Ball = _ball;
            _leftBat.Ball = _ball;

            //
            // Give initial positions to all movables.
            //
            ResetGeography();

            //
            // Set up one particle effect
            //
            ParticleEffect effect = new ParticleEffect(ParticleSize, _arena);
            effect.Initialize();
            effects.Add(effect);
            
            //
            // Set up the events we want.
            //
            this.KeyDown += new KeyEventHandler(Spong_KeyDown);
            this.KeyUp += new KeyEventHandler(Spong_KeyUp);
            this.MouseMove += new MouseEventHandler(Spong_MouseMove);
            this.Paint += new PaintEventHandler(Spong_Paint);
            this.Resize += new EventHandler(Spong_Resize);
            _ball.Scored += new Ball.ScoreEventHandler(_ball_Scored);
            _ball.Intersected += new Ball.IntersectEventHandler(_ball_Intersected);

            // Our timer.
            playTimer.Elapsed += new System.Timers.ElapsedEventHandler(togglePlayTimer);

            //
            // Start the game.
            //
            playing = true;
            StartGame();
        }


        /// <summary>
        /// Sets the ball in motion in a random direction. 
        /// At startup and every time someone scored.
        /// </summary>
        public void StartGame()
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            int yDir = rnd.Next(99);
            int xDir = rnd.Next(99);
            _ball.Direction = new Vector2((yDir < 50 ? (speed * 2) : -(speed * 2)), (xDir < 50 ? speed : -speed));

            //
            // We actually ignore the current score and just overwrite with current time.
            //
            leftScore = DateTime.Now.Hour;
            rightScore = DateTime.Now.Minute;
            _score.Text = FormattedScore(leftScore, rightScore);
        }


        /// <summary>
        /// Just restores normality for movables.
        /// </summary>
        private void ResetGeography()
        {
            _leftBat.Width = _arena.X / 64;
            _leftBat.Height = _arena.Y / 10;
            _rightBat.Width = _arena.X / 64;
            _rightBat.Height = _arena.Y / 10;
            _ball.Width = _arena.X / 80;
            _ball.Height = _arena.Y / 64;

            _ball.Position = new Vector2((_arena.X / 2) - _ball.Width, (_arena.Y / 2) - (_ball.Height / 2));
            _rightBat.Position = new Vector2(_arena.X - 50 - _rightBat.Width, (_arena.Y / 2) - (_rightBat.Height / 2));
            _leftBat.Position = new Vector2(0 + 50, (_arena.Y / 2) - (_leftBat.Height / 2));

            _score = new Message(device,
                     new System.Drawing.Size((int)(_arena.Y / 16), (int)(_arena.X / 10)),
                     FontWeight.Normal, "Courier New", FormattedScore(leftScore, rightScore),
                     new System.Drawing.Point((int)(_arena.X / 3) + (int)(_arena.Y / 16), 0),
                     System.Drawing.Color.White);
        }


        private void LoadTextures()
        {
            System.IO.Stream s;
            
            s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Spong.Textures.generic.png");
            genericTexture = TextureLoader.FromStream(device, s, 320, 384,
                1, 0, Format.A8R8G8B8, Pool.Managed, Filter.Point, Filter.Point, (unchecked((int)0xff000000)));
            s.Close();

            Format current = Manager.Adapters[0].CurrentDisplayMode.Format;

            overlayTexture = new Texture(device, (int)_arena.X, (int)_arena.Y, 1, Usage.None, current, Pool.Managed);

            s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Spong.Textures.particle.line.png");
            particleTexture = TextureLoader.FromStream(device, s, 64, 38,
                1, 0, Format.A8R8G8B8, Pool.Managed, Filter.Point, Filter.Point, (unchecked((int)0xff000000)));
            s.Close();
        }


        private void SetUpViews()
        {
            device.Transform.Projection = 
                Matrix.PerspectiveFovLH(
                    (float)Math.PI / 4, 
                    this.Width / this.Height, 
                    0.1f, 
                    100.0f
                );

            device.RenderState.CullMode = Cull.None;
            device.RenderState.Lighting = false;
        }


        internal void InitializeGraphics()
        {
            try
            {
                PresentParameters presentParams = new PresentParameters();
                if (fullScreen == true)
                {
                    presentParams.Windowed = false;
                    presentParams.FullScreenRefreshRateInHz = 60;
                }
                else
                {
                    presentParams.Windowed = true;
                }

                presentParams.SwapEffect = SwapEffect.Discard;
                presentParams.BackBufferFormat = Format.R5G6B5;
                presentParams.BackBufferWidth = (int)_arena.X;
                presentParams.BackBufferHeight = (int)_arena.Y;
                presentParams.AutoDepthStencilFormat = DepthFormat.D16;
                presentParams.EnableAutoDepthStencil = true;
                presentParams.PresentationInterval = PresentInterval.One;

                // Store the default adapter
                int adapterOrdinal = Manager.Adapters.Default.Adapter;
                CreateFlags flags = CreateFlags.SoftwareVertexProcessing;

                // Check to see if we can use a pure hardware device
                Caps caps = Manager.GetDeviceCaps(adapterOrdinal, DeviceType.Hardware);

                // Do we support hardware vertex processing?
                if (caps.DeviceCaps.SupportsHardwareTransformAndLight)
                    // Replace the software vertex processing
                    flags = CreateFlags.HardwareVertexProcessing;

                // Do we support a pure device?
                if (caps.DeviceCaps.SupportsPureDevice)
                    flags |= CreateFlags.PureDevice;

                device = new Device(0, DeviceType.Hardware, this, flags, presentParams);
                device.DeviceReset += new System.EventHandler(this.OnResetDevice);

                OnResetDevice(device, null);

#if true
                device.RenderState.AlphaBlendEnable = true;
                device.RenderState.SourceBlend = Blend.BothSourceAlpha;
                device.RenderState.DestinationBlend = Blend.BothInvSourceAlpha;
#endif

                vertexBuffer = new VertexBuffer(typeof(CustomVertex), numVerts, device, Usage.WriteOnly, customVertexFlags, Pool.Default);
                vertexBuffer.Created += new System.EventHandler(this.OnCreateVertexBuffer);
                OnCreateVertexBuffer(vertexBuffer, null);

                // set up sprites
                particleSprite = new Sprite(device);

                SetUpViews();
            }
            catch (DirectXException e)
            {
                System.Diagnostics.Debug.Print("exception: " + e.Message);
            }
        }


        /// <summary>
        /// See this as the main loop.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Spong_Paint(object sender, PaintEventArgs e)
        {
#if false    // Normal rendering (full screen clear)
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Green, 1.0f, 0);
            device.BeginScene();

            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.DestinationBlend = Blend.DestinationAlpha;
#else       // motion blur (partial clear) enabled
            device.BeginScene();

            device.RenderState.SourceBlend = Blend.BothSourceAlpha;
            device.RenderState.DestinationBlend = Blend.BothInvSourceAlpha;

            // This will paint screen black
            particleSprite.Begin(SpriteFlags.SortTexture);
            particleSprite.Draw2D(overlayTexture, Point.Empty, 0, Point.Empty, Color.FromArgb(40, 0,0,0));
            particleSprite.End();

            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.DestinationBlend = Blend.DestinationAlpha;
#endif
            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].Refresh(device, particleSprite, particleTexture);
            }

            _leftBat.Refresh(device, genericTexture);
            _rightBat.Refresh(device, genericTexture);
            _ball.Refresh(device, genericTexture);

            _score.Refresh(null);
            for (int i = 0; i < messages.Count; i++)
            {
                messages[i].Refresh(null);
            }

            // Forcefully stop movables from moving.
            if (playing)
            {
                _leftBat.PerformMove();
                _rightBat.PerformMove();
                _ball.PerformMove();

                if (leftScore < DateTime.Now.Hour)
                {
                    _rightBat.MissNext = true;
                }

                // Do not miss once an hour.
                if (rightScore < DateTime.Now.Minute && _rightBat.MissNext == false)
                {
                    _leftBat.MissNext = true;
                }

            }

            device.EndScene();
            device.Present();
            this.Invalidate();
        }


        void ExitSpong()
        {
            if (this.Disposing == false)
            {
                if (components != null)
                    components.Dispose();
                this.Dispose();
            }
            this.Close();
        }


        private void ShowStatus(string t)
        {
        }


        private string FormattedScore(int leftScore, int rightScore)
        {
            return (leftScore.ToString().Length == 1 ? "0" + leftScore : "" + leftScore) + ":" + (rightScore.ToString().Length == 1 ? "0" + rightScore : "" + rightScore);
        }


        public void OnResetDevice(object sender, EventArgs e)
        {
            Device device = (Device)sender;
            device.RenderState.CullMode = Cull.None;
            device.RenderState.Lighting = false;

            LoadTextures();
            SetUpViews();
        }


        public void OnCreateVertexBuffer(object sender, EventArgs e)
        {
            CustomVertex[] vertices;
            vertexBuffer = new VertexBuffer(typeof(CustomVertex),
                numVerts, device, Usage.WriteOnly, customVertexFlags, Pool.Default);
            vertices = vertexBuffer.Lock(0, 0) as CustomVertex[];
            vertices[0] = new CustomVertex(-1.0f, 1.0f, 0.0f, 0.0f, 0.0f);
            vertices[1] = new CustomVertex(1.0f, 1.0f, 0.0f, 1.0f, 0.0f);
            vertices[2] = new CustomVertex(-1.0f, -1.0f, 0.0f, 0.0f, 1.0f);
            vertices[3] = new CustomVertex(1.0f, -1.0f, 0.0f, 1.0f, 1.0f);
            vertexBuffer.Unlock();
        }


        void Spong_MouseMove(object sender, MouseEventArgs e)
        {
            if (screenSaverMode == true)
            {
                if (!MouseXY.IsEmpty)
                {
                    if (MouseXY != new Point(e.X, e.Y))
                        ExitSpong();
                    if (e.Clicks > 0)
                        ExitSpong();
                }
            }
            MouseXY = new Point(e.X, e.Y);
        }


        void _ball_Intersected(object source, Vector2 position)
        {
            ParticleEffect effect = null;
            for (int i = 0; i < effects.Count; i++)
            {
                if (!effects[i].isAlive())
                {
                    effect = effects[i];
                    break;
                }
            }

            // Looks like all assigned effects are busy, create a new one 
            // and add it to list of available effects.
            if (effect == null)
            {
                effect = new ParticleEffect(ParticleSize, _arena);
                effect.Initialize();
                effects.Add(effect);
            }

            effect.Reset(_ball.Position, _ball.Direction);

            System.Diagnostics.Debug.Print("Intersection!");
        }


        void _ball_Scored(object source, Vector2 position)
        {
            // A more exact check is done in ball.
            if (position.X < (_arena.X / 2))
            {
                if (_leftBat.MissNext == true)
                {
                    System.Diagnostics.Debug.Print("Left was told to miss, and did so...");
                    _leftBat.MissNext = false;
                }

                System.Diagnostics.Debug.Print("Right scored");
                rightScore++;
            }
            else
            {
                if (_rightBat.MissNext == true)
                {
                    System.Diagnostics.Debug.Print("Right was told to miss, and did so...");
                    _rightBat.MissNext = false;
                }

                System.Diagnostics.Debug.Print("Left scored");
                leftScore++;
            }

            _ball.Direction = new Vector2(0f, 0f);
            ResetGeography();

            stopPlay(5);
            ShowStatus("point");

            StartGame();
        }


        void togglePlayTimer(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.Print("startPlay");
            playing = true;
            playTimer.Stop();
            playTimer.Enabled = false;
        }


        void stopPlay(int seconds)
        {
            if (playTimer.Enabled == true)
                return;

            System.Diagnostics.Debug.Print("stopPlay");
            playing = false;
            playTimer.Interval = seconds * 1000;
            playTimer.Enabled = true;
            playTimer.Start();
        }


        private void Spong_Resize(object sender, EventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
            _arena.X = this.ClientSize.Width;
            _arena.Y = this.ClientSize.Height;
            ResetGeography();
        }

        
        private void Spong_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.Print("key");

            // Take control over left bat.
            if (e.KeyCode == Keys.F1)
            {
                _leftBat.AutoPilot = !_leftBat.AutoPilot;
                _leftBat.Direction.Y = 0f;
            }

            // Take control over right bat.
            if (e.KeyCode == Keys.F2)
            {
                _rightBat.AutoPilot = !_rightBat.AutoPilot;
                _rightBat.Direction.Y = 0f;
            }

            if (e.KeyCode == _leftBat.MoveDownKey || e.KeyCode == _leftBat.MoveUpKey)
                _leftBat.KeyDown(e);

            if (e.KeyCode == _rightBat.MoveDownKey || e.KeyCode == _rightBat.MoveUpKey)
                _rightBat.KeyDown(e);

            if (e.KeyCode == Keys.Escape)
            {
                ExitSpong();
                return;
            }
        
        }


        private void Spong_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == _leftBat.MoveDownKey || e.KeyCode == _leftBat.MoveUpKey)
                _leftBat.KeyUp(e);

            if (e.KeyCode == _rightBat.MoveDownKey || e.KeyCode == _rightBat.MoveUpKey)
                _rightBat.KeyUp(e);
        }


    }
}
