using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private Transform playerTransform;
    private ContainerInteractable nearestContainer;
    private float detectionRadius = 3f;
    [SerializeField]private LayerMask containerLayer;

    private void Awake()
    {
        playerTransform = this.transform;
    }


    private void Update()
    {
        if (FindNearestContainer() == null)
        {
            InventoryUIManager.Instance.CloseContainer();
        }
    }
    /// <summary>
    /// 查找最近的可交互容器
    /// </summary>
    /// <returns></returns>
    private ContainerInteractable FindNearestContainer()
    {
        var colliders = Physics.OverlapSphere(playerTransform.position, detectionRadius, containerLayer);

        nearestContainer = null;
        float minDistance = float.MaxValue;

        foreach (var collider in colliders)
        {
            var container = collider.GetComponent<ContainerInteractable>();
            if (container != null && container.IsInteractable)
            {
                float distance = Vector3.Distance(playerTransform.position, collider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestContainer = container;
                }
            }
        }
        return nearestContainer;
    }
    /// <summary>
    /// 返回最近可交互容器的ID
    /// </summary>
    /// <returns></returns>
    public string GetNearestContainerID()
    {
        if (nearestContainer == null) return null;
        return nearestContainer.GetContainerID();
    }
}
