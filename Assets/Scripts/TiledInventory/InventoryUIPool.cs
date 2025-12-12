using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 库存UI对象池 - 管理网格、物品、高亮等UI元素的对象池
/// 与InventoryUILayerManager配合使用，确保对象生成在正确的层级
/// </summary>
public class InventoryUIPool : Singleton<InventoryUIPool>
{
    [Header("预制体配置")]
    [SerializeField] private GameObject gridPrefab;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject highlightPrefab;

    [Header("对象池配置")]
    [SerializeField] private int initialGridCount = 2;
    [SerializeField] private int initialItemCount = 50;
    [SerializeField] private int initialHighlightCount = 5;

    // 对象池
    private Queue<ItemGrid> gridPool;
    private Queue<InventoryItem> itemPool;
    private Queue<InventoryHighlight> highlightPool;

    // 活跃对象追踪
    private List<ItemGrid> activeGrids;
    private List<InventoryItem> activeItems;
    private List<InventoryHighlight> activeHighlights;

    // 层级管理器引用
    private InventoryUILayerManager layerManager;

    private bool isInitialized = false;

    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// 初始化对象池
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;

        layerManager = InventoryUILayerManager.Instance;
        if (layerManager == null)
        {
            return;
        }

        // 初始化队列和列表
        gridPool = new Queue<ItemGrid>(initialGridCount);
        itemPool = new Queue<InventoryItem>(initialItemCount);
        highlightPool = new Queue<InventoryHighlight>(initialHighlightCount);

        activeGrids = new List<ItemGrid>(initialGridCount);
        activeItems = new List<InventoryItem>(initialItemCount);
        activeHighlights = new List<InventoryHighlight>(initialHighlightCount);

        // 预热对象池 - 严格按照配置数量
        WarmupGrids(initialGridCount);
        WarmupItems(initialItemCount);
        WarmupHighlights(initialHighlightCount);

