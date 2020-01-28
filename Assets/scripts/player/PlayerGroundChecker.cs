using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundChecker : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isGrounded = true;

    #region Trigger evnets
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "platform")
        {
            isGrounded = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "platform")
        {
            isGrounded = false;
        }
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        
    }
}
