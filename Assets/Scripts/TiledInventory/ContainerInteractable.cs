using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 容器交互组件 - 挂载在可交互容器上，处理玩家交互
/// </summary>
public class ContainerInteractable : MonoBehaviour
{
    [SerializeField] private GameObject containerUIPrefab; // 容器UI预制体
    private InventoryUIPool uiPool;
    private string containerID; // 容器唯一标识
    private bool isInteractable = true; // 是否可交互
    public bool IsInteractable
    {
        get
        {
            return isInteractable;
        }
        set
        {
            isInteractable = value;
        }
    }

    void Awake()
    {
        containerID = Guid.NewGuid().ToString();
        uiPool = InventoryUIPool.Instance;
    }

    void Start()
    {
        // 创建容器ui实例
        uiPool.CreateContainerInstance(containerID, containerUIPrefab);
    }

    public string GetContainerID() => containerID;
}
