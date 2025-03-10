﻿using System.Drawing;

namespace RayCasingKkepProject
{
    public static class TextureManager
    {
        public static Bitmap BrickTexture { get; }
        public static Bitmap DoorTexture { get; }
        public static Bitmap PortalTexture { get; }
        

        static TextureManager()
        {
            // Проверка существования файлов
            if (!System.IO.File.Exists("W3d_protoredbrick1.png") || !System.IO.File.Exists("W3d_finalgrayflag.png"))
                throw new System.IO.FileNotFoundException("Текстуры не найдены!");

            BrickTexture = new Bitmap("ceil3_3.png");
            DoorTexture = new Bitmap("Door.png");
            PortalTexture = new Bitmap("W3d_finalgrayflag.png");
        }
    }
}