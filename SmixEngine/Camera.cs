using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmixEngine
{
    public class Camera: GameObject
    {
        /// <summary>
        /// field of view (radians)
        /// </summary>
        float fov;

        public float Fov
        {
            get { return fov; }
            set { fov = value; }
        }

        /// <summary>
        /// Clipping bounds
        /// </summary>
        float nearClip = 0.001f;
        float farClip = 3000.0f;

        /// <summary>
        /// Target that camera is looking at
        /// </summary>
        float rotX = 0;
        float rotY = 0;

        public Camera() 
        {
            fov = MathHelper.PiOver2;
        }
        public Camera(Vector3 position, float rotX) : base(position, Quaternion.CreateFromYawPitchRoll(rotX, 0, 0), 1f, null)
        {
            this.rotX = rotX;
            fov = MathHelper.PiOver2;
        }

        public Matrix getViewMatrix()
        {
            // cam pos, target pos, up direction
            return Matrix.CreateLookAt(Pos, Pos + Vector3.Transform(Vector3.Forward, Rotation), Vector3.Up);
        }
        public Matrix getProjMatrix()
        {
            return Matrix.CreatePerspectiveFieldOfView(
                fov,
                1,  // aspect ratio
                nearClip,
                farClip
            );
        }

        public void MoveCamera(Vector2 byHowMuch)
        {
            rotX += byHowMuch.X;
            rotY += byHowMuch.Y;
            rotY = MathHelper.Clamp(
                rotY,
                -1.25f,
                .520f);
            Rotation = Quaternion.CreateFromYawPitchRoll(rotX, rotY, 0);
        }
        
        public float RotX
        {
            get { return rotX; }
            set { rotX = value; }
        }

        public float RotY
        {
            get { return rotX; }
            set { rotX = value; }
        }

    }
}
