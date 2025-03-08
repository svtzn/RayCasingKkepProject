using System;

namespace RayCasingKkepProject
{
    public static class Map
    {
        private static int[,] mapData;

        public static void Load(int[,] newMap)
        {
            mapData = newMap;
        }

        public static int Width => (mapData == null) ? 0 : mapData.GetLength(1);
        public static int Height => (mapData == null) ? 0 : mapData.GetLength(0);

        // Если координаты вне границ карты, считаем ячейку стеной.
        private static bool InBounds(int x, int y)
        {
            return mapData != null && x >= 0 && y >= 0 && y < Height && x < Width;
        }

        public static bool IsWall(int x, int y)
        {
            if (!InBounds(x, y)) return true;
            return mapData[y, x] == 1;
        }

        public static bool IsDoor(int x, int y)
        {
            if (!InBounds(x, y)) return false;
            // 2 – закрытая дверь, 3 – открытая дверь
            return mapData[y, x] == 2 || mapData[y, x] == 3;
        }

        public static bool IsDoorOpen(int x, int y)
        {
            if (!InBounds(x, y)) return false;
            return mapData[y, x] == 3;
        }

        public static bool IsPortal(int x, int y)
        {
            if (!InBounds(x, y)) return false;
            return mapData[y, x] == 9;
        }

        // Ячейка считается препятствием, если не пустая (0) или дверь открыта (3)
        public static bool IsObstacle(int x, int y)
        {
            if (!InBounds(x, y)) return true;
            int cell = mapData[y, x];
            if (cell == 0) return false;
            if (cell == 3) return false; // открытая дверь – можно пройти
            return true; // стена, закрытая дверь, портал – препятствия
        }

        public static void ToggleDoor(int x, int y)
        {
            if (!InBounds(x, y)) return;
            if (mapData[y, x] == 2)
                mapData[y, x] = 3; // открываем дверь
            else if (mapData[y, x] == 3)
                mapData[y, x] = 2; // закрываем дверь
        }
    }
}
