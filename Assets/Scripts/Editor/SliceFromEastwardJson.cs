//#if UNITY_EDITOR
//using UnityEngine;
//using UnityEditor;
//using System.IO;
//using Newtonsoft.Json.Linq;
//using System.Collections.Generic;

//public class SliceFromEastwardJson : EditorWindow
//{
//    static List<string> smallSpriteFiles = new List<string>();

//    [MenuItem("Tools/Eastward/Slice One (manual) (Step 1)")]
//    public static void SliceOneManual()
//    {
//        string imagePath = EditorUtility.OpenFilePanel("Chọn file atlas (.png)", "", "png");
//        if (string.IsNullOrEmpty(imagePath)) return;

//        string jsonPath = EditorUtility.OpenFilePanel("Chọn file JSON tương ứng (.json)", "", "json");
//        if (string.IsNullOrEmpty(jsonPath)) return;

//        SliceAtlas(imagePath, jsonPath, null);
//    }

//    // ==========================================================
//    // 🔍 TEST: Dựng toàn bộ object từ JSON lên Scene (bỏ qua mọi lọc)
//    // ==========================================================
//    [MenuItem("Tools/Eastward/Test - Build ALL Objects (No Filter)")]
//    public static void TestBuildAllObjects()
//    {
//        string imagePath = EditorUtility.OpenFilePanel("Chọn file atlas (.png)", "", "png");
//        if (string.IsNullOrEmpty(imagePath)) return;

//        string jsonPath = EditorUtility.OpenFilePanel("Chọn file JSON tương ứng (.json)", "", "json");
//        if (string.IsNullOrEmpty(jsonPath)) return;

//        string jsonText = File.ReadAllText(jsonPath);
//        JObject root = JObject.Parse(jsonText);

//        int atlasW = root["atlas"]?["w"]?.Value<int>() ?? 0;
//        int atlasH = root["atlas"]?["h"]?.Value<int>() ?? 0;

//        Texture2D tex = new Texture2D(2, 2);
//        tex.LoadImage(File.ReadAllBytes(imagePath));
//        if (atlasW <= 0) atlasW = tex.width;
//        if (atlasH <= 0) atlasH = tex.height;

//        var decks = root["decks"] as JArray;
//        if (decks == null)
//        {
//            Debug.LogError("❌ JSON không có 'decks'.");
//            return;
//        }

//        GameObject rootGO = new GameObject("Eastward_Test_FlatLayout");

//        // ---- Thu thập toàn bộ tile có meshes ----
//        List<(string name, JArray meshes)> allTiles = new List<(string, JArray)>();
//        foreach (var deck in decks)
//        {
//            // deck meshes
//            var meshes = deck["meshes"] as JArray;
//            if (meshes != null && meshes.Count > 0)
//                allTiles.Add((deck["name"]?.ToString() ?? "deck", meshes));

//            // groups → tiles → meshes
//            var groups = deck["groups"] as JArray;
//            if (groups != null)
//            {
//                foreach (var group in groups)
//                {
//                    var tiles = group["tiles"] as JArray;
//                    if (tiles == null) continue;
//                    foreach (var tile in tiles)
//                    {
//                        var tmeshes = tile["meshes"] as JArray;
//                        if (tmeshes == null || tmeshes.Count == 0) continue;
//                        string tname = tile["name"]?.ToString() ?? "tile";
//                        allTiles.Add((tname, tmeshes));
//                    }
//                }
//            }
//        }

//        // ---- Layout tự động ----
//        int total = allTiles.Count;
//        if (total == 0)
//        {
//            Debug.LogWarning("⚠️ Không tìm thấy tile nào để dựng.");
//            return;
//        }

//        int cols = Mathf.CeilToInt(Mathf.Sqrt(total));
//        float spacingMultiplier = 5f;
//        float spacingX = Mathf.Max(atlasW / 16f, 5f) * spacingMultiplier;
//        float spacingY = Mathf.Max(atlasH / 16f, 5f) * spacingMultiplier;

//        for (int i = 0; i < total; i++)
//        {
//            var (name, meshes) = allTiles[i];
//            int row = i / cols;
//            int col = i % cols;

