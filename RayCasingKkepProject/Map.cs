using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasingKkepProject
{
    public static class Map
    {
        public static int Width => 10;
        public static int Height => 10;

        public static int[,] map =
        {
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 1, 0, 1, 1, 1, 0, 0, 1},
            {1, 0, 2, 0, 0, 0, 1, 0, 0, 1},
            {1, 0, 1, 1, 1, 0, 1, 1, 0, 1},
            {1, 0, 0, 0, 2, 0, 0, 0, 0, 1},
            {1, 0, 1, 0, 1, 1, 0, 2, 0, 1},
            {1, 0, 1, 0, 0, 0, 0, 1, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
        };


        private static int[,] heightMap =
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 1, 1, 2, 2, 1, 1, 0, 1 }, // Возвышенности (2)
            { 1, 0, 1, 0, 0, 0, 0, 1, 0, 1 },
            { 1, 0, 1, 0, 1, 1, 0, 1, 0, 1 },
            { 1, 0, 1, 0, 1, 1, 0, 1, 0, 1 },
            { 1, 0, 1, 0, 0, 0, 0, 1, 0, 1 },
            { 1, 0, 1, 1, 1, 1, 1, 1, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        };


        public static bool IsWall(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return true;
            return map[y, x] == 1;
        }

        public static int GetHeight(int x, int y)
        {
            return heightMap[y, x];
        }
        public static bool IsDoor(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return false;
            return map[y, x] == 2; // 2 - идентификатор двери
        }

        public static bool IsObstacle(int x, int y)
        {
            return IsWall(x, y) || IsDoor(x, y);
        }

        private static Dictionary<(int, int), bool> doorStates = new Dictionary<(int, int), bool>();

        public static bool IsDoorOpen(int x, int y) => doorStates.TryGetValue((x, y), out bool state) && state;
        public static void ToggleDoor(int x, int y) => doorStates[(x, y)] = !IsDoorOpen(x, y);

    }
}