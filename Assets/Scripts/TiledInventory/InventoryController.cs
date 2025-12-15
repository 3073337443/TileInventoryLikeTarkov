using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 库存网格互动控制器
/// 使用对象池模式管理物品
/// </summary>
public class InventoryController : Singleton<InventoryController>
{
    [Header("物品生成配置")]
    [SerializeField] private ContainerGenerationData generationData;
    private MarkovItemGenerator itemGenerator;

    private ItemGrid selectedItemGrid; // 当前选中的网格
    public ItemGrid SelectedItemGrid
    {
        get => selectedItemGrid;
        set
        {
            selectedItemGrid = value;
        }
    }

    private InventoryItem selectedItem; // 当前拖拽的物品
    private InventoryItem overlarpItem; // 重叠的物品
    private InventoryItem itemToHighlight; // 高亮物品
    private RectTransform rectTransform; // 拖拽物品的RectTransform
    private Vector2Int oldPositionOnGrid; // 上一次在网格中的位置

    // 对象池引用
    private InventoryUIPool uiPool;

    // 高亮组件（从对象池获取）
    private InventoryHighlight inventoryHighlight;

    private void Start()
    {
        uiPool = InventoryUIPool.Instance;

        // 从对象池获取高亮
        inventoryHighlight = uiPool.GetHighlightObject();

        if(generationData != null)
        {
            itemGenerator = new MarkovItemGenerator(generationData, ItemDataManager.Instance);
        }
    }

    private void OnDestroy()
    {
        // 归还高亮到对象池
        if (uiPool != null && inventoryHighlight != null)
        {
            uiPool.ReturnHighlightObject(inventoryHighlight);
            inventoryHighlight = null;
        }
    }

    private void Update()
    {
        // 拖拽物品跟随鼠标
        DragSelectItem();

        if (selectedItemGrid == null)
        {
            if (inventoryHighlight != null)
            {
                inventoryHighlight.Show(false);
            }
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            LeftMousePressed();
        }
        // 处理高亮
        HandleHighlight();
    }

    #region 物品操作
    /// <summary>
    /// 鼠标左键按下
    /// </summary>
    private void LeftMousePressed()
    {
        Vector2Int tileGridPosition = GetTileGridPosition();

        if (selectedItem != null)
        {
            PlaceItem(tileGridPosition);
        }
        else
        {
            PickUpItem(tileGridPosition);
        }
    }    
    
    /// <summary>
    /// 旋转物品
    /// </summary>
    public void RotateItem()
    {
        if (selectedItem == null) return;
        selectedItem.Rotated();
    }   

    /// <summary>
    /// 向指定网格插入物品
    /// </summary>
    /// <param name="itemToInsert"></param>
    private void InsertItem(InventoryItem itemToInsert, ItemGrid targetGrid)
    {
        if (itemToInsert == null) return;
        // 查找空闲位置
        Vector2Int? posOnGrid = targetGrid.FindSpaceForObject(itemToInsert);
        if (posOnGrid == null)
        {
            uiPool.ReturnItemObject(itemToInsert);
            return;
        }
        uiPool.MoveItemReturnToGridLayer(itemToInsert, targetGrid);
        targetGrid.PlaceItem(itemToInsert, posOnGrid.Value.x, posOnGrid.Value.y);
    }
    #endregion

    /// <summary>
    /// 获取鼠标在tile上的位置
    /// </summary>
    /// <returns></returns>
    private Vector2Int GetTileGridPosition()
    {
        Vector2 position = Input.mousePosition;
        if (selectedItem != null)
        {
            position.x -= (selectedItem.Width - 1) * ItemGrid.GetSimpleTileWidth() / 2;
            position.y += (selectedItem.Height - 1) * ItemGrid.GetSimpleTileHeight() / 2;
        }
        return selectedItemGrid.GetTileGridPosition(position);
    }

    /// <summary>
    /// 处理高亮
    /// </summary>
    private void HandleHighlight()
    {
        if (inventoryHighlight == null) return;

        Vector2Int positionOnGrid = GetTileGridPosition();

        if (positionOnGrid == oldPositionOnGrid) return;
        oldPositionOnGrid = positionOnGrid;

        if (selectedItem == null)
        {
            itemToHighlight = selectedItemGrid.GetItem(positionOnGrid.x, positionOnGrid.y);

            if (itemToHighlight != null)
            {
                inventoryHighlight.Show(true);
                inventoryHighlight.SetSize(itemToHighlight);
                inventoryHighlight.SetPosition(selectedItemGrid, itemToHighlight);
            }
            else
            {
                inventoryHighlight.Show(false);
            }
        }
        else
        {
            inventoryHighlight.Show(selectedItemGrid.BoundryCheck(
                positionOnGrid.x,
                positionOnGrid.y,
                selectedItem.Width,
                selectedItem.Height
            ));
            inventoryHighlight.SetSize(selectedItem);
            inventoryHighlight.SetPosition(selectedItemGrid, selectedItem, positionOnGrid.x, positionOnGrid.y);
        }
    }