//            GameObject obj = new GameObject(name);
//            obj.transform.SetParent(rootGO.transform);
//            obj.transform.position = new Vector3(col * spacingX, row * -spacingY, 0);

//            BuildAllMeshes_NoFilter(meshes, tex, atlasW, atlasH, obj);
//        }

//        Debug.Log($"✅ Dựng {total} object theo layout phẳng ({cols} cột, spacing {spacingX:F1}x{spacingY:F1}).");

//#if UNITY_EDITOR
//        if (SceneView.lastActiveSceneView != null)
//        {
//            SceneView.lastActiveSceneView.FrameSelected();
//            SceneView.lastActiveSceneView.Repaint();
//        }
//#endif
//    }

//    [MenuItem("Tools/Eastward/Slice Folders (manual) (Step 1)")]
//    public static void SliceFoldersManual()
//    {
//        string imageFolder = EditorUtility.OpenFolderPanel("Chọn folder ảnh (.png)", "", "");
//        if (string.IsNullOrEmpty(imageFolder)) return;

//        string jsonFolder = EditorUtility.OpenFolderPanel("Chọn folder JSON", "", "");
//        if (string.IsNullOrEmpty(jsonFolder)) return;

//        string outFolder = EditorUtility.OpenFolderPanel("Chọn folder xuất", Application.dataPath, "");
//        if (string.IsNullOrEmpty(outFolder))
//            outFolder = Path.Combine(Application.dataPath, "Extracted");

//        string[] pngFiles = Directory.GetFiles(imageFolder, "*.png");
//        int total = 0;

//        foreach (string pngPath in pngFiles)
//        {
//            string prefix = Path.GetFileNameWithoutExtension(pngPath).Split('_')[0];
//            string[] jsonCandidates = Directory.GetFiles(jsonFolder, prefix + "*.json");
//            if (jsonCandidates.Length == 0)
//            {
//                Debug.LogWarning($"⚠️ Không tìm thấy JSON cho {Path.GetFileName(pngPath)}");
//                continue;
//            }

//            total += SliceAtlas(pngPath, jsonCandidates[0], outFolder);
//        }

//        if (smallSpriteFiles.Count > 0)
//        {
//            Debug.Log("📋 Danh sách file có dưới 20 sprite:");
//            for (int i = 0; i < smallSpriteFiles.Count; i++)
//                Debug.Log($"{i + 1}. {smallSpriteFiles[i]}");
//        }

//        AssetDatabase.Refresh();
//        EditorUtility.DisplayDialog("Hoàn tất", $"Đã cắt {total} sprite.", "OK");
//    }

//    // ==============================================================
//    // CHÍNH: Hàm đọc JSON, cắt sprite và dựng 2.5D nếu cần
//    // ==============================================================
//    private static int SliceAtlas(string pngPath, string jsonPath, string forcedOutFolder)
//    {
//        // Đọc JSON
//        string jsonText = File.ReadAllText(jsonPath);
//        JObject root = JObject.Parse(jsonText);

//        // Lấy kích thước atlas
//        int atlasW = root["atlas"]?["w"]?.Value<int>() ?? 0;
//        int atlasH = root["atlas"]?["h"]?.Value<int>() ?? 0;

//        // Tải ảnh PNG
//        Texture2D tex = new Texture2D(2, 2);
//        tex.LoadImage(File.ReadAllBytes(pngPath));
//        if (atlasW <= 0) atlasW = tex.width;
//        if (atlasH <= 0) atlasH = tex.height;

//        // Thư mục xuất ảnh
//        string outFolder = string.IsNullOrEmpty(forcedOutFolder)
//            ? Path.Combine(Application.dataPath, "Extracted")
//            : forcedOutFolder;

//        Directory.CreateDirectory(outFolder);
//        int index = 0;

//        // Lấy danh sách "decks" từ JSON
//        var decks = root["decks"];
//        if (decks == null)
//        {
//            Debug.LogError("❌ JSON không có 'decks'.");
//            return 0;
//        }

//        // Duyệt từng deck
//        foreach (var deck in decks)
//        {
//            string deckName = deck["name"]?.ToString() ?? $"deck_{index}";

//            // --- Nếu deck có meshes riêng ---
//            if (deck["meshes"] != null)
//            {
//                var meshes = deck["meshes"] as JArray;
//                bool build25D = ShouldBuild25D(deck, atlasW, atlasH);

