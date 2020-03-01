using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MarchingCubes.Examples.Utilities
{
    public static class ObjExporter
    {
        private static readonly string ModelFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MarchingCubesModels");
        private const string FileName = @"MarchingCubesModel_#";

        /// <summary>
        /// Exports the gameObject's mesh to a .obj file
        /// </summary>
        /// <param name="gameObject">The GameObject whose mesh should be exported</param>
        public static void Export(GameObject gameObject)
        {
            Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3[] normals = mesh.normals;

            List<string> fileLines = new List<string>();
            for (int i = 0; i < vertices.Length; i++)
            {
                string vertex = vertices[i].x + " " + vertices[i].y + " " + vertices[i].z;
                fileLines.Add("v " + vertex.Replace(",", "."));
            }

            for (int i = 0; i < normals.Length; i++)
            {
                string normal = normals[i].x + " " + normals[i].y + " " + normals[i].z;
                fileLines.Add("vn " + normal.Replace(",", "."));
            }

            for (int i = 0; i < triangles.Length; i += 3)
            {
                string v1 = $"{triangles[i + 0] + 1}//{triangles[i + 0] + 1}";
                string v2 = $"{triangles[i + 1] + 1}//{triangles[i + 1] + 1}";
                string v3 = $"{triangles[i + 2] + 1}//{triangles[i + 2] + 1}";

                string triangle = v1 + " " + v2 + " " + v3;
                fileLines.Add("f " + triangle);
            }

            int fileCount = Directory.GetFiles(ModelFolderPath, "*.obj").Length;
            string path = Path.Combine(ModelFolderPath, FileName.Replace("#", fileCount.ToString())) + ".obj";

            File.WriteAllLines(path, fileLines);
            Debug.Log("Exported " + gameObject.name + " to " + path);
        }
    }
}