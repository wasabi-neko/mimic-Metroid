using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster01 : MonoBehaviour
{
    #region Monster Settings
    [Header("Abilities")]
    public int hpMax = 10;
    public int hp;
    public int atk = 1;
    public float moveSpeed = 1;
    [Header("Private settings")]
    public int direction = 1;
    public float rotateSpeed = 90;
    public float holdWallForce = 10;    // the force that aginst the wall(to help the monster stay on the wall by using friction)
    public PlayerGroundChecker _groundChecker;
    public PlayerGroundChecker _LLChecker;  // left lower checker
    public PlayerGroundChecker _RLChecker;  // right lower checker
    public PlayerGroundChecker _LUChecker; // left upper checker
    public PlayerGroundChecker _RUChecker; // right upper chekcer
    private Rigidbody2D _rigidbody2D;
    #endregion
    // Start is called before the first frame update

    #region Monster Methods
    private void MoveUpdate()
    {
        if (_groundChecker.isGrounded)
        {
            Vector2 moveForce = transform.right * direction * moveSpeed;
            Vector2 normalFroce = new Vector2(transform.right.y, -transform.right.x);
            _rigidbody2D.velocity = moveForce + normalFroce;
        }
        else
        {
            Vector2 vec = new Vector2(0, _rigidbody2D.velocity.y);
            _rigidbody2D.velocity = vec;
        }
    }

    private void HoldWallForceUpdate()
    {
        if (_groundChecker.isGrounded)
        {
            Vector3 force = Quaternion.Euler(0, -90, 0) * transform.right;
            force *= holdWallForce;
            _rigidbody2D.AddForce(force);
        }
    }

    private void RotationUpdate()
    {
        if (_groundChecker.isGrounded)
        {
            int temp = -1;
            float rotation = rotateSpeed * moveSpeed * Time.fixedDeltaTime;
            if (_LUChecker.isGrounded == true && direction < 0)
            {
                transform.Rotate(new Vector3(0, 0, -rotateSpeed));
                temp = 2;
            }
            else if (_RUChecker.isGrounded == true && direction > 0)
            {
                transform.Rotate(new Vector3(0, 0, rotateSpeed));
                temp = 3;
            }
            else if (_LUChecker.isGrounded == true && direction > 0)
            {
                transform.Rotate(new Vector3(0, 0, rotateSpeed));
                temp = 4;
            }
            else if (_RUChecker.isGrounded == true && direction < 0)
            {
                transform.Rotate(new Vector3(0, 0, -rotateSpeed));
                temp = 5;
            }
            else if (_LLChecker.isGrounded == false && direction < 0)
            {
                transform.Rotate(new Vector3(0, 0, rotation));
                temp = 0;
            }
            else if (_RLChecker.isGrounded == false && direction > 0)
            {
                transform.Rotate(new Vector3(0, 0, -rotateSpeed));
                temp = 1;
            }
            // if (temp != -1)
            //     Debug.Log(temp);
        }
        else 
        {
            // if (Mathf.Abs(transform.rotation.z - 0) < 10)
            // {
            //     float force = Mathf.Abs(transform.rotation.z) * (-direction);
            //     transform.Rotate(new Vector3(0, 0, force));
            // }
        }
    }

    private void GravityUpdate()
    {
        if (_groundChecker.isGrounded)
        {
            _rigidbody2D.gravityScale = 0;
        }
        else {
            _rigidbody2D.gravityScale = 1;
        }
    }

    private void DeadUpdate()
    {
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // get damege
        if (other.tag == "Bullet")
        {
            hp -= 1;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "platform" && _groundChecker.isGrounded == false)
        {
            transform.rotation = Quaternion.Euler(0,0,0);
        }
    }
    #endregion

    #region Unity Methods
    void Start()
    {
        hp = hpMax;
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        MoveUpdate();
        // HoldWallForceUpdate();
        RotationUpdate();
        GravityUpdate();
        DeadUpdate();
    }
    #endregion
}
