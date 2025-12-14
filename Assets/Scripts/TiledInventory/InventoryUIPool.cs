using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 库存UI对象池 - 管理网格、物品、高亮等UI元素的对象池
/// 与InventoryUILayerManager配合使用，确保对象生成在正确的层级
/// </summary>
public class InventoryUIPool : Singleton<InventoryUIPool>
{
    [Header("预制体配置")]
    [SerializeField] private GameObject backpackPrefab;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject highlightPrefab;

    [Header("对象池配置")]
    [SerializeField] private int initialItemCount = 50;
    [SerializeField] private int initialHighlightCount = 5;

    private Queue<InventoryItem> itemPool;
    private Queue<InventoryHighlight> highlightPool;

    // 活跃对象追踪
    private List<InventoryItem> activeItems;
    private List<InventoryHighlight> activeHighlights;

    // 层级管理器引用
    private InventoryUILayerManager layerManager;

    // 物品容器字典 - 按库存系统名称组织
    private Dictionary<string, Transform> gridItemContainers;

    // 背包实例（单例）
    private GameObject backpackInstance;
    private ItemGrid backpackGrid;
    // 容器实例字典 - 按容器ID组织
    private Dictionary<string, GameObject> containerInstances = new Dictionary<string, GameObject>();

    private bool isInitialized = false;

    private void Start()
    {
        Initialize();
    }
    #region 初始化
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
        itemPool = new Queue<InventoryItem>(initialItemCount);
        highlightPool = new Queue<InventoryHighlight>(initialHighlightCount);
        
        activeItems = new List<InventoryItem>(initialItemCount);
        activeHighlights = new List<InventoryHighlight>(initialHighlightCount);

        // 初始化物品容器字典
        gridItemContainers = new Dictionary<string, Transform>();

        // 创建背包实例
        CreateBackpackInstance();

        // 预热对象池 - 严格按照配置数量
        WarmupItems(initialItemCount);
        WarmupHighlights(initialHighlightCount);

        isInitialized = true;
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
    #endregion

    #region 物品对象池

    /// <summary>
    /// 获取或创建物品容器（按库存系统名称）
    /// </summary>
    public Transform GetOrCreateGridItemContainer(ItemGrid grid)
    {
        if (grid == null) return null;

        string gridName = grid.gameObject.name;

        // 如果容器已存在，直接返回
        if (gridItemContainers.TryGetValue(gridName, out Transform container))
        {
            return container;
        }

        // 创建新容器
        Transform gridItemsLayer = layerManager.GetLayerTransform(InventoryUILayerManager.InventoryUILayer.GridItems);
        GameObject containerObj = new GameObject(gridName);
        containerObj.transform.SetParent(gridItemsLayer);

        // 添加RectTransform组件
        RectTransform rectTransform = containerObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;

        // 注册到字典
        gridItemContainers[gridName] = containerObj.transform;

        return containerObj.transform;
    }
    // InventoryUIPool.cs - 新增方法
    /// <summary>
    /// 设置网格对应的物品容器活动状态
    /// </summary>
    public void SetGridItemContainerActive(ItemGrid targetGrid, bool active)
    {
        if (targetGrid == null) return;
        
        Transform container = GetOrCreateGridItemContainer(targetGrid);
        if (container != null)
        {
            container.gameObject.SetActive(active);
        }
    }
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

        // 移回GridItems层（不放在容器下，因为已经失活）
        Transform gridItemsLayer = layerManager.GetLayerTransform(InventoryUILayerManager.InventoryUILayer.GridItems);
        item.transform.SetParent(gridItemsLayer);

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

    /// <summary>
    /// 将物品返回到网格层（放置时调用）
    /// </summary>
    public void MoveItemReturnToGridLayer(InventoryItem item, ItemGrid targetGrid)
    {
        if (item == null || layerManager == null) return;

        // 如果指定了目标网格，将物品放到对应的容器下
        if (targetGrid != null)
        {
            Transform container = GetOrCreateGridItemContainer(targetGrid);
            if (container != null)
            {
                Vector3 worldPos = item.transform.position;
                item.transform.SetParent(container);
                item.transform.position = worldPos; // 保持世界位置
                return;
            }
        }

        // 如果没有指定网格或容器创建失败，则移动到GridItems层
        layerManager.MoveToLayerKeepPosition(
            item.GetComponent<RectTransform>(),
            InventoryUILayerManager.InventoryUILayer.GridItems
        );
    }

    #endregion

    #region 背包管理
    /// <summary>
    /// 创建背包实例
    /// </summary>
    private void CreateBackpackInstance()
    {
        if (backpackPrefab == null)
        {
            return;
        }

        // 在 GridBase 层创建背包
        Transform gridBaseLayer = layerManager.GetLayerTransform(InventoryUILayerManager.InventoryUILayer.GridBase);
        backpackInstance = Instantiate(backpackPrefab, gridBaseLayer);
        backpackInstance.name = InventoryUIManager.Instance.backpackName;
        // 获取 ItemGrid 组件
        backpackGrid = backpackInstance.GetComponentInChildren<ItemGrid>();

        // 初始状态：隐藏背包
        backpackInstance.SetActive(false);

    }

    /// <summary>
    /// 根据名称获取背包（兼容接口）
    /// </summary>
    public (ItemGrid grid, GameObject panel) GetBackpackByName(string backpackName)
    {
        if (!isInitialized) Initialize();

        // 检查背包实例名称是否匹配
        if (backpackInstance != null && backpackInstance.name.Contains(backpackName))
        {
            return (backpackGrid, backpackInstance);
        }
        return (backpackGrid, backpackInstance);
    }
    
    #endregion

    #region 容器管理
    public void CreateContainerInstance(string containerID, GameObject containerPrefab)
    {
        if (containerPrefab == null)
        {
            return;
        }
        if(!containerInstances.ContainsKey(containerID))
        {
            Transform gridBaseLayer = layerManager.GetLayerTransform(InventoryUILayerManager.InventoryUILayer.GridBase);
            GameObject containerInstance = Instantiate(containerPrefab, gridBaseLayer);
            containerInstance.name = containerID;
            containerInstance.SetActive(false);
            containerInstances[containerID] = containerInstance;
        }

    }

    internal (ItemGrid grid, GameObject panel) GetContainerByName(string currentOpenContainerID)
    {
        if (containerInstances.TryGetValue(currentOpenContainerID, out GameObject containerInstance))
        {
            ItemGrid containerGrid = containerInstance.GetComponentInChildren<ItemGrid>();
            return (containerGrid, containerInstance);
        }
        return (null, null);
    }
    public ItemGrid GetContainerGrid(string containerID)
    {
        if (containerInstances.TryGetValue(containerID, out GameObject containerInstance))
        {
            ItemGrid containerGrid = containerInstance.GetComponentInChildren<ItemGrid>();
            return containerGrid;
        }
        return null;
    }

    #endregion
}
