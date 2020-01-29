using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    #region Bullet settings
    public float moveSpeed = 10;
    public float selfDestroyDelay = 0.01f;
    private Rigidbody2D _rigidbody2D;
    #endregion
    // Start is called before the first frame update

    private IEnumerator SelfDestroy() 
    {
        yield return new WaitForSeconds(selfDestroyDelay);   // delay the destroy; this action can improve the feel of attacking;
        Destroy(gameObject); 
    }

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.velocity = transform.right * moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag != "Player" && other.tag != "Bullet")
        {
            StartCoroutine("SelfDestroy");  // call the method:"SelfDestroy"
        }
    }
}
