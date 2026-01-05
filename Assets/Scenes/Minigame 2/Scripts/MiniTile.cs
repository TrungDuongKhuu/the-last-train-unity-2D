using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniTile
{
    public enum Direction
    {
        UP, DOWN, LEFT, RIGHT,
    }

    public enum PosNegType
    {
        POS,
        NEG,
        NONE,
    }

    // Padding viền ngoài của tile (do BoardGen thêm)
    public static int padding = 2;

    // Kích thước tile (vuông cũ) — giờ tách riêng thành width/height
    public static int tileSize = 32;
    public static int tileWidth = 32;
    public static int tileHeight = 32;

    // Bezier template
    private Dictionary<(Direction, PosNegType), LineRenderer> mLineRenderers
      = new Dictionary<(Direction, PosNegType), LineRenderer>();

    public static List<Vector2> BezCurve =
      BezierCurve.PointList2(TemplateBezierCurve.templateControlPoints, 0.001f);

    private Texture2D mOriginalTexture;
    public Texture2D finalCut { get; private set; }

    public static readonly Color TransparentColor = new Color(0f, 0f, 0f, 0f);

    private PosNegType[] mCurveTypes = new PosNegType[4]
    {
        PosNegType.NONE,
        PosNegType.NONE,
        PosNegType.NONE,
        PosNegType.NONE,
    };

    private bool[,] mVisited;
    private Stack<Vector2Int> mStack = new Stack<Vector2Int>();

    public int xIndex = 0;
    public int yIndex = 0;

    public static TilesSorting tilesSorting = new TilesSorting();

    public void SetCurveType(Direction dir, PosNegType type)
    {
        mCurveTypes[(int)dir] = type;
    }

    public PosNegType GetCurveType(Direction dir)
    {
        return mCurveTypes[(int)dir];
    }

    public MiniTile(Texture2D texture)
    {
        mOriginalTexture = texture;

        int tileW = 2 * padding + tileWidth;
        int tileH = 2 * padding + tileHeight;

        finalCut = new Texture2D(tileW, tileH, TextureFormat.ARGB32, false);

        // Khởi tạo toàn bộ transparent
        for (int i = 0; i < tileW; ++i)
            for (int j = 0; j < tileH; ++j)
                finalCut.SetPixel(i, j, TransparentColor);
    }

    public void Apply()
    {
        FloodFillInit();
        FloodFill();
        finalCut.Apply();
    }

    // ⚙️ Khởi tạo mảng visited + biên
    void FloodFillInit()
    {
        int tileW = 2 * padding + tileWidth;
        int tileH = 2 * padding + tileHeight;

        mVisited = new bool[tileW, tileH];
        for (int i = 0; i < tileW; ++i)
            for (int j = 0; j < tileH; ++j)
                mVisited[i, j] = false;

        List<Vector2> pts = new List<Vector2>();
        for (int i = 0; i < mCurveTypes.Length; ++i)
            pts.AddRange(CreateCurve((Direction)i, mCurveTypes[i]));

        // Đảm bảo không truy cập ra ngoài
        foreach (Vector2 p in pts)
        {
            int px = Mathf.Clamp((int)p.x, 0, tileW - 1);
            int py = Mathf.Clamp((int)p.y, 0, tileH - 1);
            mVisited[px, py] = true;
        }

        // Bắt đầu từ tâm
        Vector2Int start = new Vector2Int(tileW / 2, tileH / 2);
        mVisited[start.x, start.y] = true;
        mStack.Push(start);
    }

    void Fill(int x, int y)
    {
        // Tính vùng gốc trong ảnh để cắt chuẩn pixel
        int pieceWidth = mOriginalTexture.width / BoardGen.Instance.manualTileX;
        int pieceHeight = mOriginalTexture.height / BoardGen.Instance.manualTileY;

        int srcX = xIndex * pieceWidth + x;
        int srcY = yIndex * pieceHeight + y;

        // Giới hạn an toàn
        if (srcX >= mOriginalTexture.width) srcX = mOriginalTexture.width - 1;
        if (srcY >= mOriginalTexture.height) srcY = mOriginalTexture.height - 1;

        Color c = mOriginalTexture.GetPixel(srcX, srcY);
        finalCut.SetPixel(x, y, c);
    }




    void FloodFill()
    {
        int tileW = 2 * padding + tileWidth;
        int tileH = 2 * padding + tileHeight;

        while (mStack.Count > 0)
        {
            Vector2Int v = mStack.Pop();
            int xx = v.x;
            int yy = v.y;

            Fill(xx, yy);

            // 4 hướng
            TryPush(xx + 1, yy, tileW, tileH);
            TryPush(xx - 1, yy, tileW, tileH);
            TryPush(xx, yy + 1, tileW, tileH);
            TryPush(xx, yy - 1, tileW, tileH);
        }
    }

    void TryPush(int x, int y, int width, int height)
    {
        if (x < 0 || y < 0 || x >= width || y >= height)
            return;

        if (!mVisited[x, y])
        {
            mVisited[x, y] = true;
            mStack.Push(new Vector2Int(x, y));
        }
    }

    // 🧩 Bézier curve cho cạnh
    public List<Vector2> CreateCurve(Direction dir, PosNegType type)
    {
        int padding_x = padding;
        int padding_y = padding;
        int sw = tileWidth;
        int sh = tileHeight;

        List<Vector2> pts = new List<Vector2>(BezCurve);
        switch (dir)
        {
            case Direction.UP:
                if (type == PosNegType.POS)
                    TranslatePoints(pts, new Vector2(padding_x, padding_y + sh));
                else if (type == PosNegType.NEG)
                {
                    InvertY(pts);
                    TranslatePoints(pts, new Vector2(padding_x, padding_y + sh));
                }
                else
                {
                    pts.Clear();
                    for (int i = 0; i < sw; ++i)
                        pts.Add(new Vector2(i + padding_x, padding_y + sh));
                }
                break;

            case Direction.RIGHT:
                if (type == PosNegType.POS)
                {
                    SwapXY(pts);
                    TranslatePoints(pts, new Vector2(padding_x + sw, padding_y));
                }
                else if (type == PosNegType.NEG)
                {
                    InvertY(pts);
                    SwapXY(pts);
                    TranslatePoints(pts, new Vector2(padding_x + sw, padding_y));
                }
                else
                {
                    pts.Clear();
                    for (int i = 0; i < sh; ++i)
                        pts.Add(new Vector2(padding_x + sw, i + padding_y));
                }
                break;

            case Direction.DOWN:
                if (type == PosNegType.POS)
                {
                    InvertY(pts);
                    TranslatePoints(pts, new Vector2(padding_x, padding_y));
                }
                else if (type == PosNegType.NEG)
                    TranslatePoints(pts, new Vector2(padding_x, padding_y));
                else
                {
                    pts.Clear();
                    for (int i = 0; i < sw; ++i)
                        pts.Add(new Vector2(i + padding_x, padding_y));
                }
                break;

            case Direction.LEFT:
                if (type == PosNegType.POS)
                {
                    InvertY(pts);
                    SwapXY(pts);
                    TranslatePoints(pts, new Vector2(padding_x, padding_y));
                }
                else if (type == PosNegType.NEG)
                {
                    SwapXY(pts);
                    TranslatePoints(pts, new Vector2(padding_x, padding_y));
                }
                else
                {
                    pts.Clear();
                    for (int i = 0; i < sh; ++i)
                        pts.Add(new Vector2(padding_x, i + padding_y));
                }
                break;
        }
        return pts;
    }

    // ─── Helper: Transform Points ────────────────────────────────
    public static void TranslatePoints(List<Vector2> iList, Vector2 offset)
    {
        for (int i = 0; i < iList.Count; i++)
            iList[i] += offset;
    }

    public static void InvertY(List<Vector2> iList)
    {
        for (int i = 0; i < iList.Count; i++)
            iList[i] = new Vector2(iList[i].x, -iList[i].y);
    }

    public static void SwapXY(List<Vector2> iList)
    {
        for (int i = 0; i < iList.Count; ++i)
            iList[i] = new Vector2(iList[i].y, iList[i].x);
    }

    // ─── LineRenderer helpers ────────────────────────────────────
    public static LineRenderer CreateLineRenderer(Color color, float lineWidth = 1.0f)
    {
        GameObject obj = new GameObject();
        LineRenderer lr = obj.AddComponent<LineRenderer>();

        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        return lr;
    }

    public void DrawCurve(Direction dir, PosNegType type, Color color)
    {
        if (!mLineRenderers.ContainsKey((dir, type)))
            mLineRenderers.Add((dir, type), CreateLineRenderer(color));

        LineRenderer lr = mLineRenderers[(dir, type)];
        lr.gameObject.SetActive(true);
        lr.startColor = color;
        lr.endColor = color;
        lr.gameObject.name = "LineRenderer_" + dir.ToString() + "_" + type.ToString();

        List<Vector2> pts = CreateCurve(dir, type);
        lr.positionCount = pts.Count;
        for (int i = 0; i < pts.Count; ++i)
            lr.SetPosition(i, pts[i]);
    }

    public void HideAllCurves()
    {
        foreach (var item in mLineRenderers)
            item.Value.gameObject.SetActive(false);
    }

    public void DestroyAllCurves()
    {
        foreach (var item in mLineRenderers)
            GameObject.Destroy(item.Value.gameObject);

        mLineRenderers.Clear();
    }
}
