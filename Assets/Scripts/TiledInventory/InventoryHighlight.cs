using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InventoryHighlight : MonoBehaviour
{
    [SerializeField] private RectTransform highlightRectTransform;

    public void Show(bool show)
    {
        highlightRectTransform.gameObject.SetActive(show);
    }
    /// <summary>
    /// 设置高亮大小
    /// </summary>
    public void SetSize(InventoryItem targetItem)
    {
        Vector2 size = new Vector2();
        size.x = targetItem.itemData.width * ItemGrid.GetSimpleTileWidth();
        size.y = targetItem.itemData.height * ItemGrid.GetSimpleTileHeight();
        highlightRectTransform.sizeDelta = size;
    }
    /// <summary>
    /// 设置高亮位置
    /// </summary>
    public void SetPosition(ItemGrid targetGrid, InventoryItem targetItem)
    {
        Vector2 position = targetGrid.CalculatePositionOnGrid(
            targetItem,
            targetItem.onGridPosX,
            targetItem.onGridPosY
            );
        highlightRectTransform.localPosition = position;

    }

    public void SetPosition(ItemGrid targetGrid, InventoryItem inventoryItem, int posX, int posY)
    {
        Vector2 position = targetGrid.CalculatePositionOnGrid(
            inventoryItem, 
            posX, 
            posY
            );
        highlightRectTransform.localPosition = position;
    }    

    public void SetParent(ItemGrid targetGrid)
    {
        if(targetGrid == null) return;
        highlightRectTransform.SetParent(targetGrid.GetComponent<RectTransform>());
    }
}
