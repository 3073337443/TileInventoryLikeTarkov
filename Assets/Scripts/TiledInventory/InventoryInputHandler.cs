using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 库存输入处理器 - 连接 Input System 和库存系统
/// </summary>
public class InventoryInputHandler : MonoBehaviour
{
    [Header("输入配置")]
    [SerializeField] private InputActionReference backpackAction;
    [SerializeField] private InputActionReference closeContainerAction;
    [SerializeField] private InputActionReference openContainerAction;
    [SerializeField] private InputActionReference rotateItemAction;
    [SerializeField] private InputActionReference insertRandomItemAction;

    [Header("交互引用")]
    [SerializeField] private PlayerInteraction playerInteraction;

    [Header("库存引用")]
    [SerializeField] private InventoryController inventoryController;

    private InventoryUIManager uiManager;
    private void Awake()
    {
        // 获取引用
        uiManager = InventoryUIManager.Instance;

    }

    private void OnEnable()
    {
        backpackAction.action.performed += OnBackpackToggle;
        closeContainerAction.action.performed += OnCloseContainer;
        openContainerAction.action.performed += OnOpenContainer;
        rotateItemAction.action.performed += OnRotateItem;
        insertRandomItemAction.action.performed += OnInsertRandomItem;
    }
    private void OnDisable()
    {
        backpackAction.action.performed -= OnBackpackToggle;
        closeContainerAction.action.performed -= OnCloseContainer;
        openContainerAction.action.performed -= OnOpenContainer;
        rotateItemAction.action.performed -= OnRotateItem;
        insertRandomItemAction.action.performed -= OnInsertRandomItem;
    }
    private void OnInsertRandomItem(InputAction.CallbackContext context)
    {
        inventoryController.InsertRandomItem();
    }

    /// <summary>
    /// 旋转物品输入回调
    /// </summary>
    /// <param name="context"></param>
    private void OnRotateItem(InputAction.CallbackContext context)
    {
        inventoryController.RotateItem();
    }


    /// <summary>
    /// 背包切换输入回调
    /// </summary>
    private void OnBackpackToggle(InputAction.CallbackContext context)
    {
        if (uiManager != null)
        {
            uiManager.ToggleBackpack();
        }
    }
    /// <summary>
    /// 打开容器输入回调
    /// </summary>
    /// <param name="context"></param>
    private void OnOpenContainer(InputAction.CallbackContext context)
    {
        if(uiManager != null)
        {
            uiManager.OpenContainer(playerInteraction.GetNearestContainerID());
        }
    }

    /// <summary>
    /// 关闭容器输入回调
    /// </summary>
    /// <param name="context"></param>
    private void OnCloseContainer(InputAction.CallbackContext context)
    {
        if(uiManager != null)
        {
            uiManager.CloseContainer();
        }
    }
}

