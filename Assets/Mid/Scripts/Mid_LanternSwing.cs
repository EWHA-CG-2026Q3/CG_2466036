using System.Collections.Generic;
using UnityEngine;

// [필수 변환] 처마에 매달린 등롱이 바람에 흔들리는 계층 구조 (행렬을 직접 작성해 곱함)
//
//   끈(Cord)  ── 매단 점(이 오브젝트의 위치)을 축으로 흔들림
//    └ 등롱(Frame + Body + Light) ── 끈 끝에 붙어 조금 늦게 흔들리고, 천천히 돌아감
//       └ 술(Tassel) ── 등롱 아래에 붙어 더 크게, 더 늦게 흔들림
//
//   M_cord    = Rz(θ1) · Rx(φ1)
//   M_lantern = M_cord · T(0, -끈 길이, 0) · Rz(θ2) · Ry(회전)
//   M_tassel  = M_lantern · T(0, -등롱 높이, 0) · Rz(θ3) · Rx(φ3)
//
// 부모 행렬을 오른쪽에 계속 곱해 가므로, 부모가 움직이면 자식이 함께 따라 움직임
// 각 부품의 기본 정점(코드로 만든 회전체 메시)에 행렬을 한 번씩 곱해서 매 프레임 정점 위치를 다시 계산함
// (자식 오브젝트들의 Transform은 기본값 그대로 두고, Unity Transform이 아니라 이 행렬로만 움직임)
[ExecuteAlways]
public class Mid_LanternSwing : MonoBehaviour
{
    [Header("부품 (자식 오브젝트)")]
    [SerializeField] MeshFilter cord;
    [SerializeField] MeshFilter frame;
    [SerializeField] MeshFilter body;
    [SerializeField] MeshFilter tassel;
    [SerializeField] Transform lanternLight;

    [Header("모양")]
    [SerializeField] float cordLength = 0.9f;

    [Header("흔들림")]
    [SerializeField] float speed = 1.6f;          // 흔들리는 빠르기 (rad/s)
    [SerializeField] float phase = 0f;            // 등롱마다 다르게 주면 따로 흔들림
    [SerializeField] float cordAngle = 6f;        // θ1 크기 (도)
    [SerializeField] float lanternAngle = 9f;     // θ2 크기 (도)
    [SerializeField] float tasselAngle = 18f;     // θ3 크기 (도)
    [SerializeField] float spinAngle = 20f;       // 등롱이 좌우로 도는 크기 (도)

    const float LanternHeight = 0.37f;            // 등롱 윗면(0)부터 아랫면까지

    // 부품마다: 기본 정점·법선(움직이기 전)과, 행렬을 곱한 결과를 넣을 메시
    class Part
    {
        public MeshFilter filter;
        public Vector3[] baseVertices, baseNormals, vertices, normals;
        public Mesh mesh;
    }

    readonly List<Part> parts = new List<Part>();
    Part cordPart, framePart, bodyPart, tasselPart;

    void OnEnable()
    {
        BuildParts();
        Apply(0f);
    }

    void OnDisable()
    {
        foreach (Part p in parts)
        {
            if (p.mesh == null) continue;
            if (Application.isPlaying) Destroy(p.mesh); else DestroyImmediate(p.mesh);
        }
        parts.Clear();
    }

    void Update()
    {
        if (parts.Count == 0) return;
        Apply(Application.isPlaying ? Time.time : 0f);
    }

    // ---------- 1. 부품 메시 만들기 (모두 반복문 회전체) ----------
    void BuildParts()
    {
        OnDisable();

        // 끈: 매단 점(0)에서 아래로 cordLength만큼
        cordPart = MakePart(cord, "Lantern_Cord", (v, t) =>
            MidLathe.Append(v, t, new[] { new Vector2(0.005f, -cordLength), new Vector2(0.005f, 0f) },
                            8, false, false));

        // 등롱 틀: 위 뚜껑 + 아래 받침 (나무)
        framePart = MakePart(frame, "Lantern_Frame", (v, t) =>
        {
            MidLathe.Append(v, t, new[] { new Vector2(0.075f, -0.045f), new Vector2(0.075f, 0f) }, 24, true, true);
            MidLathe.Append(v, t, new[] { new Vector2(0.085f, -LanternHeight), new Vector2(0.085f, -0.34f) }, 24, true, true);
        });

        // 등롱 몸통: 가운데가 불룩한 한지 등롱, 세로 살 12개
        bodyPart = MakePart(body, "Lantern_Body", (v, t) =>
            MidLathe.Append(v, t, new[]
            {
                new Vector2(0.070f, -0.340f),
                new Vector2(0.108f, -0.310f),
                new Vector2(0.132f, -0.245f),
                new Vector2(0.138f, -0.190f),
                new Vector2(0.128f, -0.115f),
                new Vector2(0.098f, -0.065f),
                new Vector2(0.066f, -0.045f),
            }, 48, true, true, ribCount: 12, ribAmount: 0.05f));

        // 술: 매듭 + 아래로 퍼지는 술
        tasselPart = MakePart(tassel, "Lantern_Tassel", (v, t) =>
        {
            MidLathe.Append(v, t, new[]
            {
                new Vector2(0.004f, -0.035f), new Vector2(0.018f, -0.024f),
                new Vector2(0.018f, -0.012f), new Vector2(0.004f, 0f),
            }, 16, false, false);
            MidLathe.Append(v, t, new[]
            {
                new Vector2(0.032f, -0.230f), new Vector2(0.016f, -0.080f), new Vector2(0.007f, -0.035f),
            }, 16, true, false);
        });
    }

