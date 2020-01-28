using UnityEngine;

namespace Railcam2D
{
    [System.Serializable]
    public class RailNode : System.Object
    {
        #region Public Variables

        /// <summary>The position of the RailNode in the Scene.</summary>
        public Vector2 Position;

        /// <summary>Camera FX. The camera is held at the current node, then moved with increased velocity to reach the next node in sync with the target.</summary>
        [Range(0f, 1f)]
        public float Threshold;

        /// <summary>Camera FX. Invert the Threshold of the node so that the camera moved ahead and then waits for the target to catch up.</summary>
        public bool InvertThreshold;

        #endregion
    }
}