//                if (build25D)
//                    BuildMeshFromJson(meshes, tex, atlasW, atlasH, deckName);
//                else 
//                    index += ExtractMeshes(meshes, tex, atlasW, atlasH, pngPath, outFolder, deckName, index);
//            }

//            // --- Nếu deck có groups (nhóm các tile) ---
//            if (deck["groups"] != null)
//            {
//                foreach (var group in deck["groups"])
//                {
//                    var tiles = group["tiles"];
//                    if (tiles == null) continue;

//                    foreach (var tile in tiles)
//                    {
//                        string tileName = tile["name"]?.ToString() ?? "tile";
//                        var tmeshes = tile["meshes"] as JArray;
//                        if (tmeshes == null) continue;

//                        bool build25D = ShouldBuild25D(tile, atlasW, atlasH);

//                        if (build25D)
//                            BuildMeshFromJson(tmeshes, tex, atlasW, atlasH, $"{deckName}.{tileName}");
//                        else
//                            index += ExtractMeshes(tmeshes, tex, atlasW, atlasH, pngPath, outFolder, $"{deckName}.{tileName}", index);
//                    }
//                }
//            }
//        }

//        // Ghi log kết quả
//        Debug.Log($"✅ Cắt xong {Path.GetFileName(pngPath)} ({index} sprite) → {outFolder}");

//        // Nếu có ít sprite, ghi vào danh sách cảnh báo
//        if (index < 20)
//        {
//            smallSpriteFiles.Add($"{Path.GetFileName(pngPath)} ({index} sprite)");
//            Debug.LogWarning($"⚠️ File có ít sprite (<20): {Path.GetFileName(pngPath)} ({index} sprite)");
//        }

//        return index;
//    }

//    private static void BuildAllMeshes_NoFilter(JArray meshes, Texture2D tex, int atlasW, int atlasH, GameObject parent, bool flattenZ = false)
//    {
//        foreach (var mesh in meshes)
//        {
//            var verts = mesh["verts"] as JArray;
//            var uvs = mesh["uv"] as JArray;
//            if (verts == null || uvs == null || verts.Count != 4) continue;

//            Vector3[] vertices = new Vector3[4];
//            Vector2[] uv = new Vector2[4];
//            int[] triangles = { 0, 1, 2, 2, 3, 0 };

//            for (int i = 0; i < 4; i++)
//            {
//                float x = verts[i][0].Value<float>();
//                float y = verts[i][1].Value<float>();
//                float z = flattenZ ? 0f : verts[i][2].Value<float>();
//                vertices[i] = new Vector3(x, y, z);

//                float u = uvs[i][0].Value<float>();
//                float v = 1f - uvs[i][1].Value<float>();
//                uv[i] = new Vector2(u, v);
//            }

//            Mesh unityMesh = new Mesh();
//            unityMesh.vertices = vertices;
//            unityMesh.uv = uv;
//            unityMesh.triangles = triangles;
//            unityMesh.RecalculateNormals();

//            GameObject quad = new GameObject("mesh");
//            quad.transform.SetParent(parent.transform, false);
//            var mf = quad.AddComponent<MeshFilter>();
//            var mr = quad.AddComponent<MeshRenderer>();
//            mr.material = new Material(Shader.Find("Unlit/Transparent"))
//            {
//                mainTexture = tex
//            };
//            mf.mesh = unityMesh;
//        }
//    }
//    private static void BuildMeshFromJson(JArray meshes, Texture2D tex, int atlasW, int atlasH, string deckName)
//    {
//        GameObject parent = new GameObject(deckName);

//        foreach (var mesh in meshes)
//        {
//            var verts = mesh["verts"] as JArray;
//            var uvs = mesh["uv"] as JArray;
//            if (verts == null || uvs == null || verts.Count != 4 || uvs.Count != 4)
//                continue;

//            float uMin = float.MaxValue, uMax = float.MinValue, vMin = float.MaxValue, vMax = float.MinValue;
//            for (int i = 0; i < 4; i++)
//            {
//                float u = uvs[i][0].Value<float>();
//                float v = uvs[i][1].Value<float>();
//                uMin = Mathf.Min(uMin, u);
//                uMax = Mathf.Max(uMax, u);
//                vMin = Mathf.Min(vMin, v);
//                vMax = Mathf.Max(vMax, v);
//            }

