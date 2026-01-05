using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGen : MonoBehaviour
{
    private string imageFilename;
    Sprite mBaseSpriteOpaque;
    Sprite mBaseSpriteTransparent;

    GameObject mGameObjectOpaque;
    GameObject mGameObjectTransparent;

    public float ghostTransparency = 0.1f;

    public int numTileX { get; private set; }
    public int numTileY { get; private set; }

    MiniTile[,] mTiles = null;
    GameObject[,] mTileGameObjects = null;

    public Transform parentForTiles = null;

    public MenuPuzzle2 menu = null;

    private List<Rect> regions = new List<Rect>();
    private List<Coroutine> activeCoroutines = new List<Coroutine>();

    private Coroutine shuffleCoroutine;
    private Coroutine timerCoroutine;

    public static BoardGen Instance { get; private set; }
    private void Awake() => Instance = this;

    [Header("Custom Image Split Settings")]
    public int manualTileX = 4;
    public int manualTileY = 4;

    Sprite LoadBaseTexture()
    {
        Texture2D tex = SpriteUtils.LoadTexture(imageFilename);

        if (tex == null)
        {
            Debug.LogError("❌ Texture not found for file: " + imageFilename);
            return null;
        }

        if (!tex.isReadable)
        {
            Debug.LogError("❌ Texture is not readable. Check import settings (Read/Write Enabled).");
            return null;
        }

        Texture2D cleanTex = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
        cleanTex.SetPixels(tex.GetPixels());
        cleanTex.Apply();

        return Sprite.Create(
            cleanTex,
            new Rect(0, 0, cleanTex.width, cleanTex.height),
            new Vector2(0.5f, 0.5f),
            1f
        );
    }

    void Start()
    {
        imageFilename = GameApp.Instance.GetJigsawImageName();

        mBaseSpriteOpaque = LoadBaseTexture();
        if (mBaseSpriteOpaque == null)
            return;

        mGameObjectOpaque = new GameObject();
        mGameObjectOpaque.name = imageFilename + "_Opaque";
        var sr = mGameObjectOpaque.AddComponent<SpriteRenderer>();
        sr.sprite = mBaseSpriteOpaque;
        sr.sortingLayerName = "Decor";   
        sr.sortingOrder = 999;          

        mGameObjectOpaque.transform.position = new Vector3(
            mGameObjectOpaque.transform.position.x,
            mGameObjectOpaque.transform.position.y,
            -2f 
        );

        mGameObjectOpaque.gameObject.SetActive(false);

        mBaseSpriteTransparent = CreateTransparentView(mBaseSpriteOpaque.texture);

        SetCameraPosition();

        shuffleCoroutine = StartCoroutine(Coroutine_CreateJigsawTiles());
    }

    Sprite CreateTransparentView(Texture2D tex)
    {
        Texture2D newTex = new Texture2D(
          tex.width,
          tex.height,
          TextureFormat.ARGB32,
          false);

        for (int x = 0; x < newTex.width; x++)
        {
            for (int y = 0; y < newTex.height; y++)
            {
                Color c = tex.GetPixel(x, y);
                if (x > MiniTile.padding &&
                   x < (newTex.width - MiniTile.padding) &&
                   y > MiniTile.padding &&
                   y < (newTex.height - MiniTile.padding))
                {
                    c.a = ghostTransparency;
                }
                newTex.SetPixel(x, y, c);
            }
        }

        newTex.Apply();

        Sprite sprite = SpriteUtils.CreateSpriteFromTexture2D(
          newTex,
          0,
          0,
          newTex.width,
          newTex.height);
        return sprite;
    }

    void SetCameraPosition()
    {
        if (mBaseSpriteOpaque == null || mBaseSpriteOpaque.texture == null) return;

        Camera.main.transform.position = new Vector3(
            mBaseSpriteOpaque.texture.width / 2,
            mBaseSpriteOpaque.texture.height / 2,
            -10.0f);

        int smaller_value = Mathf.Min(mBaseSpriteOpaque.texture.width, mBaseSpriteOpaque.texture.height);
        Camera.main.orthographicSize = smaller_value * 0.8f;
    }

    public static GameObject CreateGameObjectFromTile(MiniTile tile)
    {
        GameObject obj = new GameObject();
        obj.name = "TileGameObj_" + tile.xIndex.ToString() + "_" + tile.yIndex.ToString();

        obj.transform.position = new Vector3(tile.xIndex * MiniTile.tileSize, tile.yIndex * MiniTile.tileSize, 0.0f);

        SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = SpriteUtils.CreateSpriteFromTexture2D(
          tile.finalCut,
          0,
          0,
          MiniTile.padding * 2 + MiniTile.tileSize,
          MiniTile.padding * 2 + MiniTile.tileSize);

        obj.AddComponent<BoxCollider2D>();

        TileMovement tileMovement = obj.AddComponent<TileMovement>();
        tileMovement.tile = tile;

        return obj;
    }

    IEnumerator Coroutine_CreateJigsawTiles()
    {
        Texture2D baseTexture = mBaseSpriteOpaque.texture;
        if (baseTexture == null)
        {
            Debug.LogError("❌ Base texture is missing.");
            yield break;
        }

        int innerWidth = baseTexture.width - MiniTile.padding * 2;
        int innerHeight = baseTexture.height - MiniTile.padding * 2;

        manualTileX = Mathf.Max(1, manualTileX);
        manualTileY = Mathf.Max(1, manualTileY);

        int sizeX = innerWidth / manualTileX;
        int sizeY = innerHeight / manualTileY;

        MiniTile.tileSize = Mathf.Max(1, Mathf.Min(sizeX, sizeY));

        numTileX = manualTileX;
        numTileY = manualTileY;

        mTiles = new MiniTile[numTileX, numTileY];
        mTileGameObjects = new GameObject[numTileX, numTileY];

        for (int i = 0; i < numTileX; i++)
        {
            for (int j = 0; j < numTileY; j++)
            {
                mTiles[i, j] = CreateTile(i, j, baseTexture);
                mTileGameObjects[i, j] = CreateGameObjectFromTile(mTiles[i, j]);
                if (parentForTiles != null)
                    mTileGameObjects[i, j].transform.SetParent(parentForTiles);

                mTileGameObjects[i, j].SetActive(false);
                yield return null;
            }
        }

        menu.SetEnableBottomPanel(true);
        menu.btnPlayOnClick = ShuffleTiles;
    }

    MiniTile CreateTile(int i, int j, Texture2D baseTexture)
    {
        MiniTile tile = new MiniTile(baseTexture);
        tile.xIndex = i;
        tile.yIndex = j;

        // Matching edges logic giữ nguyên...
        if (i == 0)
            tile.SetCurveType(MiniTile.Direction.LEFT, MiniTile.PosNegType.NONE);
        else
        {
            MiniTile leftTile = mTiles[i - 1, j];
            MiniTile.PosNegType rightOp = leftTile.GetCurveType(MiniTile.Direction.RIGHT);
            tile.SetCurveType(MiniTile.Direction.LEFT, rightOp == MiniTile.PosNegType.NEG ?
                MiniTile.PosNegType.POS : MiniTile.PosNegType.NEG);
        }

        if (j == 0)
            tile.SetCurveType(MiniTile.Direction.DOWN, MiniTile.PosNegType.NONE);
        else
        {
            MiniTile downTile = mTiles[i, j - 1];
            MiniTile.PosNegType upOp = downTile.GetCurveType(MiniTile.Direction.UP);
            tile.SetCurveType(MiniTile.Direction.DOWN, upOp == MiniTile.PosNegType.NEG ?
                MiniTile.PosNegType.POS : MiniTile.PosNegType.NEG);
        }

        if (i == numTileX - 1)
            tile.SetCurveType(MiniTile.Direction.RIGHT, MiniTile.PosNegType.NONE);
        else
            tile.SetCurveType(MiniTile.Direction.RIGHT,
                UnityEngine.Random.value < 0.5f ? MiniTile.PosNegType.POS : MiniTile.PosNegType.NEG);

        if (j == numTileY - 1)
            tile.SetCurveType(MiniTile.Direction.UP, MiniTile.PosNegType.NONE);
        else
            tile.SetCurveType(MiniTile.Direction.UP,
                UnityEngine.Random.value < 0.5f ? MiniTile.PosNegType.POS : MiniTile.PosNegType.NEG);

        tile.Apply();
        return tile;
    }

    private IEnumerator Coroutine_MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0.0f;
        Vector3 startingPosition = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(startingPosition, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.position = end;
    }

    void Shuffle(GameObject obj)
    {
        Camera cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float camLeft = cam.transform.position.x - halfW;
        float camRight = cam.transform.position.x + halfW;
        float camBottom = cam.transform.position.y - halfH;
        float camTop = cam.transform.position.y + halfH;

        Rect puzzleRect = new Rect(
            0f,
            0f,
            numTileX * MiniTile.tileSize,
            numTileY * MiniTile.tileSize
        );

        float margin = MiniTile.tileSize * 2f;

        Vector3 pos;
        int safety = 0;

        do
        {
            float x = UnityEngine.Random.Range(camLeft + margin, camRight - margin);
            float y = UnityEngine.Random.Range(camBottom + margin, camTop - margin);
            pos = new Vector3(x, y, 0f);
            safety++;
        }
        while (puzzleRect.Contains(new Vector2(pos.x, pos.y)) && safety < 50);

        Coroutine c = StartCoroutine(Coroutine_MoveOverSeconds(obj, pos, 1.0f));
        activeCoroutines.Add(c);
    }

    IEnumerator Coroutine_Shuffle()
    {
        activeCoroutines.Clear();

        for (int i = 0; i < numTileX; ++i)
        {
            for (int j = 0; j < numTileY; ++j)
            {
                GameObject obj = mTileGameObjects[i, j];
                obj.SetActive(true);
                Shuffle(obj);
                yield return null;
            }
        }

        // Chờ tất cả coroutines move hoàn thành
        yield return new WaitForSeconds(1f);

        OnFinishedShuffling();
    }

    public void ShuffleTiles()
    {
        if (shuffleCoroutine != null)
            StopCoroutine(shuffleCoroutine);

        shuffleCoroutine = StartCoroutine(Coroutine_Shuffle());
    }

    void OnFinishedShuffling()
    {
        menu.SetEnableBottomPanel(false);
        StartCoroutine(Coroutine_CallAfterDelay(() => menu.SetEnableTopPanel(true), 1.0f));

        GameApp.Instance.TileMovementEnabled = true;
        StartTimer();

        for (int i = 0; i < numTileX; ++i)
            for (int j = 0; j < numTileY; ++j)
            {
                TileMovement tm = mTileGameObjects[i, j].GetComponent<TileMovement>();
                tm.onTileInPlace += OnTileInPlace;

                SpriteRenderer sr = tm.gameObject.GetComponent<SpriteRenderer>();
                MiniTile.tilesSorting.BringToTop(sr);
            }

        menu.SetTotalTiles(numTileX * numTileY);
    }

    IEnumerator Coroutine_CallAfterDelay(Action function, float delay)
    {
        yield return new WaitForSeconds(delay);
        function();
    }

    public void StartTimer()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timerCoroutine = StartCoroutine(Coroutine_Timer());
    }

    IEnumerator Coroutine_Timer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            GameApp.Instance.SecondsSinceStart += 1;
            menu.SetTimeInSeconds(GameApp.Instance.SecondsSinceStart);
        }
    }

    public void StopTimer()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
    }

    void OnWin()
    {
        // Dừng timer
        StopTimer();

        // Dừng shuffle
        if (shuffleCoroutine != null)
            StopCoroutine(shuffleCoroutine);

        StopAllCoroutines(); // Ngăn rác coroutine chạy tiếp

        menu.SetEnableTopPanel(false);
        menu.SetEnableGameCompletionPanel(true);

        // Reset state
        GameApp.Instance.SecondsSinceStart = 0;
        GameApp.Instance.TotalTilesInCorrectPosition = 0;

        // Count puzzle 2
        GameApp.Instance.OnPuzzle2Finished();

        // Update UI sau khi tăng count
        menu.RefreshAllUI();
    }

    void OnTileInPlace(TileMovement tm)
    {
        GameApp.Instance.TotalTilesInCorrectPosition += 1;

        tm.enabled = false;
        Destroy(tm);

        SpriteRenderer sr = tm.gameObject.GetComponent<SpriteRenderer>();
        MiniTile.tilesSorting.Remove(sr);

        if (GameApp.Instance.TotalTilesInCorrectPosition == mTileGameObjects.Length)
        {
            OnWin();
        }

        menu.SetTilesInPlace(GameApp.Instance.TotalTilesInCorrectPosition);
    }

    public void ShowOpaqueImage()
    {
        if (mGameObjectOpaque != null)
        {
            mGameObjectOpaque.SetActive(true);
        }
    }

    public void HideOpaqueImage()
    {
        if (mGameObjectOpaque != null)
        {
            mGameObjectOpaque.SetActive(false);
        }
    }
}
