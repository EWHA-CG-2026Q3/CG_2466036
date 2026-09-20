using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class S04_CustomDiamond : MonoBehaviour
{
    void Start()
    {
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),       // v0 
            new Vector3(0f, 0f, 1f),       // v1 
            new Vector3(1f, 0f, 1f),       // v2 
            new Vector3(1f, 0f, 0f),       // v3 
            new Vector3(0.5f, 1f, 0.5f),   // v4 
            new Vector3(0.5f, -1f, 0.5f),  // v5 
        };

        int[] triangles = new int[]
        {
            0, 1, 4,
            1, 2, 4,
            2, 3, 4,
            3, 0, 4,

            1, 0, 5,
            2, 1, 5,
            3, 2, 5,
            0, 3, 5
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }
}