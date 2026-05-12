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


        public ShaderProgram(string vertexShaderSource, string fragementShaderSource, string[] attributeNames, string[] uniformNames)
        {
            _uniformNames = uniformNames;
            _programID = BuildProgram(vertexShaderSource, fragementShaderSource, attributeNames);
        }

        public string[] UniformNames
        {
            get { return _uniformNames; }
        }


        // Gets uniform locations from the GPU as needed.
        // Saves them to a dictionary to avoid repeated GPU calls.
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

            // Create a shader program and attach both compiled shaders to it —
            // the program is the complete vertex+fragment pipeline used for drawing
            int program = GLES20.GlCreateProgram();
            GLES20.GlAttachShader(program, vertexShader);
            GLES20.GlAttachShader(program, fragmentShader);

            // Bind each attribute name to a slot number — must match the slots
            // used in GlVertexAttribPointer and the attribute declarations in the vertex shader
            for (int i = 0; i < attributeNames.Length; i++)
            {
                GLES20.GlBindAttribLocation(program, i, attributeNames[i]);
            }

            // Compiles vertex and fragment shader programs into a complete shader program
            GLES20.GlLinkProgram(program);

            // Clears seperated shader programs from GPU memory.
            // We only need the final compiled shader from here on
            GLES20.GlDetachShader(program, vertexShader);
            GLES20.GlDetachShader(program, fragmentShader);
            GLES20.GlDeleteShader(vertexShader);
            GLES20.GlDeleteShader(fragmentShader);

            return program;
        }

        private int CompileShader(int type, string source)
        {

            // Create a shader object on the GPU (vertex or fragment depending on type),
            // load the GLSL source into it, and compile it
            int shader = GLES20.GlCreateShader(type);   
            GLES20.GlShaderSource(shader, source);  
            GLES20.GlCompileShader(shader);

            // Check if compilation succeeded — if not, fetch the error log from the GPU and throw
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
