using System.Drawing;
using System.IO;

namespace RayCasingKkepProject
{
    public static class TextureManager
    {
        // Свойства стали с возможностью изменения
        public static Bitmap BrickTexture { get; set; }
        public static Bitmap DoorTexture { get; set; }
        public static Bitmap PortalTexture { get; set; }
        public static Bitmap FloorTexture { get; set; }
        public static Bitmap CeilingTexture { get; set; }

        // Метод загрузки текстур для заданной локации (например, по индексу)
        public static void LoadLocationTextures(int location)
        {
            // Здесь вы можете задать имена файлов для каждой локации.
            // Ниже пример для двух локаций. Добавьте нужное количество по аналогии.
            switch (location)
            {
                // 1st location
                case 0:
                    if (!File.Exists("ceil3_3.png") ||
                        !File.Exists("Door.png") ||
                        !File.Exists("W3d_finalgrayflag.png") ||
                        !File.Exists("FLAT1_2.png") ||
                        !File.Exists("dem1_5.png"))
                    {
                        throw new FileNotFoundException("Текстуры для локации 0 не найдены!");
                    }
                    BrickTexture = new Bitmap("ceil3_3.png");
                    DoorTexture = new Bitmap("Door.png");
                    PortalTexture = new Bitmap("W3d_finalgrayflag.png");
                    FloorTexture = new Bitmap("FLAT1_2.png");
                    CeilingTexture = new Bitmap("dem1_5.png");
                    break;

                // 2nd location
                case 1:
                    if (!File.Exists("ceil3_3.png") ||
                        !File.Exists("Door.png") ||
                        !File.Exists("W3d_finalgrayflag.png") ||
                        !File.Exists("FLAT1_2.png") ||
                        !File.Exists("dem1_5.png"))
                    {
                        throw new FileNotFoundException("Текстуры для локации 0 не найдены!");
                    }
                    BrickTexture = new Bitmap("mosaik.png");
                    DoorTexture = new Bitmap("Door.png");
                    PortalTexture = new Bitmap("W3d_finalgrayflag.png");
                    FloorTexture = new Bitmap("FLAT1_2.png");
                    CeilingTexture = new Bitmap("dem1_5.png");
                    break;

                // 3nd location
                case 2:
                    if(!File.Exists("ceil3_3.png") ||
                        !File.Exists("Door.png") ||
                        !File.Exists("W3d_finalgrayflag.png") ||
                        !File.Exists("FLAT1_2.png") ||
                        !File.Exists("dem1_5.png"))
                    {
                        throw new FileNotFoundException("Текстуры для локации 0 не найдены!");
                    }
                    BrickTexture = new Bitmap("ceil3_3.png");
                    DoorTexture = new Bitmap("Door.png");
                    PortalTexture = new Bitmap("W3d_finalgrayflag.png");
                    FloorTexture = new Bitmap("FLAT1_2.png");
                    CeilingTexture = new Bitmap("dem1_5.png");
                    break;

                // 4th location
                case 3:
                    if (!File.Exists("ceil3_3.png") ||
                        !File.Exists("Door.png") ||
                        !File.Exists("W3d_finalgrayflag.png") ||
                        !File.Exists("FLAT1_2.png") ||
                        !File.Exists("dem1_5.png"))
                    {
                        throw new FileNotFoundException("Текстуры для локации 0 не найдены!");
                    }
                    BrickTexture = new Bitmap("ceil3_3.png");
                    DoorTexture = new Bitmap("Door.png");
                    PortalTexture = new Bitmap("W3d_finalgrayflag.png");
                    FloorTexture = new Bitmap("FLAT1_2.png");
                    CeilingTexture = new Bitmap("dem1_5.png");
                    break;
            }
        }
    }
}
