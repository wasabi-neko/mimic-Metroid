using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    #region Parameters
    public Transform target;
    public float speed = 100;
    private Rigidbody2D rigidbody;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    // private void LateUpdate() 
    // {
    //     Vector3 moveSpeed = target.position - transform.position;
    //     transform.Translate(moveSpeed * speed);
    // }
    private void FixedUpdate() {
        Vector3 vec = (target.position - transform.position) * speed;
        rigidbody.velocity = vec;
    }
}
