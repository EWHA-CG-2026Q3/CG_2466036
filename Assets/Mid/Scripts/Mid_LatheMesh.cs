using System.Collections.Generic;
using UnityEngine;

// [필수 기하 모델링] 반복문으로 만든 회전체 메시: 찻잔, 달항아리
// 단면 곡선의 점마다 원 위에 정점을 반복문으로 배치해 만든 그릇 모양 (MidLathe.cs 참고)
// 바깥 벽 → 입술(전) → 안쪽 벽 → 안쪽 바닥 순서로 단면을 그려서, 속이 빈 그릇이 됨
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_LatheMesh : MonoBehaviour
{
    public enum Shape { TeaCup, MoonJar }

    [SerializeField] Shape shape = Shape.TeaCup;
    [SerializeField, Range(8, 96)] int segments = 48;   // 원 하나에 놓을 정점 수 n

    Mesh mesh;

    // 찻잔: 굽(발) → 둥근 몸통 → 입술 → 안쪽 → 안쪽 바닥 (단위: m)
    static readonly Vector2[] TeaCupProfile =
    {
        new Vector2(0.020f, 0.000f),  // 굽 바닥
        new Vector2(0.022f, 0.006f),  // 굽
        new Vector2(0.019f, 0.008f),  // 굽 위 잘록한 곳
        new Vector2(0.028f, 0.014f),
        new Vector2(0.036f, 0.026f),
        new Vector2(0.041f, 0.040f),
        new Vector2(0.043f, 0.047f),  // 입술 바깥
        new Vector2(0.040f, 0.048f),  // 입술 안쪽
        new Vector2(0.037f, 0.040f),
        new Vector2(0.031f, 0.026f),
        new Vector2(0.022f, 0.015f),
        new Vector2(0.012f, 0.011f),  // 안쪽 바닥 (여기서 중심으로 막음)
    };

    // 달항아리: 넓은 굽 → 둥근 배 → 좁은 목 → 살짝 벌어진 입 → 안쪽
    static readonly Vector2[] MoonJarProfile =
    {
        new Vector2(0.045f, 0.000f),
        new Vector2(0.060f, 0.012f),
        new Vector2(0.088f, 0.040f),
        new Vector2(0.108f, 0.080f),
        new Vector2(0.115f, 0.120f),
        new Vector2(0.110f, 0.155f),
        new Vector2(0.092f, 0.190f),
        new Vector2(0.068f, 0.220f),
        new Vector2(0.062f, 0.238f),
        new Vector2(0.068f, 0.252f),  // 입 바깥
        new Vector2(0.061f, 0.254f),  // 입 안쪽
        new Vector2(0.055f, 0.238f),
        new Vector2(0.062f, 0.215f),
        new Vector2(0.085f, 0.185f),
        new Vector2(0.100f, 0.150f),
        new Vector2(0.104f, 0.115f),
        new Vector2(0.095f, 0.075f),
        new Vector2(0.070f, 0.035f),
        new Vector2(0.040f, 0.015f),  // 안쪽 바닥
    };

    void OnEnable() => Build();
    void OnValidate() { if (isActiveAndEnabled) needsRebuild = true; }

    bool needsRebuild;
    void Update()
    {
        if (!needsRebuild) return;
        needsRebuild = false;
        Build();
    }

    void Build()
    {
        Vector2[] profile = shape == Shape.TeaCup ? TeaCupProfile : MoonJarProfile;

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        MidLathe.Append(vertices, triangles, profile, segments, capStart: true, capEnd: true);

        Release();
        mesh = MidLathe.ToMesh(shape.ToString(), vertices, triangles);
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void OnDisable() => Release();

    void Release()
    {
        if (mesh == null) return;
        if (Application.isPlaying) Destroy(mesh); else DestroyImmediate(mesh);
        mesh = null;
    }
}