//            float pixelWidth = (uMax - uMin) * atlasW;
//            float pixelHeight = (vMax - vMin) * atlasH;

//            float meshWidth = verts[1][0].Value<float>() - verts[0][0].Value<float>();
//            float meshHeight = verts[2][1].Value<float>() - verts[0][1].Value<float>();

//            float scaleX = pixelWidth > 0 ? (1f / meshWidth) * pixelWidth : 1f;
//            float scaleY = pixelHeight > 0 ? (1f / meshHeight) * pixelHeight : 1f;
//            float scale = Mathf.Min(scaleX, scaleY);

//            Vector3[] vertices = new Vector3[4];
//            Vector2[] uv = new Vector2[4];
//            int[] triangles = new int[] { 0, 1, 2, 2, 3, 0 };

//            for (int i = 0; i < 4; i++)
//            {
//                float x = verts[i][0].Value<float>() * scale;
//                float y = verts[i][1].Value<float>() * scale;
//                float z = verts[i][2].Value<float>() * scale;
//                vertices[i] = new Vector3(x, y, z);

//                float u = uvs[i][0].Value<float>();
//                float v = 1f - uvs[i][1].Value<float>();
//                uv[i] = new Vector2(u, v);
//            }

//            Mesh unityMesh = new Mesh();
//            unityMesh.vertices = vertices;
//            unityMesh.uv = uv;
//            unityMesh.triangles = triangles;
//            unityMesh.RecalculateNormals();

//            GameObject quad = new GameObject("mesh");
//            var mf = quad.AddComponent<MeshFilter>();
//            var mr = quad.AddComponent<MeshRenderer>();
//            mr.material = new Material(Shader.Find("Unlit/Transparent"))
//            {
//                mainTexture = tex
//            };
//            mf.mesh = unityMesh;
//            quad.transform.SetParent(parent.transform, false);
//        }
//    }

//    private static int ExtractMeshes(JArray meshes, Texture2D tex, int atlasW, int atlasH,
//                                 string pngPath, string outFolder, string baseName, int startIndex)
//    {
//        if (meshes == null) return 0;
//        int count = 0;

//        List<Rect> mergedRects = MergeCloseMeshes(meshes, atlasW, atlasH);

//        // 🧩 Tính tổng diện tích
//        float totalArea = 0;
//        foreach (Rect r in mergedRects) totalArea += r.width * r.height;

//        // ✅ Gộp nếu là vật nhỏ hoặc có ≤ 2 mesh (phẳng)
//        if ((mergedRects.Count <= 2 && !HasDepthDifference(meshes, 4f)) ||
//            (totalArea < 1024f && mergedRects.Count > 1))
//        {
//            Rect big = mergedRects[0];
//            for (int i = 1; i < mergedRects.Count; i++)
//                big = Union(big, mergedRects[i]);
//            mergedRects.Clear();
//            mergedRects.Add(big);
//        }

//        foreach (var r in mergedRects)
//        {
//            int x = Mathf.RoundToInt(r.x);
//            int y = Mathf.RoundToInt(r.y);
//            int w = Mathf.RoundToInt(r.width);
//            int h = Mathf.RoundToInt(r.height);

//            if (w <= 1 || h <= 1) continue;
//            if (x < 0) { w += x; x = 0; }
//            if (y < 0) { h += y; y = 0; }
//            if (x + w > tex.width) w = tex.width - x;
//            if (y + h > tex.height) h = tex.height - y;

//            // ❌ Bỏ qua sprite chiếm gần hết atlas
//            if (w > atlasW * 0.9f && h > atlasH * 0.9f)
//                continue;

//            try
//            {
//                Color[] pixels = tex.GetPixels(x, y, w, h);
//                Texture2D newTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
//                newTex.SetPixels(pixels);
//                newTex.Apply(false, false);

//                string fileName = $"{Path.GetFileNameWithoutExtension(pngPath)}_{startIndex + count:D4}.png";
//                File.WriteAllBytes(Path.Combine(outFolder, fileName), newTex.EncodeToPNG());
//                count++;
//            }
//            catch (System.Exception ex)
//            {
//                Debug.LogWarning($"⚠️ Lỗi cắt {baseName}: {ex.Message}");
//            }
//        }

