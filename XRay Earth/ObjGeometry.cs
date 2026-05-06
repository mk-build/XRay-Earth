using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XRay_Earth
{
    internal class ObjGeometry
    {

        private float[] _geometryArray;
        private int[] _indexArray;

        public bool IsLoaded { get; private set; } = false;

        public ObjGeometry(string filename)
        {
            _ = LoadFromObj(filename);
        }

        public float[] MeshArray
        {
            get
            {
                if (!IsLoaded)
                {
                    System.Diagnostics.Debug.WriteLine("Warning: MeshArray accessed before mesh was loaded.");
                    return Array.Empty<float>();
                }
                return _geometryArray;
            }
        }

        public int[] IndexArray
        {
            get
            {
                if(!IsLoaded)
                {
                    System.Diagnostics.Debug.WriteLine("Warning: IndexArray accessed before mesh was loaded.");
                    return Array.Empty<int>();
                }
                return _indexArray;
            }   
        }


        private async Task LoadFromObj(string filename)
        {

            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
                using var reader = new StreamReader(stream);

                List<float> v = new List<float>();
                List<float> vt = new List<float>();
                List<float> vn = new List<float>();
                int nextIndex = 0;

                List<float> meshList = new List<float>();
                List<int> IndexList = new List<int>();


                Dictionary<(int vIndex, int vtIndex, int vnIndex), int> indexDictionary = new Dictionary<(int vIndex, int vtIndex, int vnIndex), int>();

                string line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.Length < 2) continue;

                    string lineCode = "" + line[0] + line[1];

                    switch (lineCode)
                    {
                        case "v ":
                            ParseVertexData(line, v);
                            break;

                        case "vn":
                            ParseVertexData(line, vn);
                            break;

                        case "vt":
                            ParseVertexData(line, vt);
                            break;

                        case "f ":
                            ParseFace(line, indexDictionary, v, vn, vt, ref nextIndex, meshList, IndexList);
                            break;
                    }

                }
                _geometryArray = meshList.ToArray();
                _indexArray = IndexList.ToArray();
                IsLoaded = true;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadFromObj error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }
           

        private void ParseVertexData(string line, List<float> list)
        {
            string[] lineElements = line.Split(' ');
            for(int i = 1;i < lineElements.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lineElements[i]))
                    list.Add(float.Parse(lineElements[i]));
            }
        }

        private void ParseFace(string line, Dictionary<(int vIndex, int vtIndex, int vnIndex), int> indexDictionary, List<float> v, List<float> vn, List<float> vt, ref int nextIndex, List<float> meshList, List<int> IndexList)
        {
            string[] lineElements = line.Split(' ');
            for(int i = 1; i < lineElements.Length; i++)
            {
                string[] faceIndicesString = lineElements[i].Split('/');

                int[] faceIndices = { int.Parse(faceIndicesString[0]), int.Parse(faceIndicesString[1]), int.Parse(faceIndicesString[2]) };

                if (indexDictionary.TryGetValue((faceIndices[0], faceIndices[1], faceIndices[2]),out int existingIndex))
                {
                    IndexList.Add(existingIndex);
                }
                else
                {
                    IndexList.Add(nextIndex);
                    indexDictionary.Add((faceIndices[0], faceIndices[1], faceIndices[2]), nextIndex++);

                    int start = (faceIndices[0] - 1) * 3;
                    meshList.Add(v[start]);
                    meshList.Add(v[start + 1]);
                    meshList.Add(v[start + 2]);

                    start = (faceIndices[1] - 1) * 2;
                    meshList.Add(vt[start]);
                    meshList.Add(vt[start + 1]);

                    start = (faceIndices[2] - 1) * 3;
                    meshList.Add(vn[start]);
                    meshList.Add(vn[start + 1]);
                    meshList.Add(vn[start + 2]);

                }
            }
        }

    }
}
