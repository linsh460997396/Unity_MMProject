using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteSpace.Test
{
    public struct Face
    {
        public List<Vector3> vertices { get; private set; }
        public List<int> triangles { get; private set; }
        public List<Vector2> uvs { get; private set; }
        public Face(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            this.vertices = vertices;
            this.triangles = triangles;
            this.uvs = uvs;
        }
    }

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class HexRenderer : MonoBehaviour
    {
        private Mesh m_mesh;
        private MeshFilter m_meshFilter;
        private MeshRenderer m_meshRenderer;

        private List<Face> m_faces;

        public Material material;
        public float innerSize = 0f;
        public float outerSize = 1f;
        public float height = 0f;
        public bool isFlatTopped = false;

        private void Awake()
        {
            m_mesh = new Mesh();
            m_mesh.name = "HexMesh";

            m_meshFilter = GetComponent<MeshFilter>();
            m_meshFilter.mesh = m_mesh;

            m_meshRenderer = GetComponent<MeshRenderer>();
            m_meshRenderer.material = material;
        }

        private void OnEnable()
        {
            DrawMesh();
        }

        public void OnValidate()
        {

            if (Application.isPlaying && m_mesh != null) DrawMesh();
        }

        public void DrawMesh()
        {
            DrawFaces();
            CombineFaces();
        }

        private void CombineFaces()
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i < m_faces.Count; i++)
            {
                //Add the vertices
                vertices.AddRange(m_faces[i].vertices);
                uvs.AddRange(m_faces[i].uvs);

                //Offset the triangles
                int offset = (4 * i);
                foreach (int triangle in m_faces[i].triangles)
                {
                    tris.Add(triangle + offset);
                }
            }

            m_mesh.vertices = vertices.ToArray();
            m_mesh.triangles = tris.ToArray();
            m_mesh.uv = uvs.ToArray();
            m_mesh.RecalculateNormals();

        }

        private void DrawFaces()
        {
            m_faces = new List<Face>();

            //Top faces
            for (int point = 0; point < 6; point++)
            {
                m_faces.Add(CreateFace(innerSize, outerSize, height / 2f, height / 2f, point));
            }

            //Bottom faces
            for (int point = 0; point < 6; point++)
            {
                m_faces.Add(CreateFace(innerSize, outerSize, -height / 2f, -height / 2f, point, true));
            }

            //Outer faces
            for (int point = 0; point < 6; point++)
            {
                m_faces.Add(CreateFace(outerSize, outerSize, height / 2f, -height / 2f, point, true));
            }

            //Inner faces
            for (int point = 0; point < 6; point++)
            {
                m_faces.Add(CreateFace(innerSize, innerSize, -height / 2f, height / 2f, point, false));
            }
        }

        protected Vector3 GetPoint(float size, float height, int index)
        {
            //float angle_deg = 60 * index;
            float angle_deg = isFlatTopped ? 60 * index : 60 * index - 30;
            float angle_rad = Mathf.PI / 180f * angle_deg;
            return new Vector3((size * Mathf.Cos(angle_rad)), height, size * Mathf.Sin(angle_rad));
        }

        private Face CreateFace(float innerRad, float outerRad, float heightA, float heightB, int point, bool reverse = false)
        {
            Vector3 pointA = GetPoint(innerRad, heightB, point);
            Vector3 pointB = GetPoint(innerRad, heightB, (point < 5) ? point + 1 : 0);
            Vector3 pointC = GetPoint(outerRad, heightA, (point < 5) ? point + 1 : 0);
            Vector3 pointD = GetPoint(outerRad, heightA, point);

            List<Vector3> vertices = new List<Vector3>() { pointA, pointB, pointC, pointD };
            List<int> triangles = new List<int>() { 0, 1, 2, 2, 3, 0 };
            List<Vector2> uvs = new List<Vector2>() { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
            if (reverse) vertices.Reverse();
            return new Face(vertices, triangles, uvs);
        }
    }
}

