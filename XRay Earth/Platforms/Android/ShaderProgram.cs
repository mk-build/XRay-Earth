using Android.Opengl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XRay_Earth
{
    internal class ShaderProgram
    {

        private Dictionary<string, int> _uniformDicttionary = new Dictionary<string, int>();
        private string[] _uniformNames;

        private int _programID;

        public int ProgramID
        {
            get { return _programID; }
        }


        public ShaderProgram(string vertexShaderSource, string fragementShaderSource, string[] attributeNames, string[] uniformNames = null)
        {
            _uniformNames = uniformNames ?? new string[] { "uModel","uView","uProjection" };
            _programID = BuildProgram(vertexShaderSource, fragementShaderSource, attributeNames);
        }

        public string[] UniformNames
        {
            get { return _uniformNames; }
        }

        public int GetUniformLocation(string uniform)
        {
            int location;

            if (_uniformDicttionary.TryGetValue(uniform, out location))
            {
                return location;
            }
            else
            {
                location = GLES20.GlGetUniformLocation(_programID, uniform);
                _uniformDicttionary[uniform] = location;

                if (location == -1)
                    System.Diagnostics.Debug.WriteLine($"Warning: uniform '{uniform}' not found in shader.");

                return location;
            }
        }


        private int BuildProgram(string vertexSource, string fragmentSource, string[] attributeNames)
        {
            int vertexShader = CompileShader(GLES20.GlVertexShader, vertexSource);
            int fragmentShader = CompileShader(GLES20.GlFragmentShader, fragmentSource);

            int program = GLES20.GlCreateProgram();
            GLES20.GlAttachShader(program, vertexShader);
            GLES20.GlAttachShader(program, fragmentShader);

            for(int i = 0; i < attributeNames.Length; i++)
            {
                GLES20.GlBindAttribLocation(program, i, attributeNames[i]);
            }

            GLES20.GlLinkProgram(program);

            GLES20.GlDetachShader(program, vertexShader);
            GLES20.GlDetachShader(program, fragmentShader);
            GLES20.GlDeleteShader(vertexShader);
            GLES20.GlDeleteShader(fragmentShader);

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
    }
}