//        return count;
//    }
//    private static bool HasDepthDifference(JArray meshes, float threshold)
//    {
//        float zMin = float.MaxValue, zMax = float.MinValue;
//        foreach (var mesh in meshes)
//        {
//            var verts = mesh["verts"] as JArray;
//            if (verts == null) continue;
//            foreach (var v in verts)
//            {
//                float z = v[2].Value<float>();
//                zMin = Mathf.Min(zMin, z);
//                zMax = Mathf.Max(zMax, z);
//            }
//        }
//        return Mathf.Abs(zMax - zMin) > threshold;
//    }

//    private static List<Rect> MergeCloseMeshes(JArray meshes, int atlasW, int atlasH, float mergeThreshold = 4f)
//    {
//        List<Rect> rects = new List<Rect>();

//        foreach (var mesh in meshes)
//        {
//            var uvArr = mesh["uv"] as JArray;
//            if (uvArr == null || uvArr.Count < 4) continue;

//            float uMin = 1f, uMax = 0f, vMin = 1f, vMax = 0f;
//            foreach (var uv in uvArr)
//            {
//                float u = uv[0].Value<float>();
//                float v = uv[1].Value<float>();
//                uMin = Mathf.Min(uMin, u);
//                uMax = Mathf.Max(uMax, u);
//                vMin = Mathf.Min(vMin, v);
//                vMax = Mathf.Max(vMax, v);
//            }

//            // Chuyển UV sang pixel
//            Rect r = new Rect(
//                Mathf.RoundToInt(uMin * atlasW),
//                Mathf.RoundToInt((1f - vMax) * atlasH),
//                Mathf.RoundToInt((uMax - uMin) * atlasW),
//                Mathf.RoundToInt((vMax - vMin) * atlasH)
//            );

//            bool merged = false;
//            for (int i = 0; i < rects.Count; i++)
//            {
//                if (RectsTouchOrOverlap(rects[i], r, mergeThreshold))
//                {
//                    rects[i] = Union(rects[i], r);
//                    merged = true;
//                    break;
//                }
//            }

//            if (!merged)
//                rects.Add(r);
//        }

//        return rects;
//    }

//    private static bool ShouldBuild25D(JToken deck, int atlasW, int atlasH)
//    {
//        var meshes = deck["meshes"] as JArray;
//        if (meshes == null || meshes.Count == 0)
//            return false;

//        // ❌ Nếu có ít hơn 3 mesh => gần như chắc chắn là sprite
//        if (meshes.Count < 3)
//            return false;

//        // 📏 Tính độ sâu Z của toàn bộ mesh
//        float zMin = float.MaxValue, zMax = float.MinValue;
//        foreach (var mesh in meshes)
//        {
//            var verts = mesh["verts"] as JArray;
//            if (verts == null) continue;
//            foreach (var v in verts)
//            {
//                float z = v[2].Value<float>();
//                zMin = Mathf.Min(zMin, z);
//                zMax = Mathf.Max(zMax, z);
//            }
//        }

//        float depth = Mathf.Abs(zMax - zMin);

//        // Nếu độ sâu < 8 → vật phẳng hoặc chỉ nghiêng nhẹ
//        if (depth < 8f)
//            return false;

//        // ✅ Nếu độ sâu > 8 hoặc có mesh kích thước lớn → vật có khối
//        return true;
//    }

//    // ==============================================================
//    private static bool RectsTouchOrOverlap(Rect a, Rect b, float threshold)
//    {
//        return !(b.x > a.xMax + threshold ||
//                 b.xMax < a.x - threshold ||
//                 b.y > a.yMax + threshold ||
//                 b.yMax < a.y - threshold);
//    }

//    private static Rect Union(Rect a, Rect b)
//    {
//        float xMin = Mathf.Min(a.xMin, b.xMin);
//        float yMin = Mathf.Min(a.yMin, b.yMin);
//        float xMax = Mathf.Max(a.xMax, b.xMax);
//        float yMax = Mathf.Max(a.yMax, b.yMax);
//        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
//    }
//}
//#endif