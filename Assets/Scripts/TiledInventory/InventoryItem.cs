using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    public ItemData itemData;
    public int Width
    {
        get
        {
            return rotated ? itemData.height : itemData.width;
        }
    }
    public int Height
    {
        get
        {
            return rotated ? itemData.width : itemData.height;
        }
    }

    public int onGridPosX;
    public int onGridPosY;
    public bool rotated = false;

    internal void Rotated()
    {
        rotated = !rotated;
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.rotation = Quaternion.Euler(0, 0, rotated ? 90 : 0);
    }

    internal void Set(ItemData itemData)
    {
        this.itemData = itemData;
        GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>(itemData.spritePath); // 加载物品图片，路径为itemData中的spritePath。

        Vector2 size = new Vector2();
        size.x = itemData.width * ItemGrid.GetSimpleTileWidth(); // 使物品在tile的正中央显示
        size.y = itemData.height * ItemGrid.GetSimpleTileHeight(); // 使物品在tile的正中央显示
        GetComponent<RectTransform>().sizeDelta = size;
        
    }
}
