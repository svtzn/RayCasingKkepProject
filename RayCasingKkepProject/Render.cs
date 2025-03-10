using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

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
            DrawWalls();
            DrawMinimap();

            var bmpData = bmp.LockBits(new Rectangle(0, 0, screenWidth, screenHeight),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(backBuffer, 0, bmpData.Scan0, backBuffer.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private void DrawFloorAndCeiling(int horizon)
        {
            float maxDistance = 15f;
            Color ceilingColor = Color.FromArgb(100, 100, 100);
            Color floorColor = Color.FromArgb(139, 69, 19);

            for (int y = 0; y < screenHeight; y++)
            {
                Color baseColor;
                float rowDistance;
                if (y < horizon)
                {
                    rowDistance = (screenHeight / 2f) / (horizon - y);
                    baseColor = ceilingColor;
                }
                else
                {
                    rowDistance = (screenHeight / 2f) / (y - horizon + 1);
                    baseColor = floorColor;
                }

                float shade = 1.0f - Math.Min(rowDistance / maxDistance, 1.0f);

                int r = (int)(baseColor.R * shade);
                int g = (int)(baseColor.G * shade);
                int b = (int)(baseColor.B * shade);
                Color finalColor = Color.FromArgb(255, r, g, b);

                for (int x = 0; x < screenWidth; x++)
                {
                    backBuffer[y * screenWidth + x] = finalColor.ToArgb();
                }
            }
        }

        private void DrawWalls()
        {
            float fov = (float)(60 * Math.PI / 180);
            float rayDirStep = fov / screenWidth;
            float rayAngleStart = player.Angle - fov / 2f;

            // Предварительно получим данные о текстурах
            byte[] brickData = GetTextureData(TextureManager.BrickTexture);
            byte[] doorData = GetTextureData(TextureManager.DoorTexture);
            byte[] portalData = GetTextureData(TextureManager.PortalTexture);

            int brickW = TextureManager.BrickTexture.Width;
            int brickH = TextureManager.BrickTexture.Height;
            int doorW = TextureManager.DoorTexture.Width;
            int doorH = TextureManager.DoorTexture.Height;
            int portalW = TextureManager.PortalTexture.Width;
            int portalH = TextureManager.PortalTexture.Height;

            // Расстояние до плоскости проекции
            float projectionPlaneDistance = (screenWidth / 2f) / (float)Math.Tan(fov / 2f);
            float centerY = screenHeight / 2 - (player.Pitch * screenHeight * 0.5f);
            float cameraHeight = 0.5f;
            float wallTop = 1.0f;
            float wallBottom = 0.0f;
            float maxDistance = 15f;

            for (int x = 0; x < screenWidth; x++)
            {
                float rayAngle = rayAngleStart + x * rayDirStep;
                // Запускаем луч
                var (dist, hitX, hitY, isDoor, side) = Raycaster.CastRay(player.X, player.Y, rayAngle, player);

                // Определяем текстуру
                bool isPortal = Map.IsPortal(hitX, hitY);
                byte[] texData;
                int texW, texH;
                if (isPortal)
                {
                    texData = portalData;
                    texW = portalW;
                    texH = portalH;
                }
                else if (isDoor)
                {
                    texData = doorData;
                    texW = doorW;
                    texH = doorH;
                }
                else
                {
                    texData = brickData;
                    texW = brickW;
                    texH = brickH;
                }

                // Вычисляем координату текстуры (wallX) в зависимости от того, вертикальная или горизонтальная стена
                float rayDirX = (float)Math.Cos(rayAngle);
                float rayDirY = (float)Math.Sin(rayAngle);

                float wallX;
                if (side == 0)
                {
                    // Пересекли вертикальную грань
                    wallX = player.Y + dist * rayDirY;
                }
                else
                {
                    // Горизонтальную
                    wallX = player.X + dist * rayDirX;
                }
                wallX -= (float)Math.Floor(wallX);

                // Проецируем верх и низ стены
                float projTop = centerY - (projectionPlaneDistance * (wallTop - cameraHeight) / dist);
                float projBottom = centerY - (projectionPlaneDistance * (wallBottom - cameraHeight) / dist);

                int startY = (int)projTop;
                int endY = (int)projBottom;

                // Ограничиваем экраном
                if (startY < 0) startY = 0;
                if (endY >= screenHeight) endY = screenHeight - 1;

                // Если endY < startY, значит стена за экраном
                if (endY < startY) continue;

                // Затемнение по расстоянию
                float shade = 1.0f - Math.Min(dist / maxDistance, 1.0f);

                // Рисуем столбец
                // Высота на экране
                float columnHeight = (projBottom - projTop);

                for (int yPix = startY; yPix <= endY; yPix++)
                {
                    // Относительная позиция (0..1) по вертикали
                    float relY = (yPix - projTop) / columnHeight;

                    // Координаты текстуры (без повторения, просто зажимаем)
                    int texX = (int)(wallX * texW);
                    if (texX >= texW) texX = texW - 1;
                    if (texX < 0) texX = 0;

                    int texY = (int)(relY * texH);
                    if (texY >= texH) texY = texH - 1;
                    if (texY < 0) texY = 0;

                    // Берём пиксель из текстуры
                    Color color = GetPixelFromTextureData(texData, texX, texY, texW);

                    // Применяем затемнение
                    int r = (int)(color.R * shade);
                    int g = (int)(color.G * shade);
                    int b = (int)(color.B * shade);
                    color = Color.FromArgb(255, r, g, b);

                    backBuffer[yPix * screenWidth + x] = color.ToArgb();
                }
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
            int index = y * textureWidth * 4 + x * 4;
            return Color.FromArgb(
                textureData[index + 3],
                textureData[index + 2],
                textureData[index + 1],
                textureData[index]
            );
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
                    Color portalColor = Color.Purple;
                    if (Map.IsPortal(x, y))
                    {
                        for (int i = 0; i < cellSize; i++)
                        {
                            for (int j = 0; j < cellSize; j++)
                            {
                                SetPixel(x * cellSize + i, y * cellSize + j, portalColor);
                            }
                        }
                    }
                    Color doorColor = Color.Blue;
                    if (Map.IsDoor(x, y))
                    {
                        for (int i = 0; i < cellSize; i++)
                        {
                            for (int j = 0; j < cellSize; j++)
                            {
                                SetPixel(x * cellSize + i, y * cellSize + j, doorColor);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    SetPixel((int)(player.X * 10) + i, (int)(player.Y * 10) + j, Color.Red);
                }
            }
        }

        private void SetPixel(int x, int y, Color color)
        {
            if (x >= 0 && x < screenWidth && y >= 0 && y < screenHeight)
                backBuffer[y * screenWidth + x] = color.ToArgb();
        }
    }
}
