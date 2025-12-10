using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer; // 地面layer
    [SerializeField] private float speed; // 移速
    [SerializeField] private float rotateSpeed; // 转速
    [SerializeField] private float gravity; // 重力
    [SerializeField] private GameObject groundCheck; // 地面检测对象
    [SerializeField] private float checkRadius; // 检测范围

    private PlayerInputAction moveAction;
    private CharacterController characterController;
    private Vector2 movementInput;
    private Vector3 yVelocity = Vector3.zero;
    private bool isGround;
    void Awake()
    {
        // new一个输入映射 并获取CharacterController组件
        moveAction = new PlayerInputAction();
        characterController = transform.GetComponent<CharacterController>();
    }
    void OnEnable()
    {
        // 输入事件绑定
        if (moveAction != null)
        {
            moveAction.Movement.Enable();
            moveAction.Movement.Move.performed += GetMovementInput;
            moveAction.Movement.Move.canceled += GetMovementInput;
        }
    }
    void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.Movement.Move.performed -= GetMovementInput;
            moveAction.Movement.Move.canceled -= GetMovementInput;
        }
    }

    void Update()
    {
        RotateTowardMouse();

        Move();
    }
    /// <summary>
    /// 更新输入方向值
    /// </summary>
    private void GetMovementInput(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }
    /// <summary>
    /// 执行移动，根据全局坐标
    /// </summary>
    private void Move()
    {
        // 物理检测是否贴近地面
        isGround = Physics.CheckSphere(groundCheck.transform.position, checkRadius, groundLayer);
        if(isGround && yVelocity.y < 0)
        {
            yVelocity.y = 0;
        }
        yVelocity.y += gravity * Time.deltaTime;
        Vector3 move = new(movementInput.x, 0f, movementInput.y);
        if(move.sqrMagnitude > 1e-5f)
        {
            move = move.normalized;
            move *= speed;
        }
        Vector3 finalMove = move * Time.deltaTime + yVelocity * Time.deltaTime;
        characterController.Move(finalMove);
    }
    /// <summary>
    /// 角色朝向鼠标方向
    /// </summary>
    private void RotateTowardMouse()
    {
        if (Mouse.current == null) return;
        // 获取鼠标位置
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Camera camera = Camera.main;
        // 将鼠标点转换成一条从相机出发的 3D 射线
        Ray ray = camera.ScreenPointToRay(mouseScreenPos);
        // 检测射线与 groundLayer 的交点， 返回命中的信息（点、法线、碰到的对象等）
        if(Physics.Raycast(ray, out RaycastHit hitInfo, 1000f, groundLayer))
        {
            Vector3 targetPos = hitInfo.point;
            Vector3 lookDir = targetPos - transform.position;
            lookDir.y = 0f;

            if(lookDir.sqrMagnitude <= 1e-5f) return;

            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            // 平滑旋转
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
        }
    }
}
