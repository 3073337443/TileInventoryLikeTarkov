using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 网格互动
/// </summary>
[RequireComponent(typeof(ItemGrid))]
public class GridInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private InventoryController inventoryController;
    private ItemGrid itemGrid;


    void Awake()
    {
        inventoryController = FindObjectOfType(typeof(InventoryController)) as InventoryController;
        itemGrid = GetComponent<ItemGrid>();
    }    


    public void OnPointerEnter(PointerEventData eventData)
    {
        inventoryController.SelectedItemGrid = itemGrid;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inventoryController.SelectedItemGrid = null;
    }
    private void OnDisable()
    {
        if (inventoryController == null) return;

        // 当 Grid 被禁用时，处理手上拿着的物品
        inventoryController.ReturnHeldItemToGrid(itemGrid);

        // 清除 Grid 引用
        if (inventoryController.SelectedItemGrid == itemGrid)
        {
            inventoryController.SelectedItemGrid = null;
        }
    }
}
