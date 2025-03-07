using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCasingKkepProject
{
    public class Player
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Angle { get; private set; } // Угол направления взгляда
        //public float Z { get; private set; } // Высота игрока
        public float Pitch { get; private set; } // Поворот камеры вверх/вниз
        //private const float Gravity = 0.1f; // Гравитация
        //private const float JumpStrength = 1.5f; // Сила прыжка
        //private bool isJumping = false; //Прыжок?


        private const float MoveSpeed = 0.1f; // Скорость движения
        private const float RotationSpeed = 0.05f; // Скорость поворота


        public Player(float startX, float startY, float startAngle)
        {
            X = startX;
            Y = startY;
            //Z = 0;
            Angle = startAngle;
            Pitch = 0;
        }

        //public void Jump()
        //{
        //    if (Z == 0) // Прыгать можно только с земли
        //    {
        //        isJumping = true;
        //        Z += JumpStrength;
        //    }
        //}

        //public void Update()
        //{
        //    if (isJumping)
        //    {
        //        Z -= Gravity; // Гравитация опускает игрока
        //        if (Z <= 0)
        //        {
        //            Z = 0;
        //            isJumping = false;
        //        }
        //
        //        int floorHeight = Map.GetHeight((int)X, (int)Y);
        //        if (Z < floorHeight)
        //        {
        //            Z += 0.1f; // Медленный подъем
        //        }
        //
        //    }
        //}

        public void StrafeLeft()
        {
            float strafeSpeed = 0.1f;
            float newX = X + (float)Math.Sin(Angle) * strafeSpeed;
            float newY = Y - (float)Math.Cos(Angle) * strafeSpeed;

            if (!Map.IsWall((int)newX, (int)newY)) // Проверка на стену
            {
                X = newX;
                Y = newY;
            }
        }

        public void StrafeRight()
        {
            float strafeSpeed = 0.1f;
            float newX = X - (float)Math.Sin(Angle) * strafeSpeed;
            float newY = Y + (float)Math.Cos(Angle) * strafeSpeed;

            if (!Map.IsWall((int)newX, (int)newY))
            {
                X = newX;
                Y = newY;
            }
        }


        public void MoveForward() // Движение вперед
        {
            float newX = X + (float)Math.Cos(Angle) * MoveSpeed;
            float newY = Y + (float)Math.Sin(Angle) * MoveSpeed;

            if (!Map.IsWall((int)newX, (int)newY))
            {
                X = newX;
                Y = newY;
            }
        }

        public void MoveBackward() // Движение назад
        {
            float newX = X - (float)Math.Cos(Angle) * MoveSpeed;
            float newY = Y - (float)Math.Sin(Angle) * MoveSpeed;

            int mapX = (int)newX;
            int mapY = (int)newY;

            if (!Map.IsObstacle(mapX, mapY) || (Map.IsDoor(mapX, mapY) && Map.IsDoorOpen(mapX, mapY)))
            {
                X = newX;
                Y = newY;
            }


            if (!Map.IsWall((int)newX, (int)newY))
            {
                X = newX;
                Y = newY;
            }
        }

        public void Interact()
        {
            // Проверка двери перед игроком
            int checkX = (int)(X + Math.Cos(Angle) * 1.5);
            int checkY = (int)(Y + Math.Sin(Angle) * 1.5);

            if (Map.IsDoor(checkX, checkY))
            {
                Map.ToggleDoor(checkX, checkY);
            }
        }


        
        public void RotateLeft() // Поворот камеры влево
        {
            Angle -= RotationSpeed;
        }

        public void RotateRight() // Поворот камеры вправо
        {
            Angle += RotationSpeed;
        }

        public void LookUp() { if (Pitch > -0.5f) Pitch -= 0.05f; } // Поворот камеры вверх
        public void LookDown() { if (Pitch < 0.5f) Pitch += 0.05f; } // Поворот камеры вниз

    }
}