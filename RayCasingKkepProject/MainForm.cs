using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayCasingKkepProject
{
    public partial class MainForm : Form
    {
        private readonly Renderer renderer;
        private readonly GameLoop gameLoop;
        private readonly Player player;
        private PictureBox screen;

        public MainForm()
        {
            try
            {
                InitializeComponent();
                screen = new PictureBox { Dock = DockStyle.Fill };
                Controls.Add(screen);

                player = new Player(3, 3, 0); // Начальная позиция
                renderer = new Renderer(Width, Height, player);
                gameLoop = new GameLoop(UpdateFrame);
                gameLoop.Start();
            }
            catch (System.IO.FileNotFoundException ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
                Environment.Exit(1);
            }

        }

        private void UpdateFrame()
        {
            screen.Image = renderer.RenderFrame();
            screen.Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.W: player.MoveForward(); break;
                case Keys.S: player.MoveBackward(); break;
                case Keys.A: player.StrafeLeft(); break;  //стрейф влево
                case Keys.D: player.StrafeRight(); break; //стрейф вправо
                case Keys.Left: player.RotateLeft(); break; // поворот камеры влево
                case Keys.Right: player.RotateRight(); break; // поворот камеры вправо
                case Keys.Up: player.LookUp(); break;
                case Keys.Down: player.LookDown(); break;
                case Keys.E: player.Interact(); break;
            }
        }

    }
}