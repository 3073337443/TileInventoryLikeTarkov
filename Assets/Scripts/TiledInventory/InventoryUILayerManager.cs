using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI层级管理器 - 管理库存系统中各UI元素的层级关系
/// </summary>
public class InventoryUILayerManager : Singleton<InventoryUILayerManager>
{
    [Header("层级配置")]
    [SerializeField] private Transform layerParent; // 主canvas

    // 层级字典
    private Dictionary<InventoryUILayer, Transform> layerDict = new Dictionary<InventoryUILayer, Transform>();

    // 层级Canvas字典
    private Dictionary<InventoryUILayer, Canvas> layerCanvasDict = new Dictionary<InventoryUILayer, Canvas>();

    private bool isInitialized = false;

    private void Awake()
    {
        InitializeLayers();
    }

    /// <summary>
    /// 初始化所有层级
    /// </summary>
    private void InitializeLayers()
    {
        if (isInitialized) return;

        if (layerParent == null)
        {
            layerParent = GetComponent<Transform>();
            if (layerParent == null)
            {
                Debug.LogError("[InventoryUILayerManager] 未设置layerParent!");
                return;
            }
        }

        // 自动创建缺失的层级
        CreateLayers();

        isInitialized = true;
    }

    /// <summary>
    /// 自动创建缺失的层级容器
    /// </summary>
    private void CreateLayers()
    {
        // 遍历所有枚举值
        foreach (InventoryUILayer layerType in System.Enum.GetValues(typeof(InventoryUILayer)))
        {
            if (!layerDict.ContainsKey(layerType))
            {
                CreateLayerContainer(layerType);
            }
        }
    }

    /// <summary>
    /// 创建单个层级容器
    /// </summary>
    private void CreateLayerContainer(InventoryUILayer layerType)
    {
        // 创建层级GameObject
        GameObject layerObj = new GameObject($"Layer_{layerType}");
        layerObj.transform.SetParent(layerParent);

        // 添加RectTransform并设置为全屏
        RectTransform rectTransform = layerObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;

        // 添加Canvas组件用于排序
        Canvas canvas = layerObj.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = (int)layerType;

        // 添加GraphicRaycaster以支持UI交互
        layerObj.AddComponent<GraphicRaycaster>();

        // 注册到字典
        layerDict[layerType] = layerObj.transform;
        layerCanvasDict[layerType] = canvas;

    }

    /// <summary>
    /// 获取指定层级的Transform容器
    /// </summary>
    public Transform GetLayerTransform(InventoryUILayer layer)
    {
        if (!isInitialized) InitializeLayers();

        if (layerDict.TryGetValue(layer, out Transform container))
        {
            return container;
        }
        return layerParent;
    }

    /// <summary>
    /// 将UI元素移动到指定层级
    /// </summary>
    public void MoveToLayer(RectTransform target, InventoryUILayer layer)
    {
        if (target == null) return;

        Transform layerTransform = GetLayerTransform(layer);
        target.SetParent(layerTransform);
        target.SetAsLastSibling(); // 确保在该层级最上层
    }

    /// <summary>
    /// 将UI元素移动到指定层级并保持世界位置
    /// </summary>
    public void MoveToLayerKeepPosition(RectTransform target, InventoryUILayer layer)
    {
        if (target == null) return;

        Vector3 worldPos = target.position;
        Transform layerTransform = GetLayerTransform(layer);
        target.SetParent(layerTransform);
        target.position = worldPos;
        target.SetAsLastSibling();
    }

    public enum InventoryUILayer
    {
        Background = 0,      // 背景层
        GridBase = 100,     // 网格基础层
        Highlight = 200,      // 高亮层
        GridItems = 300,     // 网格物品层
        DragItem = 400,      // 拖拽物品层
        Tooltip = 500        // 提示信息层
    }
}
