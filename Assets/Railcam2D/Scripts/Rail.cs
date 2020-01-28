using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Railcam2D
{
    [DisallowMultipleComponent]
    public class Rail : MonoBehaviour
    {
        #region Public Variables

        /// <summary>The target the camera follows while on a rail.</summary>
        public Transform Target;

        /// <summary>The axis used by the camera to follow the target.</summary>
        public RailOrientation Orientation;

        /// <summary>Camera FX. The camera leads with reduced velocity between the first two nodes.</summary>
        [Range(0.0f, 1.0f)]
        public float LeadIn;

        /// <summary>Camera FX. The camera trails with reduced velocity between the final two nodes.</summary>
        [Range(0.0f, 1.0f)]
        public float TrailOut;

        /// <summary>The list of RailNode objects that define the rail's path.</summary>
        public List<RailNode> Nodes = new List<RailNode>();

        #endregion

        #region Private Variables

        private Railcam2DCore _core;

        #endregion

        #region MonoBehaviour

        private void Awake()
        {
            if (_core == null)
            {
                if (transform.parent == null)
                    Debug.LogError("The game object " + gameObject.name + " has no parent. An object with a Rail component requires a parent with a Railcam2DCore component");
                else
                {
                    _core = GetComponentInParent<Railcam2DCore>();
                    if (_core == null)
                        Debug.LogError("Railcam2DCore component not found on the parent of the game object " + gameObject.name);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>Returns a Vector2 representing the position on the rail the camera should be on the next frame. Used by Railcam 2D Core.</summary>
        public Vector2 GetIntendedPositionOnRail()
        {
            var targetPosition = Vector2.zero;

            if ((Nodes.Count < 2) || (_core == null || (_core.Target == null && Target == null)))
                return targetPosition;

            if (Target == null)
                targetPosition = _core.Target.position;
            else
                targetPosition = Target.position;

            targetPosition = ClampToRailLimits(targetPosition);

            return GetIntendedPositionFromTargetPosition(targetPosition);
        }

        #endregion
        
        #region Private Methods

        /// <summary>Returns Vector2 clamped to the limits of the rail.</summary>
        /// <param name="targetPosition">Position value to clamp.</param>
        private Vector2 ClampToRailLimits(Vector2 targetPosition)
        {
            if (Orientation == RailOrientation.Horizontal)
                targetPosition.x = Mathf.Clamp(targetPosition.x, Nodes[0].Position.x, Nodes[Nodes.Count - 1].Position.x);
            else if(Orientation == RailOrientation.Vertical)
                targetPosition.y = Mathf.Clamp(targetPosition.y, Nodes[0].Position.y, Nodes[Nodes.Count - 1].Position.y);

            return targetPosition;
        }

        /// <summary>Returns camera position calculated from an adjusted target position.</summary>
        /// <param name="targetPosition">Adjusted Target position.</param>
        private Vector2 GetIntendedPositionFromTargetPosition(Vector2 targetPosition)
        {
            // Sets preNode as the closest node below targetPosition, and postNode as preNode + 1
            RailNode preNode = Nodes[0];
            RailNode postNode = Nodes[1];
            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                if((Orientation == RailOrientation.Horizontal && targetPosition.x - Nodes[i].Position.x < 0) ||
                    (Orientation == RailOrientation.Vertical && targetPosition.y - Nodes[i].Position.y < 0))
                        break;
                preNode = Nodes[i];
                postNode = Nodes[i + 1];
            }

            // The true interpolation value
            float targetNodeInterpolation = GetTargetNodeInterpolation(targetPosition, preNode.Position, postNode.Position);

            // Lead-In Effect
            if (LeadIn != 0.0f && preNode == Nodes[0])
                targetNodeInterpolation = ApplyLeadInFX(targetNodeInterpolation);

            // Trail-Out Effect
            if (TrailOut != 0.0f && postNode == Nodes[Nodes.Count - 1])
                targetNodeInterpolation = ApplyTrailOutFX(targetNodeInterpolation);

            // Threshold Effect
            targetNodeInterpolation = ApplyThresholdFX(targetNodeInterpolation, preNode);

            return Vector2.Lerp(preNode.Position, postNode.Position, targetNodeInterpolation);
        }

        /// <summary>Returns the interpolation value of targetPosition between preNodePosition and postNodePosition.</summary>
        /// <param name="targetPosition">Position to find the interpolation value.</param>
        /// <param name="preNodePosition">Position of closest Node before the target.</param>
        /// <param name="postNodePosition">Position of closest Node after the target.</param>
        private float GetTargetNodeInterpolation(Vector2 targetPosition, Vector2 preNodePosition, Vector2 postNodePosition)
        {
            float targetVector = 0f;
            float preNodeVector = 0f;
            float postNodeVector = 0f;

            if(Orientation == RailOrientation.Horizontal)
            {
                targetVector = targetPosition.x;
                preNodeVector = preNodePosition.x;
                postNodeVector = postNodePosition.x;
            }
            else if (Orientation == RailOrientation.Vertical)
            {
                targetVector = targetPosition.y;
                preNodeVector = preNodePosition.y;
                postNodeVector = postNodePosition.y;
            }

            return Mathf.InverseLerp(preNodeVector, postNodeVector, targetVector);
        }

        /// <summary>Returns an adjusted interpolation value with FX Lead-In applied.</summary>
        /// <param name="nodeInterpolationValue">Initial interpolation value.</param>
        private float ApplyLeadInFX(float nodeInterpolationValue)
        {
            return Mathf.Clamp01((nodeInterpolationValue * (1 - LeadIn)) + LeadIn);
        }

        /// <summary>Returns an adjusted interpolation value with FX Trail-Out applied.</summary>
        /// <param name="nodeInterpolationValue">Initial interpolation value.</param>
        private float ApplyTrailOutFX(float nodeInterpolationValue)
        {
            return Mathf.Clamp01(nodeInterpolationValue * (1 - TrailOut));
        }

        /// <summary>Returns an adjusted interpolation value with FX Threshold applied.</summary>
        /// <param name="nodeInterpolationValue">Initial interpolation value.</param>
        /// <param name="preNode">The closest node before the target.</param>
        private float ApplyThresholdFX(float nodeInterpolationValue, RailNode preNode)
        {
            float thresholdFX = preNode.Threshold;

            if (thresholdFX == 0f)
                return nodeInterpolationValue;
            else if (thresholdFX == 1f)
                return 1f;

            if (preNode.InvertThreshold)
                return (nodeInterpolationValue) / (1f - preNode.Threshold);

            return (nodeInterpolationValue - preNode.Threshold) / (1f - preNode.Threshold);
        }

        #endregion

        #if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = EditorPreferences.GetColor32(EditorPreferences.RailColorKey, EditorPreferences.RailColorValue);
            var handleSize = HandleUtility.GetHandleSize(Vector3.zero) * 0.2f;
            var nodeHandleSize = (transform.parent == null || GetComponentInParent<RailManager>() == null) ? handleSize * 0.25f : handleSize;
            var nodeGizmoSize = new Vector2(nodeHandleSize, nodeHandleSize);

            for (int i = 0; i < Nodes.Count; i++)
            {
                // Nodes
                Gizmos.DrawWireCube(Nodes[i].Position, nodeGizmoSize);
            }

            DrawRailPath();
        }

        /// <summary>Draws a line that represents a rail's path</summary>
        /// <param name="rail">The rail whose path to draw</param>
        private void DrawRailPath()
        {

            if (LeadIn > 0f || TrailOut > 0f)
            {
                // Calculate end positions of Rail that camera can reach
                var trailOutMidPoint = Nodes[Nodes.Count - 1].Position;
                var leadInMidPoint = Nodes[0].Position;
                if (TrailOut > 0f)
                    trailOutMidPoint = Vector2.Lerp(Nodes[Nodes.Count - 2].Position, Nodes[Nodes.Count - 1].Position, 1f - TrailOut);
                if (LeadIn > 0f)
                {
                    if (Nodes.Count == 2)
                        leadInMidPoint = Vector2.Lerp(Nodes[0].Position, trailOutMidPoint, LeadIn);
                    else
                        leadInMidPoint = Vector2.Lerp(Nodes[0].Position, Nodes[1].Position, LeadIn);
                }

                // Create list of positions including new ends of Rail
                var newPositions = new List<Vector2>();
                newPositions.Add(leadInMidPoint);
                for (int i = 1; i < Nodes.Count - 1; i++)
                    newPositions.Add(Nodes[i].Position);
                newPositions.Add(trailOutMidPoint);

                // Draw Rail
                for (int i = 0; i < newPositions.Count - 1; i++)
                    Gizmos.DrawLine(newPositions[i], newPositions[i + 1]);

                // Draw LeadIn and TrailOut out of bounds parts of Rail
                Gizmos.color = EditorPreferences.GetColor32(EditorPreferences.RailFXColorKey, EditorPreferences.RailFXColorValue);
                if (LeadIn > 0f)
                    Gizmos.DrawLine(Nodes[0].Position, leadInMidPoint);
                if (TrailOut > 0f)
                    Gizmos.DrawLine(trailOutMidPoint, Nodes[Nodes.Count - 1].Position);
            }
            else
                for (int i = 0; i < Nodes.Count - 1; i++)
                    Gizmos.DrawLine(Nodes[i].Position, Nodes[i + 1].Position);
        }

        #endif
    }
}
