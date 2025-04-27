using System;
using System.Drawing;
using System.Windows.Forms;

namespace RayCasingKkepProject
{
    public partial class MainForm : Form
    {
        private readonly Renderer renderer;
        private readonly GameLoop gameLoop;
        private readonly Player player;
        private PictureBox screen;

        private Point lastMousePosition;
        private bool isMouseInitialized = false;
        private bool mouseCaptured = true;
        private float mouseSensitivityX = 0.001f;
        private float mouseSensitivityY = 0.001f;

        private Cursor invisibleCursor;

        public MainForm()
        {
            try
            {
                InitializeComponent();
                screen = new PictureBox { Dock = DockStyle.Fill };
                Controls.Add(screen);

                player = new Player(3, 3, 0); // Начальная позиция
                renderer = new Renderer(Width, Height, player);

                MapManager.LoadMap(0);

                gameLoop = new GameLoop(UpdateFrame);
                gameLoop.Start();

                Bitmap bmp = new Bitmap(1, 1);
                invisibleCursor = new Cursor(bmp.GetHicon());
            }
            catch (System.IO.FileNotFoundException ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private void HandleMouseLook()
        {
            if (!mouseCaptured) return;
            Point center = this.PointToScreen(new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2));
            if (!isMouseInitialized)
            {
                Cursor.Position = center;
                lastMousePosition = center;
                isMouseInitialized = true;
            }
            Point current = Cursor.Position;
            int deltaX = current.X - lastMousePosition.X;
            int deltaY = current.Y - lastMousePosition.Y;

            player.Angle += deltaX * mouseSensitivityX;
            player.Pitch += deltaY * mouseSensitivityY;

            player.Pitch = Utils.Clamp(player.Pitch, -0.5f, 0.5f);

            Cursor.Position = center;
            lastMousePosition = center;
        }

        private void UpdateFrame()
        {
            if (mouseCaptured)
                HandleMouseLook();

            player.Update();
            screen.Image = renderer.RenderFrame();
            screen.Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.W: player.IsMovingForward = true; break;
                case Keys.S: player.IsMovingBackward = true; break;
                case Keys.A: player.IsStrafingLeft = true; break;
                case Keys.D: player.IsStrafingRight = true; break;
                case Keys.E: player.Interact(); break;
                case Keys.Escape:
                    ToggleMouseCapture();
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            switch (e.KeyCode)
            {
                case Keys.W: player.IsMovingForward = false; break;
                case Keys.S: player.IsMovingBackward = false; break;
                case Keys.A: player.IsStrafingLeft = false; break;
                case Keys.D: player.IsStrafingRight = false; break;
            }
        }

        private void ToggleMouseCapture()
        {
            mouseCaptured = !mouseCaptured;

            if (mouseCaptured)
            {
                this.Cursor = invisibleCursor;
                screen.Cursor = invisibleCursor;

                Cursor.Clip = this.Bounds;
                isMouseInitialized = false;
            }
            else
            {
                this.Cursor = Cursors.Default;
                screen.Cursor = Cursors.Default;

                Cursor.Clip = Rectangle.Empty;
            }
        }
    }
}