    Part MakePart(MeshFilter filter, string name, System.Action<List<Vector3>, List<int>> build)
    {
        if (filter == null) return null;

        var v = new List<Vector3>();
        var t = new List<int>();
        build(v, t);

        Part p = new Part { filter = filter };
        p.mesh = MidLathe.ToMesh(name, v, t);
        p.mesh.MarkDynamic();
        p.baseVertices = p.mesh.vertices;
        p.baseNormals = p.mesh.normals;
        p.vertices = new Vector3[p.baseVertices.Length];
        p.normals = new Vector3[p.baseNormals.Length];
        filter.sharedMesh = p.mesh;
        parts.Add(p);
        return p;
    }

    // ---------- 2. 시간에 따라 행렬을 만들어 부모부터 차례로 곱하기 ----------
    void Apply(float time)
    {
        float w = time * speed + phase;

        float theta1 = cordAngle * Mathf.Sin(w);
        float phi1 = cordAngle * 0.5f * Mathf.Sin(w * 0.7f + 1.3f);
        float theta2 = lanternAngle * Mathf.Sin(w - 0.8f);              // 끈보다 조금 늦게
        float spin = spinAngle * Mathf.Sin(w * 0.35f);
        float theta3 = tasselAngle * Mathf.Sin(w - 1.6f);               // 등롱보다 더 늦게
        float phi3 = tasselAngle * 0.5f * Mathf.Sin(w * 0.7f - 0.4f);

        Matrix4x4 mCord = RotateZ(theta1) * RotateX(phi1);
        Matrix4x4 mLantern = mCord * Translate(0f, -cordLength, 0f) * RotateZ(theta2) * RotateY(spin);
        Matrix4x4 mTassel = mLantern * Translate(0f, -LanternHeight, 0f) * RotateZ(theta3) * RotateX(phi3);

        ApplyMatrix(cordPart, mCord);
        ApplyMatrix(framePart, mLantern);
        ApplyMatrix(bodyPart, mLantern);
        ApplyMatrix(tasselPart, mTassel);

        // 불빛도 등롱 몸통 가운데를 따라감
        if (lanternLight != null)
            lanternLight.localPosition = mLantern.MultiplyPoint3x4(new Vector3(0f, -0.19f, 0f));
    }

    // 기본 정점마다 행렬 M을 곱함 (법선은 방향만 바뀌므로 회전 부분만 곱함)
    static void ApplyMatrix(Part p, Matrix4x4 M)
    {
        if (p == null) return;
        for (int i = 0; i < p.baseVertices.Length; i++)
        {
            p.vertices[i] = M.MultiplyPoint3x4(p.baseVertices[i]);
            p.normals[i] = M.MultiplyVector(p.baseNormals[i]);
        }
        p.mesh.vertices = p.vertices;
        p.mesh.normals = p.normals;
        p.mesh.RecalculateBounds();
    }

    // ---------- 3. 직접 작성한 4×4 변환 행렬 ----------
    //  이동            x축 회전              y축 회전              z축 회전
    //  [1 0 0 x]      [1  0   0  0]        [ c  0  s  0]        [c -s  0  0]
    //  [0 1 0 y]      [0  c  -s  0]        [ 0  1  0  0]        [s  c  0  0]
    //  [0 0 1 z]      [0  s   c  0]        [-s  0  c  0]        [0  0  1  0]
    //  [0 0 0 1]      [0  0   0  1]        [ 0  0  0  1]        [0  0  0  1]
    static Matrix4x4 Translate(float x, float y, float z)
    {
        Matrix4x4 m = Matrix4x4.identity;
        m.m03 = x; m.m13 = y; m.m23 = z;
        return m;
    }

    static Matrix4x4 RotateX(float degrees)
    {
        float r = degrees * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r);
        Matrix4x4 m = Matrix4x4.identity;
        m.m11 = c; m.m12 = -s;
        m.m21 = s; m.m22 = c;
        return m;
    }

    static Matrix4x4 RotateY(float degrees)
    {
        float r = degrees * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r);
        Matrix4x4 m = Matrix4x4.identity;
        m.m00 = c; m.m02 = s;
        m.m20 = -s; m.m22 = c;
        return m;
    }

    static Matrix4x4 RotateZ(float degrees)
    {
        float r = degrees * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r);
        Matrix4x4 m = Matrix4x4.identity;
        m.m00 = c; m.m01 = -s;
        m.m10 = s; m.m11 = c;
        return m;
    }
}
