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

        public static (float distance, int hitX, int hitY) CastRay(float startX, float startY, float angle, Player player)
        {
            float rayX = startX;
            float rayY = startY;
            float stepX = (float)Math.Cos(angle) * 0.3f;
            float stepY = (float)Math.Sin(angle) * 0.3f;

            for (int i = 0; i < MaxDepth; i++)
            {
                rayX += stepX;
                rayY += stepY;

                int mapX = (int)rayX;
                int mapY = (int)rayY;

                if (Map.IsObstacle(mapX, mapY) && !(Map.IsDoor(mapX, mapY) && Map.IsDoorOpen(mapX, mapY)))
                {
                    float distance = (float)Math.Sqrt((rayX - startX) * (rayX - startX) + (rayY - startY) * (rayY - startY));
                    return (distance * (float)Math.Cos(angle - player.Angle), mapX, mapY);
                }
            }
            return (MaxDepth, 0, 0);
        }

    }
}