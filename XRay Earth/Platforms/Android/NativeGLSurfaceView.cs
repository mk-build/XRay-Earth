using Android.Content;
using Android.Opengl;
using Android.Views;

namespace XRay_Earth.Platforms.Android
{
    public class NativeGLSurfaceView : GLSurfaceView
    {
        private readonly GLRenderer _renderer;
        private readonly ScaleGestureDetector _scaleDetector;

        public NativeGLSurfaceView(Context context) : base(context)
        {
            SetEGLContextClientVersion(2);
            _renderer = new GLRenderer();
            SetRenderer(_renderer);
            RenderMode = Rendermode.Continuously;
            _scaleDetector = new ScaleGestureDetector(context, new PinchListener());
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            _scaleDetector.OnTouchEvent(e);
            return true;
        }
        public class PinchListener : ScaleGestureDetector.SimpleOnScaleGestureListener
        {
            public override bool OnScale(ScaleGestureDetector detector)
            {
                // ScaleFactor > 1 means fingers spreading apart (zoom in = reduce FOV)
                // ScaleFactor < 1 means fingers pinching together (zoom out = increase FOV)
                Camera.Instance.FOV /= detector.ScaleFactor;
                return true;
            }
        }

        public GLRenderer Renderer => _renderer;
    }
}