using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootBullet : MonoBehaviour
{
    [Header("子弹设置")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float maxDistance = 100f; // 子弹最大飞行距离

    [Header("对象池设置")]
    [SerializeField] private int poolSize = 20; // 初始池大小

    private PlayerInputAction moveAction;
    private Queue<GameObject> bulletPool;
    private List<GameObject> activeBullets;

    void Awake()
    {
        moveAction = new PlayerInputAction();
        InitializePool();
    }

    /// <summary>
    /// 初始化对象池
    /// </summary>
    private void InitializePool()
    {
        bulletPool = new Queue<GameObject>();
        activeBullets = new List<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            CreateBulletForPool();
        }
    }

    /// <summary>
    /// 创建子弹并加入池中
    /// </summary>
    private GameObject CreateBulletForPool()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform);
        bullet.SetActive(false);

        // 添加或获取 Bullet 组件
        if (!bullet.TryGetComponent(out Bullet bulletComponent))
        {
            bulletComponent = bullet.AddComponent<Bullet>();
        }
        bulletComponent.Initialize(this, maxDistance);

        bulletPool.Enqueue(bullet);
        return bullet;
    }

    /// <summary>
    /// 从池中获取子弹
    /// </summary>
    private GameObject GetBulletFromPool()
    {
        Debug.Log("从池中获取子弹");
        // 池空了就扩展
        if (bulletPool.Count == 0)
        {
            CreateBulletForPool();
        }

        GameObject bullet = bulletPool.Dequeue();
        bullet.SetActive(true);
        activeBullets.Add(bullet);
        return bullet;
    }

    /// <summary>
    /// 将子弹回收到池中
    /// </summary>
    public void ReturnBulletToPool(GameObject bullet)
    {
        if (bullet == null) return;

        bullet.SetActive(false);
        bullet.transform.SetParent(transform);

        // 重置速度
        if (bullet.TryGetComponent(out Rigidbody rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        activeBullets.Remove(bullet);
        bulletPool.Enqueue(bullet);
    }

    void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.Movement.Enable();
            moveAction.Movement.Shoot.performed += Shoot;
        }
    }

    void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.Movement.Disable();
            moveAction.Movement.Shoot.performed -= Shoot;
        }
    }

    private void Shoot(InputAction.CallbackContext context)
    {
        // 从对象池获取子弹
        GameObject bullet = GetBulletFromPool();

        // 先脱离父物体
        bullet.transform.SetParent(null);

        // 设置位置和旋转
        bullet.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);

        // 初始化子弹（重置起始位置用于距离计算）
        if (bullet.TryGetComponent(out Bullet bulletComponent))
        {
            bulletComponent.ResetBullet(firePoint.position);
        }

        // 发射 - 确保 Rigidbody 状态正确
        if (bullet.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;           // 确保不是 Kinematic
            rb.velocity = Vector3.zero;        // 先清零
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(firePoint.forward * bulletSpeed, ForceMode.VelocityChange);
        }
    }
}
