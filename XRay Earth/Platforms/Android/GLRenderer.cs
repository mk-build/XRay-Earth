using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;
using OpenTK.Mathematics;

namespace XRay_Earth.Platforms.Android
{
    public class GLRenderer : Java.Lang.Object, GLSurfaceView.IRenderer
    {
        private int _program;
        private Matrix4 _modelMatrix;

        private int _modelLocation;
        private int _viewLocation;
        private int _projectionLocation;

        private int _vertexBuffer;
        private int _indexBuffer;

        private readonly float[] _modelArray = new float[16];
        private readonly float[] _viewArray = new float[16];
        private readonly float[] _projectionArray = new float[16];

        private const string VertexShaderSource = @"
            attribute vec4 aPosition;
            uniform mat4 uModel;
            uniform mat4 uView;
            uniform mat4 uProjection;
            void main() {
                gl_Position = uProjection * uView * uModel * aPosition;
            }";

        private const string FragmentShaderSource = @"
            precision mediump float;
            void main() {
                gl_FragColor = vec4(1.0, 0.5, 0.0, 1.0);
            }";

        private ObjMesh _mesh = new ObjMesh("UVSphere.obj");
        private bool _meshUploaded = false;

        public void OnSurfaceCreated(IGL10? gl, Javax.Microedition.Khronos.Egl.EGLConfig? config)
        {
            GLES20.GlClearColor(0.2f, 0.0f, 0.4f, 1.0f);
            _program = BuildProgram(VertexShaderSource, FragmentShaderSource);

            _modelLocation = GLES20.GlGetUniformLocation(_program, "uModel");
            _viewLocation = GLES20.GlGetUniformLocation(_program, "uView");
            _projectionLocation = GLES20.GlGetUniformLocation(_program, "uProjection");

            _modelMatrix = Matrix4.Identity;

            int[] buffers = new int[2];
            GLES20.GlGenBuffers(2, buffers, 0);
            _vertexBuffer = buffers[0];
            _indexBuffer = buffers[1];
        }

        public void OnSurfaceChanged(IGL10? gl, int width, int height)
        {
            GLES20.GlViewport(0, 0, width, height);
            Camera.Instance.ScreenDimensions = (width, height);
        }

        public void OnDrawFrame(IGL10? gl)
        {

            Camera.Instance.UpdateViewMatrix();

            GLES20.GlClear(GLES20.GlColorBufferBit);
            GLES20.GlUseProgram(_program);

            if (!_mesh.IsLoaded) return;
            if (!_meshUploaded)
            {
                UploadMesh();
                _meshUploaded = true;
            }

            FillMatrix4Array(_modelMatrix, _modelArray);
            FillMatrix4Array(Camera.Instance.ViewMatrix, _viewArray);
            FillMatrix4Array(Camera.Instance.ProjectionMatrix, _projectionArray);

            GLES20.GlUniformMatrix4fv(_modelLocation, 1, false, _modelArray, 0);
            GLES20.GlUniformMatrix4fv(_viewLocation, 1, false, _viewArray, 0);
            GLES20.GlUniformMatrix4fv(_projectionLocation, 1, false, _projectionArray, 0);

            GLES20.GlBindBuffer(GLES20.GlArrayBuffer, _vertexBuffer);
            GLES20.GlBindBuffer(GLES20.GlElementArrayBuffer, _indexBuffer);

            GLES20.GlEnableVertexAttribArray(0);
            GLES20.GlVertexAttribPointer(0, 3, GLES20.GlFloat, false, 32, 0);

            GLES20.GlDrawElements(GLES20.GlTriangles, _mesh.IndexArray.Length, GLES20.GlUnsignedInt, 0);

        }

        private int BuildProgram(string vertexSource, string fragmentSource)
        {
            int vertexShader = CompileShader(GLES20.GlVertexShader, vertexSource);
            int fragmentShader = CompileShader(GLES20.GlFragmentShader, fragmentSource);

            int program = GLES20.GlCreateProgram();
            GLES20.GlAttachShader(program, vertexShader);
            GLES20.GlAttachShader(program, fragmentShader);
            GLES20.GlBindAttribLocation(program, 0, "aPosition");
            GLES20.GlLinkProgram(program);

            return program;
        }

        private int CompileShader(int type, string source)
        {
            int shader = GLES20.GlCreateShader(type);
            GLES20.GlShaderSource(shader, source);
            GLES20.GlCompileShader(shader);

            int[] compiled = new int[1];
            GLES20.GlGetShaderiv(shader, GLES20.GlCompileStatus, compiled, 0);
            if (compiled[0] == 0)
            {
                string log = GLES20.GlGetShaderInfoLog(shader);
                throw new Exception($"Shader compile error: {log}");
            }

            return shader;
        }
        private void FillMatrix4Array(Matrix4 matrix, float[] array)
        {
            array[0] = matrix.M11; array[1] = matrix.M12; array[2] = matrix.M13; array[3] = matrix.M14;
            array[4] = matrix.M21; array[5] = matrix.M22; array[6] = matrix.M23; array[7] = matrix.M24;
            array[8] = matrix.M31; array[9] = matrix.M32; array[10] = matrix.M33; array[11] = matrix.M34;
            array[12] = matrix.M41; array[13] = matrix.M42; array[14] = matrix.M43; array[15] = matrix.M44;
        }

        private void UploadMesh()
        {
            float[] meshArray = _mesh.MeshArray;
            int[] indexArray = _mesh.IndexArray;

            var vertexData = Java.Nio.ByteBuffer
                .AllocateDirect(meshArray.Length * 4)
                .Order(Java.Nio.ByteOrder.NativeOrder())
                .AsFloatBuffer();
            vertexData.Put(meshArray);
            vertexData.Position(0);

            GLES20.GlBindBuffer(GLES20.GlArrayBuffer, _vertexBuffer);
            GLES20.GlBufferData(GLES20.GlArrayBuffer, meshArray.Length * 4, vertexData, GLES20.GlStaticDraw);

            var indexData = Java.Nio.ByteBuffer
                .AllocateDirect(indexArray.Length * 4)
                .Order(Java.Nio.ByteOrder.NativeOrder())
                .AsIntBuffer();
            indexData.Put(indexArray);
            indexData.Position(0);

            GLES20.GlBindBuffer(GLES20.GlElementArrayBuffer, _indexBuffer);
            GLES20.GlBufferData(GLES20.GlElementArrayBuffer, indexArray.Length * 4, indexData, GLES20.GlStaticDraw);
        }
    }
}