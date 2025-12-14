using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 库存网格
/// </summary>
public class ItemGrid : MonoBehaviour
{
    const float TILE_SIZE_WIDTH = 256;
    const float TILE_SIZE_HEIGHT = 256;
    private static float simpleTileWidth; // 单个tile的宽度
    private static float simpleTileHeight; // 单个tile的高度
    [SerializeField] private int gridSizeWidth = 3;
    [SerializeField] private int gridSizeHeight = 3;
    private Vector2 positionOnTheGrid = new Vector2(); // 鼠标在rect上的位置
    private Vector2Int tileGridPosition = new Vector2Int(); // 鼠标在tile上的位置
    public InventoryItem[,] inventoryItemSlot; // 二维数组存储物品引用
    private RectTransform rectTransform; // 网格的RectTransform
    private UnityEngine.UI.Image image; // 网格的图片组件

    private Queue<InventoryItem> searchQueue = new Queue<InventoryItem>();
    private bool isSearching = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<UnityEngine.UI.Image>();
        simpleTileWidth = TILE_SIZE_WIDTH / image.pixelsPerUnitMultiplier;
        simpleTileHeight = TILE_SIZE_HEIGHT / image.pixelsPerUnitMultiplier;
        Init(gridSizeWidth, gridSizeHeight);
    }
    /// <summary>
    /// 初始化库存网格
    /// </summary>
    private void Init(int width, int height)
    {
        inventoryItemSlot = new InventoryItem[width, height];
        InitGrid(width, height);
    }
    /// <summary>
    /// 初始化库存网格的大小
    /// </summary>
    private void InitGrid(int width, int height)
    {
        Vector2 size = new Vector2(width * simpleTileWidth, height * simpleTileHeight);
        rectTransform.sizeDelta = size;
    }
    /// <summary>
    /// 获取鼠标在tile上的位置
    /// </summary>
    public Vector2Int GetTileGridPosition(Vector2 mousePosition)
    {
        // 获取鼠标在rect上的位置
        positionOnTheGrid.x = mousePosition.x - rectTransform.position.x;
        positionOnTheGrid.y = rectTransform.position.y - mousePosition.y;
        // 获取鼠标在tile上的位置
        tileGridPosition.x = (int)(positionOnTheGrid.x / simpleTileWidth);
        tileGridPosition.y = (int)(positionOnTheGrid.y / simpleTileHeight);
        return tileGridPosition;
    }
    /// <summary>
    /// 放置物品
    /// </summary>
    public bool PlaceItem(InventoryItem inventoryItem, int posX, int posY, ref InventoryItem overlarpItem)
    {
        if (!BoundryCheck(posX, posY, inventoryItem.Width, inventoryItem.Height))
        {
            return false;
        }

        if (OverlarpCheck(posX, posY, inventoryItem.Width, inventoryItem.Height, ref overlarpItem))
        {
            overlarpItem = null;
            return false;
        }

        if (overlarpItem != null)
        {
            CleanGridReference(overlarpItem);
        }

        PlaceItem(inventoryItem, posX, posY);
        return true;
    }

    public void PlaceItem(InventoryItem inventoryItem, int posX, int posY)
    {
        RectTransform itemRectTransform = inventoryItem.GetComponent<RectTransform>();

        // 更新网格数组
        for (int x = 0; x < inventoryItem.Width; x++)
        {
            for (int y = 0; y < inventoryItem.Height; y++)
            {
                inventoryItemSlot[posX + x, posY + y] = inventoryItem;
            }
        }

        inventoryItem.onGridPosX = posX;
        inventoryItem.onGridPosY = posY;

        // 使用世界坐标设置物品位置
        Vector3 worldPosition = CalculateWorldPosition(inventoryItem, posX, posY);
        itemRectTransform.position = worldPosition;
    }

    /// <summary>
    /// 计算物品在网格中的本地位置（相对于网格左上角）
    /// </summary>
    public Vector2 CalculatePositionOnGrid(InventoryItem inventoryItem, int posX, int posY)
    {
        Vector2 position = new Vector2
        {
            x = posX * simpleTileWidth + simpleTileWidth * inventoryItem.Width / 2,
            y = -(posY * simpleTileHeight + simpleTileHeight * inventoryItem.Height / 2)
        };
        return position;
    }

    /// <summary>
    /// 计算物品的世界坐标位置
    /// </summary>
    public Vector3 CalculateWorldPosition(InventoryItem inventoryItem, int posX, int posY)
    {
        // 先计算本地位置
        Vector2 localPosition = CalculatePositionOnGrid(inventoryItem, posX, posY);

        // 转换为世界坐标
        Vector3 gridWorldPos = rectTransform.position;

        return new Vector3(
            gridWorldPos.x + localPosition.x,
            gridWorldPos.y + localPosition.y,
            gridWorldPos.z
        );
    }

    /// <summary>
    /// 检查物品是否重叠
    /// 重叠返回true
    /// </summary>
    private bool OverlarpCheck(int posX, int posY, int width, int height, ref InventoryItem overlarpItem)
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if(inventoryItemSlot[posX + x, posY + y] != null)
                {
                    if(overlarpItem == null)
                    {
                        overlarpItem = inventoryItemSlot[posX + x, posY + y];
                    }
                    else if(overlarpItem != inventoryItemSlot[posX + x, posY + y])
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    /// <summary>
    /// 检查指定位置是否有足够的可用空间放置物品
    /// </summary>
    /// <returns>如果空间可用返回 true，如果被占用返回 false</returns>
    private bool CheckAvailableSpace(int posX, int posY, int width, int height)
    {
        // 先进行边界检查
        if (!BoundryCheck(posX, posY, width, height))
        {
            return false; // 超出边界，空间不可用
        }

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if(inventoryItemSlot[posX + x, posY + y] != null)
                {
                    return false; // 有物品占用，空间不可用
                }
            }
        }
        return true; // 空间可用
    }

    /// <summary>
    /// 拾取物品
    /// </summary>
    public InventoryItem PickUpItem(int posX, int posY)
    {
        // 物品数组的值为null，则返回null，否则返回物品数组的值，并将物品数组的值设置为null，表示物品被拾取了。
        InventoryItem item = inventoryItemSlot[posX, posY];
        if (item == null) return null;

        if(!item.CanPickUp) 
        {
            return null;
        }

        CleanGridReference(item);

        return item;
    }
    /// <summary>
    /// 清理物品在网格上的引用
    /// </summary>
    private void CleanGridReference(InventoryItem item)
    {
        for (int x = 0; x < item.Width; x++)
        {
            for (int y = 0; y < item.Height; y++)
            {
                inventoryItemSlot[item.onGridPosX + x, item.onGridPosY + y] = null;
            }
        }
    }

    public bool BoundryCheck(int posX, int posY, int width, int height)
    {
        if(!PositionCheck(posX, posY)) return false;
        posX += width - 1;
        posY += height - 1;
        if(!PositionCheck(posX, posY)) return false;
        return true;
    }
    bool PositionCheck(int posX, int poxY)
    {
        if(posX < 0 || poxY < 0)
        {
            return false;
        }
        if(posX >= gridSizeWidth || poxY >= gridSizeHeight)
        {
            return false;
        }
        return true;
    }

    public static float GetSimpleTileWidth()
    {
        return simpleTileWidth;
    }
    public static float GetSimpleTileHeight()
    {
        return simpleTileHeight;
    }

    public InventoryItem GetItem(int x, int y)
    {
        return inventoryItemSlot[x, y];
    }

    /// <summary>
    /// 为物品查找可用的放置空间
    /// </summary>
    /// <returns>找到空间返回位置坐标，否则返回 null</returns>
    public Vector2Int? FindSpaceForObject(InventoryItem itemToInsert)
    {
        int width = gridSizeWidth - itemToInsert.Width + 1;
        int height = gridSizeHeight - itemToInsert.Height + 1;

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if(CheckAvailableSpace(x, y, itemToInsert.Width, itemToInsert.Height)) // 空间可用时返回位置
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return null; // 没有找到空间，返回null。
    }

    /// <summary>
    /// 通知网格内所有物品开始搜索
    /// </summary>
    public void StartSearchAllItems()
    {
        if(isSearching) return;

        // 遍历已记录的所有物品（避免重复调用）
        HashSet<InventoryItem> processedItems = new HashSet<InventoryItem>();
        searchQueue.Clear();

        for (int x = 0; x < gridSizeWidth; x++)
        {
            for (int y = 0; y < gridSizeHeight; y++)
            {
                InventoryItem item = inventoryItemSlot[x, y];
                if (item != null && !processedItems.Contains(item) && !item.isSearched)
                {
                    processedItems.Add(item);
                    searchQueue.Enqueue(item);
                }
            }
        }

        SearchNextItem();
    }

    private void SearchNextItem()
    {
        if(searchQueue.Count == 0)
        {
            isSearching = false;
            return;
        }

        isSearching = true;
        InventoryItem item = searchQueue.Dequeue();
        // 传入回调，搜索完成后继续下一个
        item.StartSearch(OnItemSearchComplete);
    }
    private void OnItemSearchComplete()
    {
        SearchNextItem();
    }
}
