using Android.Content;
using Android.Opengl;
using Android.Views;

namespace XRay_Earth.Platforms.Android
{
    public class NativeGLSurfaceView : GLSurfaceView
    {
        private readonly GLRenderer _renderer;

        public NativeGLSurfaceView(Context context) : base(context)
        {
            SetEGLContextClientVersion(2);
            _renderer = new GLRenderer();
            SetRenderer(_renderer);
            RenderMode = Rendermode.Continuously;
        }

        public GLRenderer Renderer => _renderer;
    }
}