using OpenTK.Mathematics;
using System.Numerics;
using Vector3 = OpenTK.Mathematics.Vector3;
using Quaternion = OpenTK.Mathematics.Quaternion;

namespace XRay_Earth
{
    internal class Camera
    {
        public static Camera Instance { get; } = new Camera();
        private Camera() {

            RecalculateViewMatrix();

        }

        private const string SCREEN_SIZE_ERROR = "Screen dimensions must be greater than zero.";
        private readonly object _lock = new object();

        private float[] _viewArray = new float[16];
        private float[] _projectionArray;

        private Vector3 _eye = new Vector3(0.0f, -2.0f, 0.0f);

        private Quaternion _targetRotation = Quaternion.Identity;
        private Quaternion _declinationCorrection = Quaternion.Identity;

        private int _height = 0;
        private int _width = 0;
        private float _fov = 60.0f;
        private float _depthNear = 0.1f;
        private float _depthFar = 100.0f;
        private float _minFOV = 1f;
        private float _maxFOV = 150f;

        public Vector3 Eye
        {
            get { return _eye; }
            set { _eye = value; }
        }

        public float FOV
        {
            get { return _fov; }
            set {
                _fov = Math.Clamp(value, _minFOV, _maxFOV); 
                RecalculateProjectionMatrix();
            }
        }
        public float MinFOV
        {
            get { return _minFOV; }
            set { _minFOV = Math.Clamp(value, 0.01f, 179.99f); }
        }
        public float MaxFOV
        {
            get { return _maxFOV; }
            set { _maxFOV = Math.Clamp(value, 0.01f, 179.99f); }
        }

        public float DepthNear
        {
            get { return _depthNear; }
            set { _depthNear = value; RecalculateProjectionMatrix(); }
        }
        public float DepthFar
        {
            get { return _depthFar; }
            set { _depthFar = value; RecalculateProjectionMatrix(); }
        }
        public int Height
        {
            get { return _height; }
            set 
            {
                if (value <= 0)
                    throw new InvalidOperationException(SCREEN_SIZE_ERROR);
                _height = value; RecalculateProjectionMatrix(); 
            }
        }
        public int Width
        {
            get { return _width; }
            set 
            {
                if (value <= 0)
                    throw new InvalidOperationException(SCREEN_SIZE_ERROR);
                _width = value; RecalculateProjectionMatrix(); 
            }
        }

        public System.Numerics.Quaternion Rotation
        {
            set
            {
                _targetRotation.X = -value.X;
                _targetRotation.Y = -value.Y;
                _targetRotation.Z = -value.Z;
                _targetRotation.W = value.W;
            }
        }

        public Quaternion DeclinationCorrection
        {
            get { return _declinationCorrection; }
            set { _declinationCorrection = value; }
        }


        public (int Width, int Height, float Fov, float DepthNear, float DepthFar) ProjectionSettings
        {
            set
            {
                if (value.Width <= 0 || value.Height <= 0)
                    throw new InvalidOperationException(SCREEN_SIZE_ERROR);

                _fov = value.Fov;
                _width = value.Width;
                _height = value.Height;
                _depthNear = value.DepthNear;
                _depthFar = value.DepthFar;
                RecalculateProjectionMatrix();
            }
        }

        public (int Width, int Height) ScreenDimensions
        {
            set
            {
                if (value.Width <= 0 || value.Height <= 0)
                    throw new InvalidOperationException(SCREEN_SIZE_ERROR);

                _width = value.Width;
                _height = value.Height;
                RecalculateProjectionMatrix();
            }
        }

        public float[] ViewArray
        {
            get
            {
                    return _viewArray;
            }
        }

        public float[] ProjectionArray
        {
            get
            {
                if (_projectionArray == null)
                        throw new InvalidOperationException("Projection matrix has not been calculated yet. Set screen dimensions first.");
                return _projectionArray;
            }
        }

        public void UpdateViewMatrix()
        {
            RecalculateViewMatrix();
        }

        private void RecalculateViewMatrix()
        {
            Matrix4 rotation = Matrix4.CreateFromQuaternion(_targetRotation * _declinationCorrection);
            Matrix4 translation = Matrix4.CreateTranslation(-_eye);
            Matrix4 viewMatrix = translation * rotation;

            UtilLib.FillMatrix4Array(viewMatrix, _viewArray);
        }

        private void RecalculateProjectionMatrix()
        {
                if (_width <= 0 || _height <= 0) return;

                Matrix4 projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                    MathHelper.DegreesToRadians(_fov),
                    (float)_width / _height,
                    _depthNear,
                    _depthFar
                );

                float[] projectionArray = new float[16];
                UtilLib.FillMatrix4Array(projectionMatrix, projectionArray);
                _projectionArray = projectionArray;

        }
    }
}
