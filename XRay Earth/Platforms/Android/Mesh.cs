using Android.Graphics;
using Android.Opengl;
using Android.Renderscripts;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // Gets empty buffer IDs for later use.
            // Doesn't allocate any space yet.
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

            //  Sets the model, view and projection matrix arrays for the current mesh.
            //  Model matrix contains position, rotation and scaling information for the mesh
            //  View matrix contains position and rotation information for the camera
            //  Projection matrix contains camera properties such as fov, display size and clipping plane distances.
            GLES20.GlUniformMatrix4fv(_shaderProgram.GetUniformLocation(_shaderProgram.UniformNames[0]), 1, false, _modelArray, 0);
            GLES20.GlUniformMatrix4fv(_shaderProgram.GetUniformLocation(_shaderProgram.UniformNames[1]), 1, false, Camera.Instance.ViewArray, 0);
            GLES20.GlUniformMatrix4fv(_shaderProgram.GetUniformLocation(_shaderProgram.UniformNames[2]), 1, false, Camera.Instance.ProjectionArray, 0);

            // sets up texture to be used on the mesh if there is a texture
            if (_texture != null)
            {
                // "hey use slot 0 for this texture.
                GLES20.GlActiveTexture(GLES20.GlTexture0);
                //  Put this texture in the slot we're using
                GLES20.GlBindTexture(GLES20.GlTexture2d, _texture.ID);
                // When you render, The texture you need is in slot 0
                GLES20.GlUniform1i(_shaderProgram.GetUniformLocation(_shaderProgram.UniformNames[3]), 0);   
            }

            // Sets the vertex and index buffers that will be used in the next drawcall.
            GLES20.GlBindBuffer(GLES20.GlArrayBuffer, _vertexBuffer);
            GLES20.GlBindBuffer(GLES20.GlElementArrayBuffer, _indexBuffer);

            // enables attribute slots for position, UV and normal attribures.
            GLES20.GlEnableVertexAttribArray(0);
            GLES20.GlEnableVertexAttribArray(1);
            GLES20.GlEnableVertexAttribArray(2);

            // Tells the GPU what part of the vertex array contrains what info
            // Position: slot 0, 3 values, starts at byte 0
            // UV: Slot 1, 2 values, starts at byte 12
            // Normals: Slot 2, 3 values, starts at byte 20
            GLES20.GlVertexAttribPointer(0, 3, GLES20.GlFloat, false, 32, 0);
            GLES20.GlVertexAttribPointer(1, 2, GLES20.GlFloat, false, 32, 12);
            GLES20.GlVertexAttribPointer(2, 3, GLES20.GlFloat, false, 32, 20);

            // Draw!
            GLES20.GlDrawElements(GLES20.GlTriangles, _geometry.IndexArray.Length, GLES20.GlUnsignedInt, 0);

        }

        // Puts the seperate position, rotation and scale vectors into a single model matrix and converts that matrix to an array.
        private void RebuildModelArray()
        {     
            Matrix4 translation = Matrix4.CreateTranslation(_position);
            Matrix4 rotation = Matrix4.CreateFromQuaternion(_rotation);
            Matrix4 scale = Matrix4.CreateScale(_scale);

            //  Multiplies the different transfomations together. ORDER MATTERS!!!!
            //  Transformations are applied left to right and reordering will cause problems.
            Matrix4 modelMatrix = scale * rotation * translation;

            UtilLib.FillMatrix4Array(modelMatrix, _modelArray);

            _rebuildModelArray = false;
        }

        private void UploadMesh()
        {
            float[] meshArray = _geometry.MeshArray;
            int[] indexArray = _geometry.IndexArray;

            // Puts the vertex and index arrays into shared memory outside of .Net heap that GPU can access.

            var vertexData = Java.Nio.ByteBuffer        //  Creates a raw byte buffer
                .AllocateDirect(meshArray.Length * 4)   //  Byte size of our buffer is array length * 4 (4 bytes per float)
                .Order(Java.Nio.ByteOrder.NativeOrder())//  Order them in Native order whatever that is.
                .AsFloatBuffer();                       //  Pretend it's a float buffer so we can put floats in easily
            vertexData.Put(meshArray);  //  Actually upload the array to the new byte buffer
            vertexData.Position(0);     //  Reset internal caret back to 0 for when the GPU reads the data

            //  Actually upload our new byte buffer to the GPU
            GLES20.GlBindBuffer(GLES20.GlArrayBuffer, _vertexBuffer);   //  Use this buffer ID for whats to come
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
