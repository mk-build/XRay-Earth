using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace XRay_Earth.Platforms.Android
{
    internal class Scene
    {
        public static Scene Instance { get; } = new Scene();
        private Scene() { }

        private List<Mesh> _renderQueue = new List<Mesh>();

        private float _latitude = 0f;
        private float _longitude = 0f;
        private float _altitude = 0f;

        public float Latitude
        {
            get { return _latitude; }
            set { _latitude = value; }
        }
        public float Longitude
        {
            get { return _longitude; }
            set { _longitude = value; }
        }
        public float Altitude
        {
            get { return _altitude; }
            set { _altitude = value; }
        }

        public IReadOnlyList<Mesh> RenderQueue
        {
            get { return _renderQueue; }
        }

        public void Initialize()
        {

            const string VertexShaderSource = @"
            attribute vec4 aPosition;
            attribute vec2 aUV;
            varying vec2 vUV;
            uniform mat4 uModel;
            uniform mat4 uView;
            uniform mat4 uProjection;
            void main() {
                gl_Position = uProjection * uView * uModel * aPosition;
                vUV = aUV;
            }";

            const string FragmentShaderSource = @"
            precision mediump float;
            varying vec2 vUV;
            uniform sampler2D uTexture;
            void main() {
                gl_FragColor = texture2D(uTexture, vec2(1.0 - vUV.x, 1.0 - vUV.y));                
            }";

            string[] attributeNames = { "aPosition","aUV","aNormal" };
            string[] uniformNames = { "uModel", "uView", "uProjection", "uTexture" };


            ShaderProgram shaderProgram = new ShaderProgram(VertexShaderSource, FragmentShaderSource, attributeNames, uniformNames);
            Texture uv_grid = new Texture("uv_grid.png");

            Texture worldmap_Texture_A1 = new Texture("worldmap_4k_A1.png");
            Texture worldmap_Texture_B1 = new Texture("worldmap_4k_B1.png");
            Texture worldmap_Texture_C1 = new Texture("worldmap_4k_C1.png");
            Texture worldmap_Texture_D1 = new Texture("worldmap_4k_D1.png");

            Texture worldmap_Texture_A2 = new Texture("worldmap_4k_A2.png");
            Texture worldmap_Texture_B2 = new Texture("worldmap_4k_B2.png");
            Texture worldmap_Texture_C2 = new Texture("worldmap_4k_C2.png");
            Texture worldmap_Texture_D2 = new Texture("worldmap_4k_D2.png");

            Vector3 northPosition = new Vector3(0, 0, 0);
            Vector3 southPosition = new Vector3(0, 0, 0);

            Quaternion rot0 = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(0));
            Quaternion rot90 = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(-90));
            Quaternion rot180 = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(-180));
            Quaternion rot270 = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(-270));

            ObjGeometry earth_North_Geo = new ObjGeometry("earthNorth.obj");
            ObjGeometry earth_South_Geo = new ObjGeometry("earthSouth.obj");

            Mesh earth_North_Mesh_1 = new Mesh(earth_North_Geo, shaderProgram, worldmap_Texture_A1);
            Mesh earth_North_Mesh_2 = new Mesh(earth_North_Geo, shaderProgram, worldmap_Texture_B1);
            Mesh earth_North_Mesh_3 = new Mesh(earth_North_Geo, shaderProgram, worldmap_Texture_C1);
            Mesh earth_North_Mesh_4 = new Mesh(earth_North_Geo, shaderProgram, worldmap_Texture_D1);

            earth_North_Mesh_1.Position = northPosition;
            earth_North_Mesh_2.Position = northPosition;
            earth_North_Mesh_3.Position = northPosition;
            earth_North_Mesh_4.Position = northPosition;

            earth_North_Mesh_1.Rotation = rot0;
            earth_North_Mesh_2.Rotation = rot90;
            earth_North_Mesh_3.Rotation = rot180;
            earth_North_Mesh_4.Rotation = rot270;

            AddMeshToRenderQueue(earth_North_Mesh_1);
            AddMeshToRenderQueue(earth_North_Mesh_2);
            AddMeshToRenderQueue(earth_North_Mesh_3);
            AddMeshToRenderQueue(earth_North_Mesh_4);

            Mesh earth_South_Mesh_1 = new Mesh(earth_South_Geo, shaderProgram, worldmap_Texture_A2);
            Mesh earth_South_Mesh_2 = new Mesh(earth_South_Geo, shaderProgram, worldmap_Texture_B2);
            Mesh earth_South_Mesh_3 = new Mesh(earth_South_Geo, shaderProgram, worldmap_Texture_C2);
            Mesh earth_South_Mesh_4 = new Mesh(earth_South_Geo, shaderProgram, worldmap_Texture_D2);

            earth_South_Mesh_1.Position = southPosition;
            earth_South_Mesh_2.Position = southPosition;
            earth_South_Mesh_3.Position = southPosition;
            earth_South_Mesh_4.Position = southPosition;

            earth_South_Mesh_1.Rotation = rot0;
            earth_South_Mesh_2.Rotation = rot90;
            earth_South_Mesh_3.Rotation = rot180;
            earth_South_Mesh_4.Rotation = rot270;

            AddMeshToRenderQueue(earth_South_Mesh_1);
            AddMeshToRenderQueue(earth_South_Mesh_2);
            AddMeshToRenderQueue(earth_South_Mesh_3);
            AddMeshToRenderQueue(earth_South_Mesh_4);



            //ObjGeometry monkey_Geo = new ObjGeometry("monkey.obj");
            //Mesh monkey_Mesh = new Mesh(monkey_Geo, shaderProgram, texture);
            //monkey_Mesh.Position = northPosition;
            //monkey_Mesh.Scale = new Vector3(0.25f, 0.25f, 0.25f);
            //AddMeshToRenderQueue(monkey_Mesh);


        }

        public void AddMeshToRenderQueue(Mesh mesh)
        {
            _renderQueue.Add(mesh);
            _renderQueue = _renderQueue.OrderBy(r => r.ShaderProgram.ProgramID).ToList();

        }
    }
}
