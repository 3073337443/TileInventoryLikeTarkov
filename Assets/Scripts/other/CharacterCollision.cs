using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCollision : MonoBehaviour
{
    public float pushForce;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
    Rigidbody body = hit.collider.attachedRigidbody;

    // 如果物体具有Rigidbody且不为Kinematic
    if (body != null && !body.isKinematic)
    {
        // 施加力或做相应处理
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.AddForce(pushDir * pushForce, ForceMode.Impulse);
    }
    }
}
