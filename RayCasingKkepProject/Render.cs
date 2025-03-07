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
            for (int y = 0; y < screenHeight; y++)
            {
                int color = y < horizon
                    ? Color.FromArgb(100, 100, 100).ToArgb()
                    : Color.FromArgb(139, 69, 19).ToArgb();

                for (int x = 0; x < screenWidth; x++)
                {
                    backBuffer[y * screenWidth + x] = color;
                }
            }
        }

        private void DrawWalls(int horizon)
        {
            Bitmap brickCopy = new Bitmap(TextureManager.BrickTexture);
            Bitmap doorCopy = new Bitmap(TextureManager.DoorTexture);

            // Загружаем данные текстур в массивы (чтобы не использовать Bitmap в потоках)
            byte[] brickData = GetTextureData(TextureManager.BrickTexture);
            byte[] doorData = GetTextureData(TextureManager.DoorTexture);
            int brickWidth = TextureManager.BrickTexture.Width;
            int brickHeight = TextureManager.BrickTexture.Height;
            int doorWidth = TextureManager.DoorTexture.Width;
            int doorHeight = TextureManager.DoorTexture.Height;


            Parallel.For(0, screenWidth, x =>
            {
                float rayAngle = player.Angle - 0.5f + (x / (float)screenWidth);
                var (distance, hitX, hitY) = Raycaster.CastRay(player.X, player.Y, rayAngle, player);

                bool isDoor = Map.IsDoor(hitX, hitY) && !Map.IsDoorOpen(hitX, hitY);
                byte[] textureData = isDoor ? doorData : brickData;
                int textureWidth = isDoor ? doorWidth : brickWidth;
                int textureHeight = isDoor ? doorHeight : brickHeight;

                float wallX = GetWallTextureX(hitX, hitY, player.X, player.Y, rayAngle, distance);
                DrawTexturedStrip(x, distance, textureData, textureWidth, textureHeight, horizon, wallX);
            });

            brickCopy.Dispose();
            doorCopy.Dispose();
        }




        private float GetWallTextureX(int hitX, int hitY, float startX, float startY, float angle, float distance)
        {
            float rayDirX = (float)Math.Cos(angle);
            float rayDirY = (float)Math.Sin(angle);

            float wallX = Math.Abs((startX + rayDirX * distance) - hitX) > 0.01f
                ? startY + rayDirY * (hitX - startX) / rayDirX
                : startX + rayDirX * (hitY - startY) / rayDirY;

            return wallX - (float)Math.Floor(wallX);
        }

        private void DrawTexturedStrip(int x, float distance, byte[] textureData, int textureWidth, int textureHeight, int horizon, float wallX)
        {
            int lineHeight = (int)(screenHeight / distance);
            int startY = Math.Max(0, horizon - lineHeight / 2);
            int endY = Math.Min(screenHeight - 1, startY + lineHeight);

            int textureX = (int)(wallX * textureWidth);
            textureX = Utils.Clamp(textureX, 0, textureWidth - 1);

            for (int y = startY; y < endY; y++)
            {
                float texPosY = ((y - startY) / (float)lineHeight) * textureHeight;
                int textureY = (int)texPosY % textureHeight;

                Color color = GetPixelFromTextureData(textureData, textureX, textureY, textureWidth);
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

        private void DrawMinimap()
        {
            using (var g = Graphics.FromImage(bmp))
            {
                int cellSize = 10;
                for (int y = 0; y < Map.Height; y++)
                {
                    for (int x = 0; x < Map.Width; x++)
                    {
                        var brush = Map.IsWall(x, y) ? Brushes.White : Brushes.Black;
                        g.FillRectangle(brush, x * cellSize, y * cellSize, cellSize, cellSize);
                    }
                }
                g.FillRectangle(Brushes.Red, (int)(player.X * 10), (int)(player.Y * 10), 5, 5);
            }
        }
    }
}