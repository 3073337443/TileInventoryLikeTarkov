using UnityEngine;

/// <summary>
/// 库存网格互动控制器
/// 使用对象池模式管理物品
/// </summary>
public class InventoryController : MonoBehaviour
{
    private ItemGrid selectedItemGrid;
    public ItemGrid SelectedItemGrid
    {
        get => selectedItemGrid;
        set
        {
            selectedItemGrid = value;
        }
    }

    private InventoryItem selectedItem;
    private InventoryItem overlarpItem;
    private InventoryItem itemToHighlight;
    private RectTransform rectTransform;
    private Vector2Int oldPositionOnGrid;

    // 对象池引用
    private InventoryUIPool uiPool;

    // 高亮组件（从对象池获取）
    private InventoryHighlight inventoryHighlight;

    private void Start()
    {
        uiPool = InventoryUIPool.Instance;

        // 从对象池获取高亮
        inventoryHighlight = uiPool.GetHighlightObject();
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
        DragSelectItem();

        if (selectedItemGrid == null)
        {
            if (inventoryHighlight != null)
            {
                inventoryHighlight.Show(false);
            }
            return;
        }

        HandleInput();
        HandleHighlight();
    }

    /// <summary>
    /// 处理输入
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            InsertRandomItem();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateItem();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CreateRandomItem();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            CreateAllItem();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            DeleteItem();
        }
        if (Input.GetMouseButtonDown(0))
        {
            LeftMousePressed();
        }
        if (Input.GetMouseButtonDown(1))
        {
            Vector2Int tileGridPosition = selectedItemGrid.GetTileGridPosition(Input.mousePosition);
            LogGridInfo(tileGridPosition);
        }
    }

    private void RotateItem()
    {
        if (selectedItem == null) return;
        selectedItem.Rotated();
    }

    private void InsertRandomItem()
    {
        if (selectedItemGrid == null) return;

        CreateRandomItem();
        InventoryItem itemToInsert = selectedItem;
        selectedItem = null;
        InsertItem(itemToInsert);
    }

    private void InsertItem(InventoryItem itemToInsert)
    {
        if (itemToInsert == null) return;

        Vector2Int? posOnGrid = selectedItemGrid.FindSpaceForObject(itemToInsert);
        if (posOnGrid == null)
        {
            uiPool.ReturnItemObject(itemToInsert);
            return;
        }
        selectedItemGrid.PlaceItem(itemToInsert, posOnGrid.Value.x, posOnGrid.Value.y);
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
    /// 拾取物品
    /// </summary>
    private void PickUpItem(Vector2Int tileGridPosition)
    {
        selectedItem = selectedItemGrid.PickUpItem(tileGridPosition.x, tileGridPosition.y);
        if (selectedItem != null)
        {
            rectTransform = selectedItem.GetComponent<RectTransform>();
            uiPool.MoveItemToDragLayer(selectedItem);
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
            selectedItem = null;
            if (overlarpItem != null)
            {
                selectedItem = overlarpItem;
                overlarpItem = null;
                rectTransform = selectedItem.GetComponent<RectTransform>();
                uiPool.MoveItemToDragLayer(selectedItem);
            }
        }
    }

    /// <summary>
    /// 删除物品
    /// </summary>
    private void DeleteItem()
    {
        if (selectedItem == null) return;

        uiPool.ReturnItemObject(selectedItem);
        selectedItem = null;
    }

    /// <summary>
    /// 创建随机物品
    /// </summary>
    private void CreateRandomItem()
    {
        InventoryItem inventoryItem = uiPool.GetItemObject();
        if (inventoryItem == null) return;

        int randomItemID = Random.Range(0, ItemDataManager.Instance.itemDataList.Count);
        inventoryItem.Set(ItemDataManager.Instance.itemDataList[randomItemID]);

        uiPool.MoveItemToDragLayer(inventoryItem);

        selectedItem = inventoryItem;
        rectTransform = inventoryItem.GetComponent<RectTransform>();
    }

    /// <summary>
    /// 创建所有物品
    /// </summary>
    private void CreateAllItem()
    {
        if (selectedItemGrid == null) return;

        for (int i = 0; i < ItemDataManager.Instance.itemDataList.Count; i++)
        {
            InventoryItem inventoryItem = uiPool.GetItemGamObject(ItemDataManager.Instance.itemDataList[i]);
            if (inventoryItem != null)
            {
                InsertItem(inventoryItem);
            }
        }

        selectedItem = null;
    }

    private void LogGridInfo(Vector2Int tileGridPosition)
    {
        selectedItemGrid.LogGridItemInfo(tileGridPosition);
    }
}