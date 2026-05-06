using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;
using OpenTK.Mathematics;

namespace XRay_Earth.Platforms.Android
{
    public class GLRenderer : Java.Lang.Object, GLSurfaceView.IRenderer
    {
        private int shaderProgramID = 0;

        public void OnSurfaceCreated(IGL10? gl, Javax.Microedition.Khronos.Egl.EGLConfig? config)
        {
            GLES20.GlClearColor(0.2f, 0.0f, 0.4f, 1.0f);
            GLES20.GlEnable(GLES20.GlDepthTest);
            Scene.Instance.Initialize();

        }

        public void OnSurfaceChanged(IGL10? gl, int width, int height)
        {
            GLES20.GlViewport(0, 0, width, height);
            Camera.Instance.ScreenDimensions = (width, height);
        }

        public void OnDrawFrame(IGL10? gl)
        {

            Camera.Instance.UpdateViewMatrix();
            GLES20.GlClear(GLES20.GlColorBufferBit | GLES20.GlDepthBufferBit);

            foreach (Mesh mesh in Scene.Instance.RenderQueue)
            {
                if (mesh.ShaderProgram.ProgramID != shaderProgramID)
                {
                    GLES20.GlUseProgram(mesh.ShaderProgram.ProgramID);
                    shaderProgramID = mesh.ShaderProgram.ProgramID;
                }

                mesh.Draw();
            }

        }
    }
}