using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class S03_CustomPolygonMesh_Square : MonoBehaviour
{
    void Start()
    {
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),    // v0
            new Vector3(0f, 0f, 1f),    // v1
            new Vector3(0f, 1f, 0f),    // v2
            new Vector3(0f, 1f, 1f),    // v3
            new Vector3(1f, 1f, 0f),    // v4
            new Vector3(1f, 1f, 1f),    // v5
            new Vector3(1f, 0f, 0f),    // v6
            new Vector3(1f, 0f, 1f),    // v7
        };

        int[] triangles = new int[]
        {
            0, 2, 4,
            0, 4, 6
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }
}