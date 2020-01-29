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
    public float moveForce = 100;
    public float jumpSpeed = 0;  // init at `Start()`
    public int jumpEnergeMax = 10;
    public float atk = 1;
    public float def = 1;
    public int hpMax = 10;
    public int hp = 10;  // init at `Start()`
    public GameObject bulletPrefab;

    [Header("Character chliderns")]
    public PlayerGroundChecker groundChecker;
    public GameObject weapon;
    private Transform _firePoint;

    [Header("Character Private Status")]
    private bool _freezedByEvent = false;    // chara freezed by event like UI
    private int _jumpEnerge = 0;
    private Vector3 _movementForce;
    private bool _attacking = false;
    private Rigidbody2D _rigidbody2D;
    #endregion

    #region Character Methods
    public void AttackUpdate()
    {
        if (_attacking)
        {
            _attacking = false;
            Instantiate(bulletPrefab, _firePoint.position, _firePoint.rotation);
        }
    }
    public void MoveUpdate(Vector2 inputSpeed)
    {
        // speed > 0 => right
        float oriSpeedY = _rigidbody2D.velocity.y;
        Vector2 maxSpeedNow = new Vector2(inputSpeed.x, oriSpeedY);

        if (Mathf.Abs(_rigidbody2D.velocity.x) > Mathf.Abs(maxSpeedNow.x))
        {
            _rigidbody2D.velocity = maxSpeedNow;
        }
        else
        {
            _rigidbody2D.AddForce(inputSpeed * moveForce);
        }
        
        // TODO: replace scaleFliping with animation("look right", "look left")
        // TODO: draw img: "look left"
        if (inputSpeed.x > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);;
        }
        else if (inputSpeed.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    public void JumpUpdate(Vector2 inputSpeed)
    {
        if (_jumpEnerge > 0)
        {
            _jumpEnerge -= 1;
            _rigidbody2D.AddForce(inputSpeed, ForceMode2D.Force);
        }
    }
    #endregion


    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        // initialize
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        _firePoint = weapon.transform;

        hp = hpMax;
        moveSpeed = moveSpeedMax;
        jumpSpeed = jumpSpeedMax;
    }

    // 100HZ, seted in project setting
    private void FixedUpdate() 
    {
        JumpUpdate(Vector2.up * jumpSpeed * Time.fixedDeltaTime);
        MoveUpdate(_movementForce * Time.fixedDeltaTime);
        AttackUpdate();
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
            if (InputManager.GetKeyDown("jump"))
            {
                if (groundChecker.isGrounded)
                {
                    if (_jumpEnerge == 0)
                        _jumpEnerge = jumpEnergeMax;
                }
            }
            if (!InputManager.GetKey("jump"))
            {
                _jumpEnerge = 0;
            }
            if (InputManager.GetKeyDown("attack"))
            {
                _attacking = true;
            }
        }
    }
    #endregion
}
