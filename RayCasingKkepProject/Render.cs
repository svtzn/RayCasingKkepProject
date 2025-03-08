using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RayCasingKkepProject
{
    public class Renderer
    {
        private Bitmap bmp;
        private Graphics g;
        private Player player;
        private int screenWidth;
        private int screenHeight;
        private int[] backBuffer;
        private GCHandle backBufferHandle;

        public Renderer(int width, int height, Player player)
        {
            screenWidth = width;
            screenHeight = height;
            bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            g = Graphics.FromImage(bmp);
            this.player = player;
            backBuffer = new int[width * height];
            backBufferHandle = GCHandle.Alloc(backBuffer, GCHandleType.Pinned);
        }

        public Bitmap RenderFrame()
        {
            Array.Clear(backBuffer, 0, backBuffer.Length);

            int horizon = screenHeight / 2 - (int)(player.Pitch * screenHeight * 0.5);
            DrawFloorAndCeiling(horizon);
            DrawWalls(horizon);
            DrawMinimap();

            var bmpData = bmp.LockBits(new Rectangle(0, 0, screenWidth, screenHeight),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(backBuffer, 0, bmpData.Scan0, backBuffer.Length);
            bmp.UnlockBits(bmpData);
                
            return bmp;
        }

        private void DrawFloorAndCeiling(int horizon)
        {
            // Задаем максимальное расстояние для затемнения (подбирай по вкусу)
            float maxDistance = 15f;
            // Цвета для потолка и пола (базовые)
            Color ceilingColor = Color.FromArgb(100, 100, 100);
            Color floorColor = Color.FromArgb(139, 69, 19);

            // Проходим по всем строкам экрана
            for (int y = 0; y < screenHeight; y++)
            {
                // Будем вычислять затемнение отдельно для потолка и пола
                Color baseColor;
                float rowDistance;
                if (y < horizon)
                {
                    // Потолок: чем ближе к краю горизонта сверху, тем дальше (и темнее)
                    rowDistance = (screenHeight / 2f) / (horizon - y);
                    baseColor = ceilingColor;
                }
                else
                {
                    // Пол: чем дальше от горизонта вниз, тем дальше (и темнее)
                    // Добавляем +1 чтобы не было деления на 0
                    rowDistance = (screenHeight / 2f) / (y - horizon + 1);
                    baseColor = floorColor;
                }

                // Вычисляем коэффициент затемнения
                float shade = 1.0f - Math.Min(rowDistance / maxDistance, 1.0f);

                // Применяем затемнение к базовому цвету
                int r = (int)(baseColor.R * shade);
                int g = (int)(baseColor.G * shade);
                int b = (int)(baseColor.B * shade);
                Color finalColor = Color.FromArgb(255, r, g, b);

                // Заполняем строку этим цветом
                for (int x = 0; x < screenWidth; x++)
                {
                    backBuffer[y * screenWidth + x] = finalColor.ToArgb();
                }
            }
        }

        private void DrawWalls(int horizon)
        {
            Bitmap brickCopy = new Bitmap(TextureManager.BrickTexture);
            Bitmap doorCopy = new Bitmap(TextureManager.DoorTexture);

            byte[] brickData = GetTextureData(TextureManager.BrickTexture);
            byte[] doorData = GetTextureData(TextureManager.DoorTexture);
            int brickWidth = TextureManager.BrickTexture.Width;
            int brickHeight = TextureManager.BrickTexture.Height;
            int doorWidth = TextureManager.DoorTexture.Width;
            int doorHeight = TextureManager.DoorTexture.Height;


            Parallel.For(0, screenWidth, x =>
            {
                float rayAngle = player.Angle - 0.5f + (x / (float)screenWidth);
                var (distance, hitX, hitY, isDoor) = Raycaster.CastRay(player.X, player.Y, rayAngle, player);
                bool isDoorOpen = isDoor && Map.IsDoorOpen(hitX, hitY);


                byte[] textureData = isDoor ? doorData : brickData;
                int textureWidth = isDoor ? doorWidth : brickWidth;
                int textureHeight = isDoor ? doorHeight : brickHeight;

                //if (isDoorOpen)
                //{
                //    textureData = brickData;
                //    textureWidth = brickWidth;
                //    textureHeight = brickHeight;
                //}

                float wallX = GetWallTextureX(hitX, hitY, player.X, player.Y, rayAngle, distance);

                if (isDoor && !isDoorOpen)
                {
                    wallX = 0.3f + wallX * 0.4f; // Сжимаем текстуру двери, чтобы она выглядела тоньше
                }

                DrawTexturedStrip(x, distance, textureData, textureWidth, textureHeight, horizon, wallX);
            });


            brickCopy.Dispose();
            doorCopy.Dispose();
        }




        private float GetWallTextureX(int hitX, int hitY, float startX, float startY, float angle, float distance)
        {
            float rayDirX = (float)Math.Cos(angle);
            float rayDirY = (float)Math.Sin(angle);
            float wallX;

            // Определяем, какая сторона стены была поражена:
            // Если абсолютное значение горизонтальной компоненты направления больше вертикальной,
            // значит, мы столкнулись с вертикальной стеной.
            if (Math.Abs(rayDirX) > Math.Abs(rayDirY))
            {
                // вертикальная стена – используем координату y
                wallX = startY + distance * rayDirY;
            }
            else
            {
                // горизонтальная стена – используем координату x
                wallX = startX + distance * rayDirX;
            }

            // Возвращаем дробную часть (это и есть позиция на текстуре)
            return wallX - (float)Math.Floor(wallX);
        }


        private void DrawTexturedStrip(int x, float distance, byte[] textureData, int textureWidth, int textureHeight, int horizon, float wallX)
        {
            // Используем корректное расстояние до проекционной плоскости для устранения эффекта рыбьего глаза:
            float fov = (float)(60 * Math.PI / 180);
            float projectionPlaneDistance = (screenWidth / 2f) / (float)Math.Tan(fov / 2f);
            int lineHeight = (int)(projectionPlaneDistance / distance);

            int startY = Math.Max(0, horizon - lineHeight / 2);
            int endY = Math.Min(screenHeight - 1, startY + lineHeight);

            int textureX = (int)(wallX * textureWidth);
            textureX = Utils.Clamp(textureX, 0, textureWidth - 1);

            // Задаем максимальное расстояние для затемнения
            float maxDistance = 15f;

            for (int y = startY; y < endY; y++)
            {
                float texPosY = ((y - startY) / (float)lineHeight) * textureHeight;
                int textureY = Math.Max(0, Math.Min(textureHeight - 1, (int)texPosY));

                Color color = GetPixelFromTextureData(textureData, textureX, textureY, textureWidth);

                // Вычисляем коэффициент затемнения (от 1 до 0)
                float shade = 1.0f - Math.Min(distance / maxDistance, 1.0f);

                // Применяем затемнение к цвету:
                int r = (int)(color.R * shade);
                int g = (int)(color.G * shade);
                int b = (int)(color.B * shade);

                color = Color.FromArgb(255, r, g, b);

                backBuffer[y * screenWidth + x] = color.ToArgb();
            }
        }



        private static Dictionary<Bitmap, byte[]> textureCache = new Dictionary<Bitmap, byte[]>();

        private byte[] GetTextureData(Bitmap texture)
        {
            if (!textureCache.TryGetValue(texture, out byte[] data))
            {
                var rect = new Rectangle(0, 0, texture.Width, texture.Height);
                var bitmapData = texture.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                data = new byte[bitmapData.Stride * bitmapData.Height];
                Marshal.Copy(bitmapData.Scan0, data, 0, data.Length);
                texture.UnlockBits(bitmapData);

                textureCache[texture] = data;
            }
            return data;
        }


        private Color GetPixelFromTextureData(byte[] textureData, int x, int y, int textureWidth)
        {
            int index = y * textureWidth * 4 + x * 4; // 4 байта на пиксель (ARGB)
            return Color.FromArgb(
                textureData[index + 3], // A
                textureData[index + 2], // R
                textureData[index + 1], // G
                textureData[index]     // B
            );
        }

        private void SetPixel(int x, int y, Color color)
        {
            if (x >= 0 && x < screenWidth && y >= 0 && y < screenHeight)
                backBuffer[y * screenWidth + x] = color.ToArgb();
        }


        private void DrawMinimap()
        {
            int cellSize = 10;
            for (int y = 0; y < Map.Height; y++)
            {
                for (int x = 0; x < Map.Width; x++)
                {
                    Color color = Map.IsWall(x, y) ? Color.White : Color.Black;
                    for (int i = 0; i < cellSize; i++)
                    {
                        for (int j = 0; j < cellSize; j++)
                        {
                            SetPixel(x * cellSize + i, y * cellSize + j, color);
                        }
                    }
                }
            }

            // Отображаем игрока
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    SetPixel((int)(player.X * 10) + i, (int)(player.Y * 10) + j, Color.Red);
                }
            }
        }

    }
}