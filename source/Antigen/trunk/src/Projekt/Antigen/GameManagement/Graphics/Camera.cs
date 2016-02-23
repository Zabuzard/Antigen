using System;
using Antigen.Input;
using Microsoft.Xna.Framework;

namespace Antigen.Graphics
{
// ReSharper disable once UnusedMember.Global
    class Camera
    {
        private Matrix mTransformMatrix;
        private Vector2 mPosition;
        private float mZoom = 1;

        private const float MinZoom = 0.1f;
        private const float MaxZoom = 10f;
// ReSharper disable once UnusedMember.Local
        private const int ScrollSensitiveBorderWidth = 10;

        public Camera() 
        {
            UpdateMatrix();
        }

// ReSharper disable UnusedMember.Global
        public Vector2 Position
// ReSharper restore UnusedMember.Global
        {
            get { return mPosition; }
            set { mPosition = value; UpdateMatrix(); }
        }

// ReSharper disable UnusedMember.Global
        public Matrix TransformMatrix
// ReSharper restore UnusedMember.Global
        {
            get { return mTransformMatrix; }
        }

// ReSharper disable UnusedMember.Global
        public float Zoom
// ReSharper restore UnusedMember.Global
        {
            get { return mZoom; }
            set
            {
                if (value < MinZoom) mZoom = MinZoom;
                else if (value > MaxZoom) mZoom = MaxZoom;
                else mZoom = value;
                UpdateMatrix();
            }
        }

// ReSharper disable once UnusedMember.Local
// ReSharper disable once UnusedParameter.Local
        private void MouseMoved(MouseMovedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void UpdateMatrix()
        {
            mTransformMatrix = Matrix.CreateTranslation(mPosition.X, mPosition.Y, 0) * Matrix.CreateScale(mZoom);
        }
    }
}
