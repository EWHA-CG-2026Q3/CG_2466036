using UnityEngine;
using UnityEngine.Rendering;

// S17 텍스처 실습용: 같은 오브젝트의 메시에 UV가 없으면 UV를 붙여 줌
// 메시를 만드는 스크립트(예: S10 DiamondMesh_Rewinded)는 그대로 두고, 이 컴포넌트만 추가해서 사용
// UV = 위에서 내려다본 정점 위치 (x, z)  → 정점이 0~1 범위면 그림 전체가 한 번 펼쳐짐
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
public class S17_PlanarUV : MonoBehaviour
{
    void Update()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null || mesh.HasVertexAttribute(VertexAttribute.TexCoord0)) return;   // 이미 UV가 있으면 그대로

        Vector3[] vertices = mesh.vertices;
        Vector2[] uv = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            uv[i] = new Vector2(vertices[i].x, vertices[i].z);
        mesh.uv = uv;
    }
}
