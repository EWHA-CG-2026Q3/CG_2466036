using UnityEngine;

// S12 StillLife용 삼각기둥 메시 (S03–S04 방식: 정점 배열 + 삼각형 배열)
// 앞 삼각형(z = 0) 3개 + 뒤 삼각형(z = 1) 3개 = 정점 6개, 삼각형 8개 (앞뒤 2 + 옆면 6)
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class S12_CustomPrismMesh : MonoBehaviour
{
    void Start()
    {
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0f,   0f, 0f),   // v0 앞 왼쪽 아래
            new Vector3(1f,   0f, 0f),   // v1 앞 오른쪽 아래
            new Vector3(0.5f, 1f, 0f),   // v2 앞 꼭대기
            new Vector3(0f,   0f, 1f),   // v3 뒤 왼쪽 아래
            new Vector3(1f,   0f, 1f),   // v4 뒤 오른쪽 아래
            new Vector3(0.5f, 1f, 1f),   // v5 뒤 꼭대기
        };

        int[] triangles = new int[]
        {
            0, 2, 1,            // 앞면 (z = 0)
            3, 4, 5,            // 뒷면 (z = 1)
            0, 3, 5,  0, 5, 2,  // 왼쪽 비탈면
            1, 2, 5,  1, 5, 4,  // 오른쪽 비탈면
            0, 1, 4,  0, 4, 3   // 바닥
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }
}
