using Android.Opengl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Android.Renderscripts;

namespace XRay_Earth.Platforms.Android
{
    internal class Mesh
    {
        private ObjGeometry _geometry;
        private ShaderProgram _shaderProgram;
        private Texture _texture;

        private bool _geometryUploaded = false;
        private bool _rebuildModelArray = true;

        private float[] _modelArray = new float[16];
        private int _vertexBuffer;
        private int _indexBuffer;

        private Vector3 _position = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _scale = Vector3.One;

        public Vector3 Position
        {
            get { return _position; }
            set {
                _position = value;
                _rebuildModelArray = true;
            }
        }

        public Quaternion Rotation
        {
            get { return _rotation; }
            set {
                _rotation = value;
                _rebuildModelArray = true;
            }
        }

        public Vector3 Scale
        {
            get { return _scale; }
            set {
                _scale = value;
                _rebuildModelArray = true;
            }
        }

        public ObjGeometry Geometry
        {
            get { return _geometry; }
            set { 
                _geometry = value;
                _geometryUploaded = false;            
                }
        }

        public ShaderProgram ShaderProgram
        {
            get { return _shaderProgram; }
            set { _shaderProgram = value; }
        }


        public Mesh(ObjGeometry geometry, ShaderProgram shader)
        {
            _geometry = geometry;
            _shaderProgram = shader;

            SetupBuffers();
        }

        public Mesh(ObjGeometry geometry, ShaderProgram shader, Texture texture)
        {
            _texture = texture;
            _geometry = geometry;
            _shaderProgram = shader;

            SetupBuffers();
        }

        private void SetupBuffers()
        {
            int[] buffers = new int[2];
            GLES20.GlGenBuffers(2, buffers, 0);
            _vertexBuffer = buffers[0];
            _indexBuffer = buffers[1];
        }


        public void Draw()
        {

            if(!_geometryUploaded && _geometry.IsLoaded)
            {
                UploadMesh();
            }
            
            if(_texture != null && !_texture.IsUploaded && _texture.IsLoaded)
            {
                _texture.UploadTexture();
            }

            if (_geometryUploaded && (_texture == null || _texture.IsUploaded))
            {
                DrawElement();
            }
        }


        private void DrawElement()
        {
            if (_rebuildModelArray)
            {
                RebuildModelArray();
            }            

            GLES20.GlUniformMatrix4fv(_shaderProgram.GetUniformLocation(_shaderProgram.UniformNames[0]), 1, false, _modelArray, 0);
            GLES20.GlUniformMatrix4fv(_shaderProgram.GetUniformLocation(_shaderProgram.UniformNames[1]), 1, false, Camera.Instance.ViewArray, 0);
            GLES20.GlUniformMatrix4fv(_shaderProgram.GetUniformLocation(_shaderProgram.UniformNames[2]), 1, false, Camera.Instance.ProjectionArray, 0);

            if (_texture != null)
            {
                GLES20.GlActiveTexture(GLES20.GlTexture0);
                GLES20.GlBindTexture(GLES20.GlTexture2d, _texture.ID);
                GLES20.GlUniform1i(_shaderProgram.GetUniformLocation(_shaderProgram.UniformNames[3]), 0);
            }
            GLES20.GlBindBuffer(GLES20.GlArrayBuffer, _vertexBuffer);
            GLES20.GlBindBuffer(GLES20.GlElementArrayBuffer, _indexBuffer);

            GLES20.GlEnableVertexAttribArray(0);
            GLES20.GlEnableVertexAttribArray(1);
            GLES20.GlEnableVertexAttribArray(2);
            GLES20.GlVertexAttribPointer(0, 3, GLES20.GlFloat, false, 32, 0);
            GLES20.GlVertexAttribPointer(1, 2, GLES20.GlFloat, false, 32, 12);
            GLES20.GlVertexAttribPointer(2, 3, GLES20.GlFloat, false, 32, 20);

            GLES20.GlDrawElements(GLES20.GlTriangles, _geometry.IndexArray.Length, GLES20.GlUnsignedInt, 0);

        }

        private void RebuildModelArray()
        {     
            Matrix4 t = Matrix4.CreateTranslation(_position);
            Matrix4 r = Matrix4.CreateFromQuaternion(_rotation);
            Matrix4 s = Matrix4.CreateScale(_scale);

            Matrix4 modelMatrix = t * r * s;

            UtilLib.FillMatrix4Array(modelMatrix, _modelArray);

            _rebuildModelArray = false;
        }

        private void UploadMesh()
        {
            float[] meshArray = _geometry.MeshArray;
            int[] indexArray = _geometry.IndexArray;

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

            _geometryUploaded = true;
        }
    }
}
