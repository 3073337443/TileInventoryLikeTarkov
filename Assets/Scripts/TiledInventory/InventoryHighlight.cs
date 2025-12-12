using UnityEngine;

/// <summary>
/// 库存高亮显示组件
/// 始终在Highlight层显示，使用世界坐标定位
/// </summary>
public class InventoryHighlight : MonoBehaviour
{
    [SerializeField] private RectTransform highlightRectTransform;

    /// <summary>
    /// 设置高亮的RectTransform
    /// </summary>
    public void SetHighlightRectTransform(RectTransform rectTransform)
    {
        highlightRectTransform = rectTransform;
    }

    /// <summary>
    /// 获取高亮的RectTransform
    /// </summary>
    public RectTransform GetHighlightRectTransform()
    {
        return highlightRectTransform;
    }

    /// <summary>
    /// 显示/隐藏高亮
    /// </summary>
    public void Show(bool show)
    {
        if (highlightRectTransform == null) return;
        highlightRectTransform.gameObject.SetActive(show);
    }

    /// <summary>
    /// 设置高亮大小
    /// </summary>
    public void SetSize(InventoryItem targetItem)
    {
        if (highlightRectTransform == null || targetItem == null) return;

        Vector2 size = new Vector2
        {
            x = targetItem.Width * ItemGrid.GetSimpleTileWidth(),
            y = targetItem.Height * ItemGrid.GetSimpleTileHeight()
        };
        highlightRectTransform.sizeDelta = size;
    }

    /// <summary>
    /// 设置高亮位置（根据已放置物品的位置）
    /// </summary>
    public void SetPosition(ItemGrid targetGrid, InventoryItem targetItem)
    {
        if (highlightRectTransform == null || targetGrid == null || targetItem == null) return;

        // 直接使用ItemGrid的世界坐标计算方法
        Vector3 worldPosition = targetGrid.CalculateWorldPosition(
            targetItem,
            targetItem.onGridPosX,
            targetItem.onGridPosY
        );
        highlightRectTransform.position = worldPosition;
    }

    /// <summary>
    /// 设置高亮位置（根据指定的网格坐标）
    /// </summary>
    public void SetPosition(ItemGrid targetGrid, InventoryItem inventoryItem, int posX, int posY)
    {
        if (highlightRectTransform == null || targetGrid == null || inventoryItem == null) return;

        // 直接使用ItemGrid的世界坐标计算方法
        Vector3 worldPosition = targetGrid.CalculateWorldPosition(
            inventoryItem,
            posX,
            posY
        );
        highlightRectTransform.position = worldPosition;
    }

    /// <summary>
    /// 重置高亮状态
    /// </summary>
    public void Reset()
    {
        Show(false);

        if (highlightRectTransform != null)
        {
            highlightRectTransform.localPosition = Vector3.zero;
            highlightRectTransform.sizeDelta = Vector2.zero;
        }
    }
}
