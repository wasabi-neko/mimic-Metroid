using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    #region Bullet settings
    public float moveSpeed = 10;
    private Rigidbody2D _rigidbody2D;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.velocity = transform.right * moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag != "Player")
        {
            Destroy(gameObject);    // Destroy itself
        }
    }
}
