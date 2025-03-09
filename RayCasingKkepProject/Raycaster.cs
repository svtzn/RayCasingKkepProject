using System;

namespace RayCasingKkepProject
{
    public class Raycaster
    {
        private const float MAX_DISTANCE = 40f;

        /// <summary>
        /// Возвращаем:
        /// distance — расстояние до стены
        /// hitX, hitY — координаты ячейки карты, в которую упёрлись
        /// isDoor — является ли эта ячейка дверью
        /// side — 0, если пересекли вертикальную сторону (OX), 1 — горизонтальную (OY)
        /// </summary>
        public static (float distance, int hitX, int hitY, bool isDoor, int side)
            CastRay(float startX, float startY, float angle, Player player)
        {
            float rayDirX = (float)Math.Cos(angle);
            float rayDirY = (float)Math.Sin(angle);

            int mapX = (int)startX;
            int mapY = (int)startY;

            // Длина луча от одной x-границы до следующей
            float deltaDistX = (rayDirX == 0) ? 1e30f : Math.Abs(1f / rayDirX);
            // Аналогично по y
            float deltaDistY = (rayDirY == 0) ? 1e30f : Math.Abs(1f / rayDirY);

            // Определяем направление шага по x и y
            int stepX = (rayDirX < 0) ? -1 : 1;
            int stepY = (rayDirY < 0) ? -1 : 1;

            // sideDistX / sideDistY — расстояние от начала луча до ближайшей границы по x или y
            float sideDistX, sideDistY;

            if (rayDirX < 0)
                sideDistX = (startX - mapX) * deltaDistX;
            else
                sideDistX = (mapX + 1.0f - startX) * deltaDistX;

            if (rayDirY < 0)
                sideDistY = (startY - mapY) * deltaDistY;
            else
                sideDistY = (mapY + 1.0f - startY) * deltaDistY;

            // side=0 => пересекли вертикальную границу, side=1 => горизонтальную
            int side = 0;

            // Выполняем DDA
            for (int i = 0; i < 200; i++)
            {
                // Проверяем, идём ли мы по x или по y
                if (sideDistX < sideDistY)
                {
                    sideDistX += deltaDistX;
                    mapX += stepX;
                    side = 0;  // вертикальная граница
                }
                else
                {
                    sideDistY += deltaDistY;
                    mapY += stepY;
                    side = 1;  // горизонтальная граница
                }

                // Если вышли за границы или достигли препятствия
                if (mapX < 0 || mapX >= Map.Width || mapY < 0 || mapY >= Map.Height)
                {
                    // Считаем, что ничего не нашли (макс. расстояние)
                    return (MAX_DISTANCE, 0, 0, false, side);
                }

                bool isDoor = Map.IsDoor(mapX, mapY);
                bool isDoorOpen = isDoor && Map.IsDoorOpen(mapX, mapY);

                if (!isDoorOpen && Map.IsObstacle(mapX, mapY))
                {
                    // Найдена стена или закрытая дверь
                    float dist;
                    if (side == 0)
                    {
                        // sideDistX уже перешёл к следующему, значит реальное расстояние:
                        dist = sideDistX - deltaDistX;
                    }
                    else
                    {
                        dist = sideDistY - deltaDistY;
                    }
                    return (dist, mapX, mapY, isDoor, side);
                }
            }

            // Если ничего не нашли
            return (MAX_DISTANCE, 0, 0, false, side);
        }
    }
}
