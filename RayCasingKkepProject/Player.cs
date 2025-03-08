﻿using System;
using System.Windows.Forms;

namespace RayCasingKkepProject
{
    public class Player
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Angle { get; private set; } // Угол направления взгляда
        public float Pitch { get; private set; } // Поворот камеры вверх/вниз

        private const float MoveSpeed = 0.1f; // Скорость движения
        private const float RotationSpeed = 0.05f; // Скорость поворота

        public Player(float startX, float startY, float startAngle)
        {
            X = startX;
            Y = startY;
            Angle = startAngle;
            Pitch = 0;
        }

        public void StrafeLeft()
        {
            float strafeSpeed = 0.1f;
            float newX = X + (float)Math.Sin(Angle) * strafeSpeed;
            float newY = Y - (float)Math.Cos(Angle) * strafeSpeed;

            if (!Map.IsWall((int)newX, (int)newY) && !(Map.IsDoor((int)newX, (int)newY) && !Map.IsDoorOpen((int)newX, (int)newY)))
            {
                // Добавлена закрывающая скобка после условия:
                if (!Map.IsWall((int)newX, (int)newY) && !(Map.IsDoor((int)newX, (int)newY) && !Map.IsDoorOpen((int)newX, (int)newY))) 
                {
                    X = newX;
                    Y = newY;
                }
            }
        }

        public void StrafeRight()
        {
            float strafeSpeed = 0.1f;
            float newX = X - (float)Math.Sin(Angle) * strafeSpeed;
            float newY = Y + (float)Math.Cos(Angle) * strafeSpeed;

            if (!Map.IsWall((int)newX, (int)newY) && !(Map.IsDoor((int)newX, (int)newY) && !Map.IsDoorOpen((int)newX, (int)newY)))
            {
                if (!Map.IsWall((int)newX, (int)newY) && !(Map.IsDoor((int)newX, (int)newY) && !Map.IsDoorOpen((int)newX, (int)newY))) 
                {
                    X = newX;
                    Y = newY;
                }
            }
        }

        public void MoveForward() // Движение вперед
        {
            float newX = X + (float)Math.Cos(Angle) * MoveSpeed;
            float newY = Y + (float)Math.Sin(Angle) * MoveSpeed;

            if (!Map.IsWall((int)newX, (int)newY) && !(Map.IsDoor((int)newX, (int)newY) && !Map.IsDoorOpen((int)newX, (int)newY)))
            {
                if (!Map.IsWall((int)newX, (int)newY) && !(Map.IsDoor((int)newX, (int)newY) && !Map.IsDoorOpen((int)newX, (int)newY))) 
                {
                    X = newX;
                    Y = newY;
                }
            }
        }

        public void MoveBackward() // Движение назад
        {
            float newX = X - (float)Math.Cos(Angle) * MoveSpeed;
            float newY = Y - (float)Math.Sin(Angle) * MoveSpeed;

            if (!Map.IsWall((int)newX, (int)newY) && !(Map.IsDoor((int)newX, (int)newY) && !Map.IsDoorOpen((int)newX, (int)newY)))
            {
                if (!Map.IsWall((int)newX, (int)newY) && !(Map.IsDoor((int)newX, (int)newY) && !Map.IsDoorOpen((int)newX, (int)newY))) 
                {
                    X = newX;
                    Y = newY;
                }
            }
        }

        public void Interact()
        {
            // Проверка двери или портала перед игроком
            int checkX = (int)(X + Math.Cos(Angle) * 1.5);
            int checkY = (int)(Y + Math.Sin(Angle) * 1.5);

            if (Map.IsDoor(checkX, checkY))
            {
                Map.ToggleDoor(checkX, checkY);
            }
            else if (Map.IsPortal(checkX, checkY))
            {
                using (var menu = new LocationMenu())
                {
                    if (menu.ShowDialog() == DialogResult.OK)
                    {
                        MapManager.LoadMap(menu.SelectedLocation);
                        X = 3; // Новая позиция на новой карте
                        Y = 3;
                    }
                }
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
