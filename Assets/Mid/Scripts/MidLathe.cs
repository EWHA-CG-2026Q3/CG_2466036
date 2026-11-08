using System.Collections.Generic;
using UnityEngine;

// 회전체(Lathe) 메시를 만드는 공용 코드
// 단면 곡선(profile)의 점 (r, y)마다, 원 위에 정점 segments개를 반복문으로 배치함
//   j번째 정점의 각도 = 360° / segments × j
//   위치 = (r·cos(각도), y, r·sin(각도))
// 이웃한 두 원(고리)의 정점을 사각형(삼각형 2개)으로 잇고,
// 바닥·윗면은 중심 정점과 원 위의 인접한 두 정점으로 삼각형을 만들어 채움 (부채꼴 방식)
public static class MidLathe
{
    // profile: 단면의 점들 (x = 반지름 r, y = 높이). 순서대로 이어 면을 만듦
    // capStart: 첫 번째 점의 원을 중심 정점으로 막음 (예: 그릇 바닥 아랫면)
    // capEnd:   마지막 점의 원을 중심 정점으로 막음 (예: 그릇 안쪽 바닥)
    // ribCount, ribAmount: 0이 아니면 각도에 따라 반지름을 살짝 바꿔 세로 주름(살)을 만듦 (등롱용)
    public static void Append(List<Vector3> vertices, List<int> triangles, Vector2[] profile,
                              int segments, bool capStart, bool capEnd,
                              int ribCount = 0, float ribAmount = 0f)
    {
        int ringStart = vertices.Count;

        // 1. 정점: 단면의 점마다 원 하나씩 (이중 반복문)
        for (int i = 0; i < profile.Length; i++)
        {
            for (int j = 0; j < segments; j++)
            {
                vertices.Add(RingPoint(profile[i], j, segments, ribCount, ribAmount));
            }
        }

        // 2. 옆면: i번째 원과 i+1번째 원 사이를 삼각형 2개씩으로 채움
        //    a ── b  (i번째 원의 j, j+1)
        //    c ── d  (i+1번째 원의 j, j+1)  → 삼각형 (a, c, d), (a, d, b)
        for (int i = 0; i < profile.Length - 1; i++)
        {
            for (int j = 0; j < segments; j++)
            {
                int jNext = (j + 1) % segments;           // 마지막 정점은 0번과 이어 원을 닫음
                int a = ringStart + i * segments + j;
                int b = ringStart + i * segments + jNext;
                int c = a + segments;
                int d = b + segments;

                triangles.Add(a); triangles.Add(c); triangles.Add(d);
                triangles.Add(a); triangles.Add(d); triangles.Add(b);
            }
        }

        // 3. 막는 면: 중심 정점 + 원 위의 인접한 두 정점
        //    (모서리가 날카롭게 보이도록 원의 정점은 따로 한 벌 더 만듦)
        if (capStart)
        {
            Vector2 p = profile[0];
            int center = vertices.Count;
            vertices.Add(new Vector3(0f, p.y, 0f));
            int ring = vertices.Count;
            for (int j = 0; j < segments; j++)
                vertices.Add(RingPoint(p, j, segments, ribCount, ribAmount));

            for (int j = 0; j < segments; j++)
            {
                triangles.Add(center);
                triangles.Add(ring + j);
                triangles.Add(ring + (j + 1) % segments);
            }
        }

        if (capEnd)
        {
            Vector2 p = profile[profile.Length - 1];
            int center = vertices.Count;
            vertices.Add(new Vector3(0f, p.y, 0f));
            int ring = vertices.Count;
            for (int j = 0; j < segments; j++)
                vertices.Add(RingPoint(p, j, segments, ribCount, ribAmount));

            for (int j = 0; j < segments; j++)
            {
                triangles.Add(ring + j);
                triangles.Add(center);
                triangles.Add(ring + (j + 1) % segments);
            }
        }
    }

    static Vector3 RingPoint(Vector2 p, int j, int segments, int ribCount, float ribAmount)
    {
        float angle = 2f * Mathf.PI / segments * j;              // 360° / n × j (라디안)
        float r = p.x;
        if (ribCount > 0)
            r *= 1f + ribAmount * Mathf.Abs(Mathf.Cos(angle * ribCount * 0.5f));
        return new Vector3(r * Mathf.Cos(angle), p.y, r * Mathf.Sin(angle));
    }

    public static Mesh ToMesh(string name, List<Vector3> vertices, List<int> triangles)
    {
        Mesh mesh = new Mesh();
        mesh.name = name;
        mesh.hideFlags = HideFlags.DontSave;   // 실행할 때마다 코드로 다시 만들므로 씬에 저장하지 않음
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
