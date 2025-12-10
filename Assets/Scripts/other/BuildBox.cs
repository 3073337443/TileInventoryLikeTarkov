using System.Collections;
using UnityEngine;

public class BuildBox : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private int rows;
    [SerializeField] private int columns;
    [SerializeField] private int layers;
    public float spacing = 1.0f;
    public Vector3 gridOffset = new Vector3(5, 0.5f, 5);

    [Header("分帧生成设置")]
    [Tooltip("每帧生成的物体数量")]
    [SerializeField] private int objectsPerFrame = 100;

    private Coroutine generateCoroutine;

    void Start()
    {
        GenerateCubeGrid();
    }

    /// <summary>
    /// 开始生成立方体网格（使用协程分帧）
    /// </summary>
    public void GenerateCubeGrid()
    {
        if (generateCoroutine != null)
        {
            StopCoroutine(generateCoroutine);
        }
        generateCoroutine = StartCoroutine(GenerateCubeGridCoroutine());
    }
    /// <summary>
    /// 协程分帧生成，避免卡顿
    /// </summary>
    private IEnumerator GenerateCubeGridCoroutine()
    {
        int count = 0;
        int createdObjects = 0;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                for (int h = 0; h < layers; h++)
                {
                    Vector3 position = gridOffset + new Vector3(
                        i * spacing,
                        h * spacing,
                        j * spacing
                    );
                    Instantiate(cubePrefab, position, Quaternion.identity, transform);

                    count++;
                    createdObjects++;

                    // 每生成 objectsPerFrame 个物体后，等待下一帧
                    if (count >= objectsPerFrame)
                    {
                        count = 0;
                        yield return null; // 等待下一帧
                    }
                }
            }
        }

    }
}
