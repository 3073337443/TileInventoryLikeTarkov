using UnityEngine;

/// <summary>
/// 子弹组件：处理距离检测和自动回收
/// </summary>
public class Bullet : MonoBehaviour
{
    private ShootBullet shooter;
    private float maxDistance;
    private Vector3 startPosition;
    private bool isInitialized;

    /// <summary>
    /// 初始化子弹（由 ShootBullet 调用一次）
    /// </summary>
    public void Initialize(ShootBullet shooterRef, float distance)
    {
        shooter = shooterRef;
        maxDistance = distance;
        isInitialized = true;
    }

    /// <summary>
    /// 重置子弹状态（每次发射时调用）
    /// </summary>
    public void ResetBullet(Vector3 firePosition)
    {
        startPosition = firePosition;
    }

    void Update()
    {
        if (!isInitialized) return;

        // 检测飞行距离
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);

        if (distanceTraveled >= maxDistance)
        {
            ReturnToPool();
        }
    }

    /// <summary>
    /// 回收子弹到对象池
    /// </summary>
    private void ReturnToPool()
    {
        if (shooter != null)
        {
            shooter.ReturnBulletToPool(gameObject);
        }
        else
        {
            // 如果没有 shooter 引用，直接销毁
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 碰撞时回收
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        // 可以在这里添加碰撞逻辑（伤害、特效等）
        collision.transform.GetComponent<Rigidbody>().AddForce(transform.forward * 1000);
        ReturnToPool();

        collision.transform.GetComponent<CubeHealth>()?.TakeDamage(10);
    }
}
