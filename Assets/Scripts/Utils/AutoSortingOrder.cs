using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AutoSortingOrder : MonoBehaviour
{
    public int offset = 0;  // nếu bạn muốn chỉnh lệch thêm (vd: để đầu không bị cắt)
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Càng thấp (y nhỏ) thì sortingOrder càng cao => vẽ nằm trước
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100) + offset;
    }
}
