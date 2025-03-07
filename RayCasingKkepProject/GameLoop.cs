using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayCasingKkepProject
{
    public class GameLoop
    {
        private readonly Timer timer;
        private readonly Action onUpdate;

        public GameLoop(Action updateMethod, int interval = 16)
        {
            onUpdate = updateMethod;
            timer = new Timer { Interval = interval };
            timer.Tick += (s, e) => onUpdate();
        }

        public void Start() => timer.Start();
        public void Stop() => timer.Stop();

    }
}