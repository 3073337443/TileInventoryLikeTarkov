using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 库存网格互动控制器
/// </summary>
public class InventoryController : MonoBehaviour
{
    [SerializeField] private GameObject inventoryItemPrefab;
    [SerializeField] private Transform canvasTransform;
    
    private ItemGrid selectedItemGrid;
    public ItemGrid SelectedItemGrid 
    {
         get => selectedItemGrid; 
         set {
            selectedItemGrid = value; 
            inventoryHighlight.SetParent(value);
        }
    }
    private InventoryItem selectedItem;
    private InventoryItem overlarpItem;
    private InventoryItem ItemToHighlight;
    private RectTransform rectTransform;
    private InventoryHighlight inventoryHighlight;
    private Vector2Int oldPositionOnGrid;

    private void Awake()
    {
        inventoryHighlight = GetComponent<InventoryHighlight>();
    }


    private void Update()
    {
        DragSelectItem();
        if (selectedItemGrid == null) 
        {
            inventoryHighlight.Show(false);
            return;
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            InsertRandomItem();
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            RotateItem();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("生成随机物品");
            CreateRandomItem();
        }
        if(Input.GetKeyDown(KeyCode.A))
        {
            CreateAllItem();
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            DeleteItem();
        }
        HandleHighlight();
        if(Input.GetMouseButtonDown(0))
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
        if(selectedItemGrid == null) return;
        CreateRandomItem();
        InventoryItem itemToInsert = selectedItem;
        selectedItem = null;
        InsertItem(itemToInsert);
    }

    private void InsertItem(InventoryItem itemToInsert)
    {
        Vector2Int? posOnGrid = selectedItemGrid.FindSpaceForObject(itemToInsert);
        if (posOnGrid == null)
        {
            Destroy(itemToInsert.gameObject);
            return;
        }
        selectedItemGrid.PlaceItem(itemToInsert, posOnGrid.Value.x, posOnGrid.Value.y);
    }

    /// <summary>
    /// 处理高亮
    /// </summary>
    private void HandleHighlight()
    {            
        Vector2Int positionOnGrid = GetTileGridPosition();

        if(positionOnGrid == oldPositionOnGrid) return;
        oldPositionOnGrid = positionOnGrid;

        if (selectedItem == null)
        {
            ItemToHighlight = selectedItemGrid.GetItem(positionOnGrid.x, positionOnGrid.y);

            if (ItemToHighlight != null)
            {
                inventoryHighlight.Show(true);
                inventoryHighlight.SetSize(ItemToHighlight);
                inventoryHighlight.SetPosition(selectedItemGrid, ItemToHighlight);
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
        // 获取鼠标在tile上的位置
        return selectedItemGrid.GetTileGridPosition(position);;
    }

    /// <summary>
    /// 拾取物品
    /// </summary>
    private void PickUpItem(Vector2Int tileGridPosition)
    {
        selectedItem = selectedItemGrid.PickUpItem(tileGridPosition.x, tileGridPosition.y);
        if(selectedItem != null)
        {
            rectTransform = selectedItem.GetComponent<RectTransform>();
            rectTransform.SetParent(canvasTransform);
            rectTransform.SetAsLastSibling();
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
            if(overlarpItem != null)
            {
                selectedItem = overlarpItem;
                overlarpItem = null;
                rectTransform = selectedItem.GetComponent<RectTransform>();
                rectTransform.SetAsLastSibling();
            }
        }
        
    }
    void DeleteItem()
    {
        if (selectedItem == null) return;
        Destroy(selectedItem.gameObject);
        selectedItem = null;
    }
    void CreateRandomItem()
    {
        InventoryItem inventoryItem = Instantiate(inventoryItemPrefab).GetComponent<InventoryItem>();
        selectedItem = inventoryItem;

        rectTransform = inventoryItem.GetComponent<RectTransform>();
        rectTransform.SetParent(canvasTransform);
        rectTransform.SetAsLastSibling();

        int selectedItemID = UnityEngine.Random.Range(0, ItemDataManager.Instance.itemDataList.Count);; // 随机生成物品id，从0到物品列表的长度之间随机生成一个数值，作为物品id。
        inventoryItem.Set(ItemDataManager.Instance.itemDataList[selectedItemID]); // 根据物品id获取物品数据，并设置物品。
        
    }
    void CreateAllItem()
    {
        if (selectedItemGrid == null) return;

        for (int i = 0; i < ItemDataManager.Instance.itemDataList.Count; i++)
        {
            // 为每个物品创建新的实例
            InventoryItem inventoryItem = Instantiate(inventoryItemPrefab).GetComponent<InventoryItem>();

            rectTransform = inventoryItem.GetComponent<RectTransform>();
            rectTransform.SetParent(canvasTransform);
            rectTransform.SetAsLastSibling();

            // 设置物品数据
            inventoryItem.Set(ItemDataManager.Instance.itemDataList[i]);

            // 插入物品到网格
            InsertItem(inventoryItem);
        }

        // 清空选中物品，因为所有物品都已放置
        selectedItem = null;
    }
    private void LogGridInfo(Vector2Int tileGridPosition)
    {
        selectedItemGrid.LogGridItemInfo(tileGridPosition);
    }
}
