using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// transform box or poly collider to edge collider
public class ColliderTransInslide : MonoBehaviour
{
    #region inputs
    [Tooltip("choose one of these")]
    public bool useBoxCollider = true;
    public bool usePolygonCollider = false;
    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        List<Vector2> vertexs = new List<Vector2>();
        if (useBoxCollider) 
        {
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            Vector3 maxVertex = collider.bounds.max;
            Vector3 minVertex = collider.bounds.min;

            vertexs.Add(new Vector2(maxVertex.x, minVertex.y));
            vertexs.Add(new Vector2(maxVertex.x, maxVertex.y));
            vertexs.Add(new Vector2(minVertex.x, maxVertex.y));
            vertexs.Add(new Vector2(minVertex.x, minVertex.y));
            Destroy(collider);
        }
        else if (usePolygonCollider)
        {
            PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
            vertexs.AddRange(collider.points);
            Destroy(collider);
        }
        else
        {
            Debug.LogError("the type of Collider did'nt selected");
        }

        EdgeCollider2D newCollider = gameObject.AddComponent<EdgeCollider2D>();
        vertexs.Add(new Vector2(vertexs[0].x, vertexs[0].y));
        newCollider.points = vertexs.ToArray();
    }
}
