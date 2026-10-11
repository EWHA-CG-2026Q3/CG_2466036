using UnityEngine;

// 과제: 높이(y)에 비례해 x축 방향으로 밀리는 기울이기(shear) 4×4 행렬
// 점 (x, y, z) → (x + k·y, y, z)
// k = (학번 끝자리 + 1) ÷ 5 = (6 + 1) ÷ 5 = 1.4
// (이 오브젝트의 Transform은 기본값 Position 0, Rotation 0, Scale 1을 유지)
[ExecuteAlways]
[RequireComponent(typeof(DiamondMesh))]
public class S09_Shear : MonoBehaviour
{
    [SerializeField] float k = 1.4f;   // 기울이기 정도 (−k 확인 시 −1.4 입력)

    DiamondMesh diamondMesh;

    void OnEnable()
    {
        diamondMesh = GetComponent<DiamondMesh>();
    }

    void Update()
    {
        if (diamondMesh == null || diamondMesh.BaseVertices == null) return;

        float[,] H = ShearMatrixRaw(k);
        Vector3[] baseVertices = diamondMesh.BaseVertices;
        Vector3[] verts = new Vector3[baseVertices.Length];
        for (int i = 0; i < baseVertices.Length; i++)
            verts[i] = FromHomogeneous(MultiplyMatrixVectorRaw(H, ToHomogeneous(baseVertices[i])));
        diamondMesh.SetVertices(verts);
    }

    // Inspector 값을 바꿀 때마다 꼭대기 정점의 결과를 Console에 출력
    void OnValidate()
    {
        Vector3 top = new Vector3(0.5f, 1f, 0.5f);
        Vector3 result = FromHomogeneous(MultiplyMatrixVectorRaw(ShearMatrixRaw(k), ToHomogeneous(top)));
        Debug.Log($"[Shear k = {k}] 꼭대기 정점 {top} → {result}");
    }

    // 1열: e₁ 그대로 / 2열: e₂ → (k, 1, 0) / 3열: e₃ 그대로 / 4열: 원점 그대로
    float[,] ShearMatrixRaw(float k)
    {
        return new float[,] {
            { 1f, k,  0f, 0f },
            { 0f, 1f, 0f, 0f },
            { 0f, 0f, 1f, 0f },
            { 0f, 0f, 0f, 1f }
        };
    }

    Vector4 ToHomogeneous(Vector3 v)
    {
        return new Vector4(v.x, v.y, v.z, 1f);
    }

    Vector3 FromHomogeneous(Vector4 h)
    {
        return new Vector3(h.x, h.y, h.z);
    }

    Vector4 MultiplyMatrixVectorRaw(float[,] M, Vector4 v)
    {
        float[] input = { v.x, v.y, v.z, v.w };
        float[] result = new float[4];
        for (int row = 0; row < 4; row++)
            for (int col = 0; col < 4; col++)
                result[row] += M[row, col] * input[col];
        return new Vector4(result[0], result[1], result[2], result[3]);
    }
}
