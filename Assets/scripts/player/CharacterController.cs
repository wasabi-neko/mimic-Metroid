using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    #region Variables
    [Header("Character ablilities")]
    public float moveSpeedMax = 5f; // pixel per second
    public float jumpSpeedMax = 5f; // jump force
    public float moveSpeed = 0;  // init at `Start()`
    public float jumpSpeed = 0;  // init at `Start()`
    public int jumpEnergeMax = 10;
    public float atk = 1;
    public float def = 1;
    public int hpMax = 10;
    public int hp = 10;  // init at `Start()`

    [Header("Character grounded checker")]
    public PlayerGroundChecker groundChecker;

    [Header("Character Private Status")]
    private bool _freezedByEvent = false;    // chara freezed by event like UI
    private int _jumpEnerge = 0;
    private Vector3 _movementForce;
    private Rigidbody2D _rigidbody2D;
    #endregion


    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        // initialize
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        hp = hpMax;
        moveSpeed = moveSpeedMax;
        jumpSpeed = jumpSpeedMax;
    }

    // 100HZ, seted in project setting
    private void FixedUpdate() 
    {
        JumpUpdate(Vector2.up * jumpSpeed * Time.fixedDeltaTime);
        MoveUpdate(_movementForce * Time.fixedDeltaTime);
        
    }

    // Update is called once per frame
    void Update()
    {   
        _movementForce = Vector3.zero;
        if (_freezedByEvent == false)
        {
            // Gey Key
            if (InputManager.GetKey("right"))
            {
                _movementForce += new Vector3(moveSpeed,0,0);
            }
            if (InputManager.GetKey("left"))
            {
                _movementForce += new Vector3(-moveSpeed,0,0);
            }
            if (InputManager.GetKey("jump"))
            {
                if (groundChecker.isGrounded)
                {
                    _jumpEnerge = jumpEnergeMax;
                }
            }
            if (!InputManager.GetKey("jump"))
            {
                _jumpEnerge = 0;
            }
        }
    }
    #endregion

    #region Character Methods
    public void Attack()
    {

    }
    public void MoveUpdate(Vector2 inputSpeed)
    {
        // speed > 0 => right
        float oriSpeedY = _rigidbody2D.velocity.y;
        _rigidbody2D.velocity = new Vector2(inputSpeed.x, oriSpeedY);
        Debug.Log(inputSpeed);
        // TODO: replace scaleFliping with animation("look right", "look left")
        // TODO: draw img: "look left"
        if (inputSpeed.x > 0)
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
        else if (inputSpeed.x < 0)
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }
    }

    public void JumpUpdate(Vector2 inputSpeed)
    {
        if (_jumpEnerge > 0)
        {
            _jumpEnerge -= 1;
            _rigidbody2D.AddForce(inputSpeed, ForceMode2D.Force);
            Debug.Log(inputSpeed);
        }
    }
    #endregion
}
