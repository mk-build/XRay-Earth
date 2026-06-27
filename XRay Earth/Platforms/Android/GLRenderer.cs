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

            //  Sets background color
            GLES20.GlClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GLES20.GlEnable(GLES20.GlDepthTest);
            GLES20.GlEnable(0x0B44);    //  Gl Cull face raw constant. Don't know the name of the constant :/
            GLES20.GlCullFace(GLES20.GlFront);
            Scene.Instance.Initialize();

        }

        public void OnSurfaceChanged(IGL10? gl, int width, int height)
        {
            GLES20.GlViewport(0, 0, width, height);
            Camera.GetCamera(Camera.Type.Main).ViewPortDimensions = (width, height);
            Camera.GetCamera(Camera.Type.Compass).ViewPortDimensions = (width, height);
        }

        public void OnDrawFrame(IGL10? gl)
        {

            Camera.GetCamera(Camera.Type.Main).UpdateViewMatrix();
            Camera.GetCamera(Camera.Type.Compass).UpdateViewMatrix();

            GLES20.GlClear(GLES20.GlColorBufferBit | GLES20.GlDepthBufferBit);
            foreach (Mesh mesh in Scene.Instance.RenderQueue)
            {
                //  Only change the shader program in use if it is different from the previous mesh
                //  Scene handles sorting meshes by shaderProgramID
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