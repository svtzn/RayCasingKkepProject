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

            DrawSceneColumns();
            DrawMinimap();

            var bmpData = bmp.LockBits(new Rectangle(0, 0, screenWidth, screenHeight),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(backBuffer, 0, bmpData.Scan0, backBuffer.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        /// <summary>
        /// Отрисовка стены, пола и потолка по столбцам.
        /// </summary>
        private void DrawSceneColumns()
        {
            float fov = (float)(60 * Math.PI / 180);
            float rayDirStep = fov / screenWidth;
            float rayAngleStart = player.Angle - fov / 2f;

            // Предварительное получение данных текстур стен, пола и потолка
            byte[] brickData = GetTextureData(TextureManager.BrickTexture);
            byte[] doorData = GetTextureData(TextureManager.DoorTexture);
            byte[] portalData = GetTextureData(TextureManager.PortalTexture);
            byte[] floorData = GetTextureData(TextureManager.FloorTexture);
            byte[] ceilingData = GetTextureData(TextureManager.CeilingTexture);

            int brickW = TextureManager.BrickTexture.Width;
            int brickH = TextureManager.BrickTexture.Height;
            int doorW = TextureManager.DoorTexture.Width;
            int doorH = TextureManager.DoorTexture.Height;
            int portalW = TextureManager.PortalTexture.Width;
            int portalH = TextureManager.PortalTexture.Height;
            int floorW = TextureManager.FloorTexture.Width;
            int floorH = TextureManager.FloorTexture.Height;
            int ceilW = TextureManager.CeilingTexture.Width;
            int ceilH = TextureManager.CeilingTexture.Height;

            float projectionPlaneDistance = (screenWidth / 2f) / (float)Math.Tan(fov / 2f);
            float centerY = screenHeight / 2 - (player.Pitch * screenHeight * 0.5f);

            // Допустим, уровень пола – 0, потолок – 1, а камера на 0.5
            float cameraHeight = 0.5f;
            float wallTop = 1.0f;
            float wallBottom = 0.0f;
            float maxDistance = 15f;

            for (int x = 0; x < screenWidth; x++)
            {
                float rayAngle = rayAngleStart + x * rayDirStep;
                // Запускаем луч
                var (dist, hitX, hitY, isDoor, side) = Raycaster.CastRay(player.X, player.Y, rayAngle, player);
                bool isPortal = Map.IsPortal(hitX, hitY);

                // Выбираем текстуру стены
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

                float rayDirX = (float)Math.Cos(rayAngle);
                float rayDirY = (float)Math.Sin(rayAngle);

                // Вычисляем wallX в зависимости от стороны столкновения
                float wallX;
                if (side == 0)
                    wallX = player.Y + dist * rayDirY;
                else
                    wallX = player.X + dist * rayDirX;

                wallX -= (float)Math.Floor(wallX);

                // Проецируем верх и низ стены
                float projTop = centerY - (projectionPlaneDistance * (wallTop - cameraHeight) / dist);
                float projBottom = centerY - (projectionPlaneDistance * (wallBottom - cameraHeight) / dist);

                // Индексы на экране, между которыми будем рисовать стену
                int wallStartY = (int)projTop;
                int wallEndY = (int)projBottom;

                // -- ВАЖНО: Зажимаем (clamp) значения в диапазон [0..screenHeight]
                if (wallStartY < 0) wallStartY = 0;
                if (wallStartY > screenHeight) wallStartY = screenHeight;

                if (wallEndY < 0) wallEndY = 0;
                if (wallEndY >= screenHeight) wallEndY = screenHeight - 1;

                // Затемнение для стены
                float shadeWall = 1.0f - Math.Min(dist / maxDistance, 1.0f);

                // Отрисовка столбца стены
                float columnHeight = projBottom - projTop;
                // Если после зажима wallStartY > wallEndY, значит стены не видно (вся стена вышла за экран)
                if (wallStartY <= wallEndY)
                {
                    for (int y = wallStartY; y <= wallEndY; y++)
                    {
                        float relY = (y - projTop) / columnHeight;
                        int texX = (int)(wallX * texW);
                        if (texX >= texW) texX = texW - 1;
                        if (texX < 0) texX = 0;

                        int texY = (int)(relY * texH);
                        if (texY >= texH) texY = texH - 1;
                        if (texY < 0) texY = 0;

                        Color color = GetPixelFromTextureData(texData, texX, texY, texW);
                        int r = (int)(color.R * shadeWall);
                        int g = (int)(color.G * shadeWall);
                        int b = (int)(color.B * shadeWall);
                        color = Color.FromArgb(255, r, g, b);

                        // Гарантированно в диапазоне, т.к. y ∈ [wallStartY..wallEndY], x ∈ [0..screenWidth-1]
                        backBuffer[y * screenWidth + x] = color.ToArgb();
                    }
                }

                // Отрисовка потолка над стеной (от y=0 до wallStartY-1)
                // Если wallStartY > 0, значит есть место для потолка
                for (int y = 0; y < wallStartY; y++)
                {
                    // Проверяем, чтобы y не вылез за экран (обычно не должен, но на всякий случай)
                    if (y < 0 || y >= screenHeight) continue;

                    float p = centerY - y;
                    if (p < 1.0f) p = 1.0f; // защита от слишком малого значения
                    float rowDistance = cameraHeight * projectionPlaneDistance / p;

                    float worldX = player.X + rowDistance * rayDirX;
                    float worldY = player.Y + rowDistance * rayDirY;

                    int texX = (int)(worldX * ceilW) % ceilW;
                    if (texX < 0) texX += ceilW;
                    int texY = (int)(worldY * ceilH) % ceilH;
                    if (texY < 0) texY += ceilH;

                    Color color = GetPixelFromTextureData(ceilingData, texX, texY, ceilW);
                    float shade = 1.0f - Math.Min(rowDistance / maxDistance, 1.0f);

                    int r = (int)(color.R * shade);
                    int g = (int)(color.G * shade);
                    int b = (int)(color.B * shade);
                    color = Color.FromArgb(255, r, g, b);

                    backBuffer[y * screenWidth + x] = color.ToArgb();
                }

                // Отрисовка пола под стеной (от y=wallEndY+1 до screenHeight-1)
                // Если wallEndY < screenHeight-1, значит есть место для пола
                for (int y = wallEndY + 1; y < screenHeight; y++)
                {
                    // Проверяем, чтобы y не вылез за экран
                    if (y < 0 || y >= screenHeight) continue;

                    float p = y - centerY;
                    if (p < 1.0f) p = 1.0f; // защита от слишком малого p
                    float rowDistance = cameraHeight * projectionPlaneDistance / p;

                    float worldX = player.X + rowDistance * rayDirX;
                    float worldY = player.Y + rowDistance * rayDirY;

                    int texX = (int)(worldX * floorW) % floorW;
                    if (texX < 0) texX += floorW;
                    int texY = (int)(worldY * floorH) % floorH;
                    if (texY < 0) texY += floorH;

                    Color color = GetPixelFromTextureData(floorData, texX, texY, floorW);
                    float shade = 1.0f - Math.Min(rowDistance / maxDistance, 1.0f);

                    int r = (int)(color.R * shade);
                    int g = (int)(color.G * shade);
                    int b = (int)(color.B * shade);
                    color = Color.FromArgb(255, r, g, b);

                    backBuffer[y * screenWidth + x] = color.ToArgb();
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
            // Отрисовка клеток карты (как у вас было)
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
                    // Порталы
                    if (Map.IsPortal(x, y))
                    {
                        for (int i = 0; i < cellSize; i++)
                        {
                            for (int j = 0; j < cellSize; j++)
                            {
                                SetPixel(x * cellSize + i, y * cellSize + j, Color.Purple);
                            }
                        }
                    }
                    // Двери
                    if (Map.IsDoor(x, y))
                    {
                        for (int i = 0; i < cellSize; i++)
                        {
                            for (int j = 0; j < cellSize; j++)
                            {
                                SetPixel(x * cellSize + i, y * cellSize + j, Color.Blue);
                            }
                        }
                    }
                }
            }

            // Отрисовка игрока (красный квадратик 5x5)
            int playerMapX = (int)(player.X * cellSize);
            int playerMapY = (int)(player.Y * cellSize);
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    SetPixel(playerMapX + i, playerMapY + j, Color.Red);
                }
            }

            // Теперь рисуем линию (стрелку) от центра этого квадратика в направлении player.Angle.
            // Центр квадратика: +2, +2
            int centerX = playerMapX + 2;
            int centerY = playerMapY + 2;

            // Длина стрелки, например, 10 пикселей на миникарте
            int arrowLength = 5;

            // Конечная точка стрелки
            int arrowX = centerX + (int)(Math.Cos(player.Angle) * arrowLength);
            int arrowY = centerY + (int)(Math.Sin(player.Angle) * arrowLength);

            // Рисуем линию (желтым цветом) между (centerX, centerY) и (arrowX, arrowY)
            DrawLineMinimap(centerX, centerY, arrowX, arrowY, Color.Yellow);
        }

        private void DrawLineMinimap(int x0, int y0, int x1, int y1, Color color)
        {
            // Алгоритм Брезенхэма
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;

            while (true)
            {
                SetPixel(x0, y0, color); // рисуем пиксель

                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }
                if (e2 <= dx)
                {
                    err += dx;
                    y0 += sy;
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
