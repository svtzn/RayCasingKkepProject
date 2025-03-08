using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasingKkepProject
{
    public class Raycaster
    {
        private const int MaxDepth = 40; // Максимальная дальность лучей

        public static (float distance, int hitX, int hitY, bool isDoor) CastRay(float startX, float startY, float angle, Player player)
        {
            float rayX = startX;
            float rayY = startY;
            float stepX = (float)Math.Cos(angle) * 0.3f;
            float stepY = (float)Math.Sin(angle) * 0.3f;

            // Задаем толщину двери (например, 40% ячейки)
            float doorThickness = 0.4f;
            float doorMin = (1 - doorThickness) / 2f; // например, 0.3
            float doorMax = doorMin + doorThickness;    // например, 0.7

            for (int i = 0; i < MaxDepth; i++)
            {
                rayX += stepX;
                rayY += stepY;

                int mapX = (int)rayX;
                int mapY = (int)rayY;

                bool isDoor = Map.IsDoor(mapX, mapY);
                bool isDoorOpen = isDoor && Map.IsDoorOpen(mapX, mapY);

                // Если дверь открыта, пропускаем её (как если бы её не было)
                if (isDoorOpen)
                {
                    continue;
                }

                if (Map.IsObstacle(mapX, mapY))
                {
                    if (isDoor)
                    {
                        // Определяем, по какому измерению производить проверку.
                        // Если угол больше горизонтального – берем разницу по X, иначе по Y.
                        float absRayDirX = Math.Abs((float)Math.Cos(angle));
                        float absRayDirY = Math.Abs((float)Math.Sin(angle));
                        float hitPos = (absRayDirX > absRayDirY) ? (rayX - mapX) : (rayY - mapY);

                        // Если попадание вне центральной области двери, пропускаем (продолжаем искать)
                        if (hitPos < doorMin || hitPos > doorMax)
                        {
                            continue;
                        }
                    }

                    float distance = (float)Math.Sqrt((rayX - startX) * (rayX - startX) +
                                                        (rayY - startY) * (rayY - startY));
                    // Корректируем расстояние, учитывая угол отклонения
                    return (distance * (float)Math.Cos(angle - player.Angle), mapX, mapY, isDoor);
                }
            }
            return (MaxDepth, 0, 0, false);
        }


    }
}