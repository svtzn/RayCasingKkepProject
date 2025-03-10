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
                case 0:
                    // Локация 0 – набор текстур по умолчанию
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

                case 1:
                    // Локация 1 – другой набор текстур
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

                case 2:
                    // Для остальных локаций можно задать текстуры по умолчанию
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
                case 3:
                    // Для остальных локаций можно задать текстуры по умолчанию
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
