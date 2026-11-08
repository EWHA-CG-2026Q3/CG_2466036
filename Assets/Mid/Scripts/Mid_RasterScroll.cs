using UnityEngine;

// [선택 요소: 래스터화] 직접 작성한 소프트웨어 래스터라이저로 그린 족자 그림 (달밤 산수화)
// 모든 그림은 삼각형으로만 이루어지고, 삼각형을 픽셀로 채우는 과정을 직접 계산함 (S06 방식 확장)
//   1. 삼각형의 경계 상자 안의 픽셀만 검사
//   2. 픽셀 중심의 무게중심 좌표(barycentric) w1, w2, w3로 삼각형 안인지 판정
//   3. 꼭짓점 색 세 개를 w1, w2, w3로 섞어 픽셀 색을 정함 (그러데이션)
//   4. 깊이 버퍼: 더 가까운(깊이 값이 작은) 삼각형만 덮어씀 → 그리는 순서와 상관없이 앞뒤가 맞음
// 완성된 텍스처를 이 오브젝트(Quad)의 재질에 붙여 정자 안에 걸어 둠
[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class Mid_RasterScroll : MonoBehaviour
{
    [SerializeField] int width = 240;
    [SerializeField] int height = 400;

    Texture2D texture;
    Color[] pixels;
    float[] depth;
    MaterialPropertyBlock block;

    // 깊이 값: 작을수록 앞
    const float ZFrame = 0.0f, ZSeal = 0.1f, ZPine = 0.2f, ZNear = 0.3f, ZWater = 0.45f,
                ZMid = 0.5f, ZFar = 0.7f, ZMoon = 0.8f, ZSky = 1.0f;

    void OnEnable() => Draw();
    void OnValidate() { if (isActiveAndEnabled) needsRedraw = true; }

    bool needsRedraw;
    void Update()
    {
        if (!needsRedraw) return;
        needsRedraw = false;
        Draw();
    }

    void OnDisable()
    {
        if (texture == null) return;
        if (Application.isPlaying) Destroy(texture); else DestroyImmediate(texture);
        texture = null;
    }

    // ---------- 그림 그리기 ----------
    void Draw()
    {
        pixels = new Color[width * height];
        depth = new float[width * height];
        for (int i = 0; i < depth.Length; i++) depth[i] = float.MaxValue;

        float W = width, H = height;

        // 1. 하늘: 사각형 = 삼각형 2개, 위는 짙은 남색 → 아래는 옅은 안개색
        Color skyTop = new Color(0.10f, 0.12f, 0.22f), skyLow = new Color(0.55f, 0.52f, 0.58f);
        DrawQuad(new Vector2(0, 0), new Vector2(W, 0), new Vector2(W, H), new Vector2(0, H),
                 skyLow, skyLow, skyTop, skyTop, ZSky);

        // 2. 보름달: 원 위에 정점을 반복문으로 놓고 중심과 이어 삼각형 부채꼴로 채움
        DrawDisc(new Vector2(W * 0.68f, H * 0.76f), W * 0.15f, 40,
                 new Color(1f, 0.96f, 0.80f), new Color(0.98f, 0.82f, 0.48f), ZMoon);

        // 3. 먼 산 → 중간 산 → 가까운 절벽 (능선을 반복문으로 만들고, 능선 아래를 삼각형으로 채움)
        DrawRidge(H * 0.36f, H * 0.62f, 0.9f, 3.1f, new Color(0.40f, 0.44f, 0.55f), new Color(0.62f, 0.60f, 0.66f), ZFar);
        DrawRidge(H * 0.30f, H * 0.50f, 1.7f, 0.4f, new Color(0.20f, 0.23f, 0.30f), new Color(0.45f, 0.45f, 0.50f), ZMid);

        // 4. 강물: 아래쪽 띠 + 달빛이 비친 물결 (가는 삼각형들)
        Color waterTop = new Color(0.30f, 0.33f, 0.42f), waterLow = new Color(0.12f, 0.14f, 0.20f);
        DrawQuad(new Vector2(0, 0), new Vector2(W, 0), new Vector2(W, H * 0.32f), new Vector2(0, H * 0.32f),
                 waterLow, waterLow, waterTop, waterTop, ZWater);
        Color glint = new Color(1f, 0.9f, 0.62f);
        for (int k = 0; k < 6; k++)
        {
            float y = H * (0.29f - k * 0.035f);
            float half = W * (0.10f - k * 0.012f);
            float cx = W * 0.68f + Mathf.Sin(k * 1.7f) * 6f;
            DrawTriangle(new Vector2(cx - half, y), new Vector2(cx + half, y), new Vector2(cx, y + 3f),
                         glint, glint, glint, ZWater - 0.01f);
        }

        // 5. 가까운 절벽(왼쪽 아래)
        Color ink = new Color(0.06f, 0.06f, 0.08f), inkSoft = new Color(0.22f, 0.22f, 0.26f);
        DrawTriangle(new Vector2(0, H * 0.10f), new Vector2(W * 0.55f, H * 0.10f), new Vector2(W * 0.12f, H * 0.46f), inkSoft, inkSoft, ink, ZNear);
        DrawTriangle(new Vector2(0, H * 0.10f), new Vector2(W * 0.12f, H * 0.46f), new Vector2(0, H * 0.40f), inkSoft, ink, ink, ZNear);
        DrawTriangle(new Vector2(W * 0.10f, H * 0.10f), new Vector2(W * 0.45f, H * 0.10f), new Vector2(W * 0.30f, H * 0.30f), inkSoft, inkSoft, ink, ZNear);

        // 6. 절벽 위 소나무: 줄기 + 가지(층층이 납작한 삼각형)
        Color trunk = new Color(0.10f, 0.07f, 0.06f), pine = new Color(0.06f, 0.14f, 0.10f), pineLight = new Color(0.16f, 0.28f, 0.20f);
        Vector2 root = new Vector2(W * 0.14f, H * 0.44f);
        DrawTriangle(root + new Vector2(-3, 0), root + new Vector2(3, 0), root + new Vector2(18, 82), trunk, trunk, trunk, ZPine);
        for (int k = 0; k < 4; k++)
        {
            Vector2 c = root + new Vector2(6 + k * 3.5f, 30 + k * 16);
            float spread = 34 - k * 6;
            DrawTriangle(c + new Vector2(-spread, -6), c + new Vector2(spread + 8, -4), c + new Vector2(4, 10),
                         pine, pine, pineLight, ZPine - k * 0.01f);
        }

        // 7. 낙관(빨간 도장)
        Color seal = new Color(0.72f, 0.10f, 0.08f);
        Vector2 s0 = new Vector2(W * 0.82f, H * 0.09f);
        DrawQuad(s0, s0 + new Vector2(16, 0), s0 + new Vector2(16, 16), s0 + new Vector2(0, 16), seal, seal, seal, seal, ZSeal);

        // 8. 족자 테두리(비단): 위·아래는 넓게, 양옆은 좁게, 가장 앞
        Color silk = new Color(0.18f, 0.27f, 0.28f), silkLight = new Color(0.30f, 0.40f, 0.40f);
        float side = 12f, top = 42f, bottom = 30f;
        DrawQuad(new Vector2(0, H - top), new Vector2(W, H - top), new Vector2(W, H), new Vector2(0, H), silkLight, silkLight, silk, silk, ZFrame);
        DrawQuad(new Vector2(0, 0), new Vector2(W, 0), new Vector2(W, bottom), new Vector2(0, bottom), silk, silk, silkLight, silkLight, ZFrame);
        DrawQuad(new Vector2(0, 0), new Vector2(side, 0), new Vector2(side, H), new Vector2(0, H), silk, silk, silk, silk, ZFrame);
        DrawQuad(new Vector2(W - side, 0), new Vector2(W, 0), new Vector2(W, H), new Vector2(W - side, H), silk, silk, silk, silk, ZFrame);

        // 텍스처로 올려서 Quad 재질에 붙이기
        if (texture == null)
        {
            texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.DontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
        }
        else if (texture.width != width || texture.height != height)
        {
            texture.Reinitialize(width, height);
        }
        texture.SetPixels(pixels);
        texture.Apply();

        if (block == null) block = new MaterialPropertyBlock();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.GetPropertyBlock(block);
        block.SetTexture("_BaseMap", texture);
        block.SetTexture("_EmissionMap", texture);
        meshRenderer.SetPropertyBlock(block);
    }

    // ---------- 래스터라이저 ----------

    // 삼각형 하나를 픽셀로 채움: 경계 상자 → 무게중심 좌표 판정 → 색 보간 → 깊이 테스트
    void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, Color ca, Color cb, Color cc, float z)
    {
        float denom = a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y);
        if (Mathf.Abs(denom) < 1e-6f) return;   // 넓이가 0인 삼각형

        int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))));
        int maxX = Mathf.Min(width - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))));
        int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))));
        int maxY = Mathf.Min(height - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);   // 픽셀 중심

                float w1 = (p.x * (b.y - c.y) + b.x * (c.y - p.y) + c.x * (p.y - b.y)) / denom;
                float w2 = (a.x * (p.y - c.y) + p.x * (c.y - a.y) + c.x * (a.y - p.y)) / denom;
                float w3 = 1f - w1 - w2;
                if (w1 < 0f || w2 < 0f || w3 < 0f) continue;    // 삼각형 밖

                int index = y * width + x;
                if (z >= depth[index]) continue;                // 이미 더 앞에 있는 것이 그려짐
                depth[index] = z;
                pixels[index] = ca * w1 + cb * w2 + cc * w3;   // 꼭짓점 색 보간
            }
        }
    }

    // 사각형 = 삼각형 2개 (a: 왼쪽 아래, b: 오른쪽 아래, c: 오른쪽 위, d: 왼쪽 위)
    void DrawQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color ca, Color cb, Color cc, Color cd, float z)
    {
        DrawTriangle(a, b, c, ca, cb, cc, z);
        DrawTriangle(a, c, d, ca, cc, cd, z);
    }

    // 원 = 중심 정점 + 원 위의 정점 n개로 만든 삼각형 n개 (각도 = 360° / n × i)
    void DrawDisc(Vector2 center, float radius, int n, Color centerColor, Color edgeColor, float z)
    {
        for (int i = 0; i < n; i++)
        {
            float a0 = 2f * Mathf.PI / n * i;
            float a1 = 2f * Mathf.PI / n * (i + 1);
            Vector2 p0 = center + radius * new Vector2(Mathf.Cos(a0), Mathf.Sin(a0));
            Vector2 p1 = center + radius * new Vector2(Mathf.Cos(a1), Mathf.Sin(a1));
            DrawTriangle(center, p0, p1, centerColor, edgeColor, edgeColor, z);
        }
    }

    // 산 능선: x를 일정 간격으로 나누고 사인파를 섞어 봉우리 높이를 정한 뒤,
    // 능선의 이웃한 두 점과 바닥선을 이어 삼각형 2개씩으로 아래를 채움
    void DrawRidge(float baseY, float peakY, float frequency, float offset, Color ridgeColor, Color baseColor, float z)
    {
        const int steps = 48;
        float W = width;
        float amp = peakY - baseY;

        Vector2 prev = Vector2.zero;
        for (int i = 0; i <= steps; i++)
        {
            float x = W * i / steps;
            float t = (float)i / steps * Mathf.PI * 2f * frequency + offset;
            float h = 0.55f + 0.30f * Mathf.Sin(t) + 0.15f * Mathf.Sin(t * 2.7f + 1.1f);
            Vector2 cur = new Vector2(x, baseY + amp * Mathf.Clamp01(h));

            if (i > 0)
            {
                Vector2 bl = new Vector2(prev.x, 0f), br = new Vector2(cur.x, 0f);
                DrawTriangle(prev, cur, br, ridgeColor, ridgeColor, baseColor, z);
                DrawTriangle(prev, br, bl, ridgeColor, baseColor, baseColor, z);
            }
            prev = cur;
        }
    }
}
