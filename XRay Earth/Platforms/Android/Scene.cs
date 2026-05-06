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
                gl_FragColor = texture2D(uTexture,vUV);                
            }";

            string[] attributeNames = { "aPosition","aUV","aNormal" };
            string[] uniformNames = { "uModel", "uView", "uProjection", "uTexture" };


            ShaderProgram shaderProgram = new ShaderProgram(VertexShaderSource, FragmentShaderSource, attributeNames, uniformNames);
            Texture texture = new Texture("uv_grid.png");

            ObjGeometry monke_geo = new ObjGeometry("monke.obj");
            Mesh Monke_mesh = new Mesh(monke_geo, shaderProgram, texture);
            Monke_mesh.Position = new Vector3(2, 0, 0);
            AddMeshToRenderQueue(Monke_mesh);

            ObjGeometry donut_geo = new ObjGeometry("donut.obj");
            Mesh Donut_mesh = new Mesh(donut_geo, shaderProgram, texture);
            Donut_mesh.Position = new Vector3(-2, 0, 0);
            AddMeshToRenderQueue(Donut_mesh);


        }

        public void AddMeshToRenderQueue(Mesh mesh)
        {
            _renderQueue.Add(mesh);
            _renderQueue = _renderQueue.OrderBy(r => r.ShaderProgram.ProgramID).ToList();

        }
    }
}