    /// <summary>
    /// 拖拽选中物品
    /// </summary>
    private void DragSelectItem()
    {
        if (selectedItem == null) return;
        rectTransform.position = Input.mousePosition;
    }

    /// <summary>
    /// 拾取物品
    /// </summary>
    private void PickUpItem(Vector2Int tileGridPosition)
    {
        selectedItem = selectedItemGrid.PickUpItem(tileGridPosition.x, tileGridPosition.y);
        if (selectedItem != null)
        {
            rectTransform = selectedItem.GetComponent<RectTransform>();
            uiPool.MoveItemToDragLayer(selectedItem);
            rectTransform.SetAsLastSibling();

            selectedItem.SetDragTransparency(true);
        }
    }

    /// <summary>
    /// 放置物品
    /// </summary>
    private void PlaceItem(Vector2Int tileGridPosition)
    {
        rectTransform = selectedItem.GetComponent<RectTransform>();
        bool complete = selectedItemGrid.PlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y, ref overlarpItem);

        if (complete)
        {
            selectedItem.SetDragTransparency(false);

            rectTransform.SetAsLastSibling();
            uiPool.MoveItemReturnToGridLayer(selectedItem, selectedItemGrid);
            selectedItem = null;
            if (overlarpItem != null)
            {
                selectedItem = overlarpItem;
                uiPool.MoveItemToDragLayer(selectedItem);
                overlarpItem = null;
                rectTransform = selectedItem.GetComponent<RectTransform>();

                selectedItem.SetDragTransparency(true);
            }

        }
    }

    /// <summary>
    /// 将手上拿着的物品放回指定网格的空闲位置
    /// </summary>
    /// <param name="targetGrid">目标网格</param>
    public void ReturnHeldItemToGrid(ItemGrid targetGrid)
    {
        if (selectedItem == null || targetGrid == null) return;
        selectedItem.SetDragTransparency(false);
        // 查找空闲位置
        Vector2Int? freePos = targetGrid.FindSpaceForObject(selectedItem);

        if (freePos != null)
        {
            // 放置物品到空闲位置
            targetGrid.PlaceItem(selectedItem, freePos.Value.x, freePos.Value.y);
            uiPool.MoveItemReturnToGridLayer(selectedItem, targetGrid);
        }
        else
        {
            // 没有空间则归还到对象池
            uiPool.ReturnItemObject(selectedItem);
        }

        // 清除引用
        selectedItem = null;
        rectTransform = null;

        // 隐藏高亮
        if (inventoryHighlight != null)
        {
            inventoryHighlight.Show(false);
        }
    }

    #region 测试方法
    /// <summary>
    /// i键添加物品测试方法
    /// </summary>
    public void InsertRandomItem()
    {
        if (selectedItemGrid == null) return;

        CreateRandomItem();
        InventoryItem itemToInsert = selectedItem;
        selectedItem = null;
        InsertItem(itemToInsert, selectedItemGrid);
    }
    public void InsertRandomItem(ItemGrid targetGrid)
    {
        if (targetGrid == null) return;

        InventoryItem inventoryItem = uiPool.GetItemObject();
        if (inventoryItem == null) return;

        int randomItemID = Random.Range(0, ItemDataManager.Instance.GetItemCount());
        inventoryItem.Set(ItemDataManager.Instance.GetItemDataByID(randomItemID));
        uiPool.MoveItemReturnToGridLayer(inventoryItem, targetGrid);

        InsertItem(inventoryItem, targetGrid);

        uiPool.SetGridItemContainerActive(targetGrid, false);
    }
    /// <summary>
    /// 为容器生成物品（使用马尔科夫链）
    /// </summary>
    public void GenerateItemsForContainer(ItemGrid targetGrid, ContainerType containerType)
    {
        if (targetGrid == null || itemGenerator == null) 
        {
            Debug.LogWarning("无法生成物品：targetGrid或itemGenerator为空");
            return;
        }
        
        List<ItemData> itemsToGenerate = itemGenerator.GenerateItems(containerType, targetGrid.GridWidth, targetGrid.GridHeight);
        
        foreach (ItemData itemData in itemsToGenerate)
        {
            InventoryItem inventoryItem = uiPool.GetItemObject();
            if (inventoryItem == null) continue;
            
            inventoryItem.Set(itemData);
            uiPool.MoveItemReturnToGridLayer(inventoryItem, targetGrid);
            InsertItem(inventoryItem, targetGrid);
        }
        
        uiPool.SetGridItemContainerActive(targetGrid, false);
    }
    /// <summary>
    /// 创建随机物品
    /// </summary>
    private void CreateRandomItem()
    {
        InventoryItem inventoryItem = uiPool.GetItemObject();
        if (inventoryItem == null) return;

        int randomItemID = Random.Range(0, ItemDataManager.Instance.GetItemCount());
        inventoryItem.Set(ItemDataManager.Instance.GetItemDataByID(randomItemID));

        uiPool.MoveItemToDragLayer(inventoryItem);

        selectedItem = inventoryItem;
        rectTransform = inventoryItem.GetComponent<RectTransform>();
    }
    #endregion
}