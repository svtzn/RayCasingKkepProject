using System;
using System.Windows.Forms;

namespace RayCasingKkepProject
{
    public partial class LocationMenu : Form
    {
        private Button[] buttons;
        public int SelectedLocation { get; private set; }

        public LocationMenu()
        {
            Text = "Выберите локацию";
            Size = new System.Drawing.Size(300, 250);
            buttons = new Button[4];

            for (int i = 0; i < 4; i++)
            {
                buttons[i] = new Button
                {
                    Text = $"Локация {i + 1}",
                    Size = new System.Drawing.Size(200, 40),
                    Location = new System.Drawing.Point(50, 30 + i * 50)
                };

                int locationIndex = i;
                buttons[i].Click += (sender, e) => { SelectedLocation = locationIndex; DialogResult = DialogResult.OK; };
                Controls.Add(buttons[i]);
            }
        }
    }
}
