using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    public ItemData itemData;
    public int onGridPosX;
    public int onGridPosY;

    internal void Set(ItemData itemData)
    {
        this.itemData = itemData;
        GetComponent<UnityEngine.UI.Image>().sprite = itemData.itemIcon;

        Vector2 size = new Vector2();
        size.x = itemData.width * ItemGrid.GetSimpleTileWidth(); // 使物品在tile的正中央显示
        size.y = itemData.height * ItemGrid.GetSimpleTileHeight(); // 使物品在tile的正中央显示
        GetComponent<RectTransform>().sizeDelta = size;
        
    }
}
