using System.Collections.Generic;
using UnityEngine;

public class TilesSorting
{
    private List<SpriteRenderer> mSortIndices = new List<SpriteRenderer>();

    public TilesSorting()
    {
    }

    public void Clear()
    {
        mSortIndices.Clear();
    }

    public void Add(SpriteRenderer renderer)
    {
        if (!IsValid(renderer)) return;

        mSortIndices.Add(renderer);
        SetRenderOrderSafe(renderer, mSortIndices.Count);
    }

    public void Remove(SpriteRenderer renderer)
    {
        if (!IsValid(renderer)) return;

        mSortIndices.Remove(renderer);

        // Recalculate orders safely
        for (int i = 0; i < mSortIndices.Count; i++)
        {
            SetRenderOrderSafe(mSortIndices[i], i + 1);
        }
    }

    public void BringToTop(SpriteRenderer renderer)
    {
        if (!IsValid(renderer)) return;

        Remove(renderer);
        Add(renderer);
    }

    private void SetRenderOrderSafe(SpriteRenderer renderer, int index)
    {
        if (!IsValid(renderer)) return;

        // Đặt sorting order
        renderer.sortingOrder = index;

        // Đặt vị trí z để thể hiện thứ tự
        if (renderer.transform != null)
        {
            Vector3 p = renderer.transform.position;
            p.z = -index / 10.0f;
            renderer.transform.position = p;
        }
    }

    private bool IsValid(SpriteRenderer renderer)
    {
        // Renderer bị destroy → return false
        if (renderer == null) return false;

        // GameObject đã bị destroy → Unity trả về renderer != null nhưng .gameObject == null
        if (renderer.gameObject == null) return false;

        return true;
    }
}
