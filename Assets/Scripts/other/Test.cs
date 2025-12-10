using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    void Start()
    {
        StartCoroutine(SendMessge(HelloWorld));
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
    void HelloWorld(string s)
    {
        Debug.Log(s);
    }

    IEnumerator SendMessge(Action<string> callback)
    {
        yield return new WaitForSeconds(1f);
        callback?.Invoke("Hello World");
    }
    [ContextMenu("ConsoleTool")]
    void ConsoleTool()
    {

        // 绘制射线
        Debug.DrawRay(transform.position, new Vector3(9,9,9), Color.red);

        // 绘制线段
        Debug.DrawLine(transform.position, GameObject.FindWithTag("Player").transform.position, Color.green, 2f);

        Debug.LogError("Exception Error Syntax Unexpected string:KFC CrazyThursday need 50RMB");
    }
}