        isInitialized = true;
    }

    /// <summary>
    /// 预热网格对象池
    /// </summary>
    private void WarmupGrids(int count)
    {
        for (int i = 0; i < count; i++)
        {
            ItemGrid grid = CreateGridObject();
            if (grid != null)
            {
                grid.gameObject.SetActive(false);
                gridPool.Enqueue(grid);
            }
        }
    }

    /// <summary>
    /// 预热物品对象池
    /// </summary>
    private void WarmupItems(int count)
    {
        for (int i = 0; i < count; i++)
        {
            InventoryItem item = CreateItemObject();
            if (item != null)
            {
                item.gameObject.SetActive(false);
                itemPool.Enqueue(item);
            }
        }
    }

    /// <summary>
    /// 预热高亮对象池
    /// </summary>
    private void WarmupHighlights(int count)
    {
        for (int i = 0; i < count; i++)
        {
            InventoryHighlight highlight = CreateHighlightObejct();
            if (highlight != null)
            {
                highlight.gameObject.SetActive(false);
                highlightPool.Enqueue(highlight);
            }
        }
    }

    #region 网格对象池

    /// <summary>
    /// 创建网格实例（内部方法）
    /// </summary>
    private ItemGrid CreateGridObject()
    {
        if (gridPrefab == null)
        {
            return null;
        }

        // 在GridBase层创建网格
        Transform parent = layerManager.GetLayerTransform(InventoryUILayerManager.InventoryUILayer.GridBase);
        GameObject gridObj = Instantiate(gridPrefab, parent);
        ItemGrid grid = gridObj.GetComponent<ItemGrid>();

        return grid;
    }

    /// <summary>
    /// 从池中获取网格
    /// </summary>
    public ItemGrid GetGridObject()
    {
        if (!isInitialized) Initialize();

        ItemGrid grid;

        if (gridPool.Count > 0)
        {
            grid = gridPool.Dequeue();
        }
        else
        {
            grid = CreateGridObject();
        }

        if (grid != null)
        {
            grid.gameObject.SetActive(true);
            activeGrids.Add(grid);

            // 确保在正确的层级
            layerManager.MoveToLayer(
                grid.GetComponent<RectTransform>(),
                InventoryUILayerManager.InventoryUILayer.GridBase
            );
        }

        return grid;
    }

    /// <summary>
    /// 归还网格到池中
    /// </summary>
    public void ReturnGridGameObject(ItemGrid grid)
    {
        if (grid == null) return;

        grid.gameObject.SetActive(false);
        activeGrids.Remove(grid);
        gridPool.Enqueue(grid);
    }

    #endregion

    #region 物品对象池

    /// <summary>
    /// 创建物品实例（内部方法）
    /// </summary>
    private InventoryItem CreateItemObject()
    {
        if (itemPrefab == null)
        {
            return null;
        }

        // 在GridItems层创建物品（默认位置，放置到网格时会重新设置父物体）
        Transform parent = layerManager.GetLayerTransform(InventoryUILayerManager.InventoryUILayer.GridItems);
        GameObject itemObj = Instantiate(itemPrefab, parent);
        InventoryItem item = itemObj.GetComponent<InventoryItem>();

        return item;
    }

    /// <summary>
    /// 从池中获取物品
    /// </summary>
    public InventoryItem GetItemObject()
    {
        if (!isInitialized) Initialize();

        InventoryItem item;

        if (itemPool.Count > 0)
        {
            item = itemPool.Dequeue();
        }
        else
        {
            item = CreateItemObject();
        }

        if (item != null)
        {
            item.gameObject.SetActive(true);
            activeItems.Add(item);

            // 重置物品状态
            ResetItemObejct(item);
        }

        return item;
    }

    /// <summary>
    /// 获取物品并设置数据
    /// </summary>
    public InventoryItem GetItemGamObject(ItemData itemData)
    {
        InventoryItem item = GetItemObject();
        if (item != null && itemData != null)
        {
            item.Set(itemData);
        }
        return item;
    }

    /// <summary>
    /// 归还物品到池中
    /// </summary>
    public void ReturnItemObject(InventoryItem item)
    {
        if (item == null) return;

        item.gameObject.SetActive(false);
        activeItems.Remove(item);

        // 移回GridItems层
        layerManager.MoveToLayer(
            item.GetComponent<RectTransform>(),
            InventoryUILayerManager.InventoryUILayer.GridItems
        );

        itemPool.Enqueue(item);
    }

    /// <summary>
    /// 重置物品状态
    /// </summary>
    private void ResetItemObejct(InventoryItem item)
    {
        if (item == null) return;

        item.onGridPosX = 0;
        item.onGridPosY = 0;
        item.rotated = false;

        RectTransform rect = item.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.rotation = Quaternion.identity;
        }
    }

    #endregion

    #region 高亮对象池

    /// <summary>
    /// 创建高亮实例（内部方法）
    /// </summary>
    private InventoryHighlight CreateHighlightObejct()
    {
        if (highlightPrefab == null)
        {
            return null;
        }

        // 在Highlight层创建高亮
        Transform parent = layerManager.GetLayerTransform(InventoryUILayerManager.InventoryUILayer.Highlight);
        GameObject highlightObj = Instantiate(highlightPrefab, parent);
        InventoryHighlight highlight = highlightObj.GetComponent<InventoryHighlight>();

        // 如果预制体没有InventoryHighlight组件，添加一个
        if (highlight == null)
        {
            highlight = highlightObj.AddComponent<InventoryHighlight>();
        }

        // 设置高亮的RectTransform（如果预制体本身就是高亮UI）
        RectTransform rectTransform = highlightObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            highlight.SetHighlightRectTransform(rectTransform);
        }

        return highlight;
    }

    /// <summary>
    /// 从池中获取高亮
    /// </summary>
    public InventoryHighlight GetHighlightObject()
    {
        if (!isInitialized) Initialize();

        InventoryHighlight highlight;

        if (highlightPool.Count > 0)
        {
            highlight = highlightPool.Dequeue();
        }
        else
        {
            highlight = CreateHighlightObejct();
        }

        if (highlight != null)
        {
            highlight.gameObject.SetActive(true);
            activeHighlights.Add(highlight);

            // 确保高亮在正确的层级
            RectTransform highlightRect = highlight.GetHighlightRectTransform();
            if (highlightRect != null)
            {
                layerManager.MoveToLayer(
                    highlightRect,
                    InventoryUILayerManager.InventoryUILayer.Highlight
                );
            }
        }

        return highlight;
    }

    /// <summary>
    /// 归还高亮到池中
    /// </summary>
    public void ReturnHighlightObject(InventoryHighlight highlight)
    {
        if (highlight == null) return;

        highlight.Reset();
        highlight.gameObject.SetActive(false);
        activeHighlights.Remove(highlight);

        // 确保归还时高亮仍在Highlight层
        RectTransform highlightRect = highlight.GetHighlightRectTransform();
        if (highlightRect != null)
        {
            layerManager.MoveToLayer(
                highlightRect,
                InventoryUILayerManager.InventoryUILayer.Highlight
            );
        }

        highlightPool.Enqueue(highlight);
    }

    #endregion

    #region 拖拽层级管理

    /// <summary>
    /// 将物品移动到拖拽层（拾取时调用）
    /// </summary>
    public void MoveItemToDragLayer(InventoryItem item)
    {
        if (item == null || layerManager == null) return;

        layerManager.MoveToLayerKeepPosition(
            item.GetComponent<RectTransform>(),
            InventoryUILayerManager.InventoryUILayer.DragItem
        );
    }

    #endregion

    #region 清理
    /// <summary>
    /// 清理所有对象池
    /// </summary>
    public void ClearAllPools()
    {
        if (!isInitialized) return;

        // 销毁所有池中对象
        while (gridPool.Count > 0)
        {
            var grid = gridPool.Dequeue();
            if (grid != null) Destroy(grid.gameObject);
        }

        while (itemPool.Count > 0)
        {
            var item = itemPool.Dequeue();
            if (item != null) Destroy(item.gameObject);
        }

        while (highlightPool.Count > 0)
        {
            var highlight = highlightPool.Dequeue();
            if (highlight != null) Destroy(highlight.gameObject);
        }

        // 清理活跃对象列表
        activeGrids.Clear();
        activeItems.Clear();
        activeHighlights.Clear();

    }

    /// <summary>
    /// 归还所有活跃对象到池中
    /// </summary>
    public void ReturnAllToPool()
    {
        if (!isInitialized) return;

        // 归还所有活跃网格
        for (int i = activeGrids.Count - 1; i >= 0; i--)
        {
            ReturnGridGameObject(activeGrids[i]);
        }

        // 归还所有活跃物品
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            ReturnItemObject(activeItems[i]);
        }

        // 归还所有活跃高亮
        for (int i = activeHighlights.Count - 1; i >= 0; i--)
        {
            ReturnHighlightObject(activeHighlights[i]);
        }
    }

    #endregion
}
