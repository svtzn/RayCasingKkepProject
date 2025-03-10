using System.Drawing;

namespace RayCasingKkepProject
{
    public static class TextureManager
    {
        public static Bitmap BrickTexture { get; }
        public static Bitmap DoorTexture { get; }
        public static Bitmap PortalTexture { get; }

        // Новые поля:
        public static Bitmap FloorTexture { get; }
        public static Bitmap CeilingTexture { get; }

        static TextureManager()
        {
            // Проверка существования файлов — подставьте свои реальные пути/названия:
            // (Ниже просто пример, подкорректируйте под свои файлы!)
            if (!System.IO.File.Exists("ceil3_3.png") ||
                !System.IO.File.Exists("dem1_5.png") ||
                !System.IO.File.Exists("Door.png") ||
                !System.IO.File.Exists("W3d_finalgrayflag.png") ||
                !System.IO.File.Exists("FLAT1_2.png"))
            {
                throw new System.IO.FileNotFoundException("Текстуры не найдены!");
            }

            // Загрузка стен/порталов
            BrickTexture = new Bitmap("ceil3_3.png");
            DoorTexture = new Bitmap("Door.png");
            PortalTexture = new Bitmap("W3d_finalgrayflag.png");

            // Загрузка пола и потолка
            FloorTexture = new Bitmap("FLAT1_2.png");
            CeilingTexture = new Bitmap("dem1_5.png");
        }
    }
}
