using Microsoft.Maui.Handlers;

namespace XRay_Earth.Platforms.Android
{
    public class GLViewHandler : ViewHandler<GLView, NativeGLSurfaceView>
    {
        public static PropertyMapper<GLView, GLViewHandler> Mapper = new(ViewHandler.ViewMapper);

        public GLViewHandler() : base(Mapper) { }

        protected override NativeGLSurfaceView CreatePlatformView()
        {
            return new NativeGLSurfaceView(Context);
        }
    }
}