using System;
using UnityEngine;

/// <summary>
/// 库存 UI 管理器 - 统一管理背包和容器的显示
/// </summary>
public class InventoryUIManager : Singleton<InventoryUIManager>
{
    [Header("背包配置")]
    public string backpackName = "Backpack";

    [Header("引用")]
    private InventoryUIPool uiPool;

    // 背包引用（从对象池获取）
    private ItemGrid backpackGrid;
    private GameObject backpackPanel;
    private GameObject backpackContainer;

    // 容器引用（从对象池获取）
    private ItemGrid containerGrid;
    private GameObject containerPanel;
    private GameObject containerContainer;

    // 状态
    private bool isBackpackOpen = false;
    private bool isContainerOpen = false;

    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Initialize()
    {
        uiPool = InventoryUIPool.Instance;

        // 从对象池获取背包
        var backpackInfo = uiPool.GetBackpackByName(backpackName);
        backpackGrid = backpackInfo.grid;
        backpackPanel = backpackInfo.panel;
        backpackContainer = uiPool.GetOrCreateGridItemContainer(backpackGrid).gameObject;

        SetBackpackActive(false);

    }

    #region 背包控制

    /// <summary>
    /// 切换背包开关状态
    /// </summary>
    public void ToggleBackpack()
    {
        if (isBackpackOpen)
        {
            CloseBackpack();
        }
        else
        {
            OpenBackpack();
        }
    }

    /// <summary>
    /// 打开背包
    /// </summary>
    private void OpenBackpack()
    {
        if (isBackpackOpen)
        {
            return;
        }

        if (backpackGrid == null || backpackPanel == null)
        {
            return;
        }

        // 显示背包面板
        SetBackpackActive(true);

        isBackpackOpen = true;
    }

    /// <summary>
    /// 关闭背包
    /// </summary>
    private void CloseBackpack()
    {
        if (!isBackpackOpen)
        {
            return;
        }

        if (backpackGrid == null || backpackPanel == null)
        {
            return;
        }

        // 隐藏背包面板
        SetBackpackActive(false);

        isBackpackOpen = false;
    }
    /// <summary>
    /// 设置背包面板活动状态
    /// </summary>
    /// <param name="active"></param>
    private void SetBackpackActive(bool active)
    {
        if (backpackPanel == null) return;

        backpackPanel.SetActive(active);
        backpackContainer.SetActive(active);
    }
    #endregion

    #region 容器控制
    /// <summary>
    /// 打开容器
    /// </summary>
    /// <param name="containerID"></param>
    public void OpenContainer(string containerID)
    {
        if (isContainerOpen) return;

        if (containerID == null) return;


        GetContainer(containerID);

        if (containerGrid == null || containerPanel == null) return;
        
        // 显示容器面板
        SetContainerActive(true);
        isContainerOpen = true;

        containerGrid.StartSearchAllItems();
    }
    /// <summary>
    /// 关闭容器
    /// </summary>
    public void CloseContainer()
    {
        if (!isContainerOpen)
        {
            return;
        }

        if (containerGrid == null || containerPanel == null)
        {
            return;
        }
        containerGrid.StopSearchAllItems();
        // 隐藏容器面板
        SetContainerActive(false);

        isContainerOpen = false;
    }
    public void SetItemContainerActive(bool isActive)
    {
        if (containerContainer == null) return;

        containerContainer.SetActive(isActive);
    }
    /// <summary>
    /// 设置容器面板活动状态
    /// </summary>
    /// <param name="isActive"></param>
    private void SetContainerActive(bool isActive)
    {
        if (containerPanel == null) return;

        containerPanel.SetActive(isActive);
        containerContainer.SetActive(isActive);
    }

    /// <summary>
    /// 根据ID获取容器数据
    /// </summary>
    /// <param name="containerID"></param>
    private void GetContainer(string containerID)
    {
        var containerInfo = uiPool.GetContainerByName(containerID);
        containerGrid = containerInfo.grid;
        containerPanel = containerInfo.panel;
        containerContainer = uiPool.GetOrCreateGridItemContainer(containerGrid).gameObject;
    }
    #endregion
    
}

