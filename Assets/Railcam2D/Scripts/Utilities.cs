using System.Collections.Generic;
using UnityEngine;

namespace Railcam2D
{
    public static class Utilities
    {
        #region Public Methods

        /// <summary>Returns a list of Rail components from the children of the specified parent object.</summary>
        /// <param name="parentObject">The parent object.</param>
        /// <param name="includeRailsWithLessThanTwoNodes">Include Rail components with less than two nodes.</param>
        public static List<Rail> GetRailsFromChildren(GameObject parentObject, bool includeRailsWithLessThanTwoNodes)
        {
            Rail[] childrenObjects = parentObject.GetComponentsInChildren<Rail>();
            var rails = new List<Rail>();
            foreach (var rail in childrenObjects)
            {
                if (rail.Nodes.Count < 2)
                {
                    if (includeRailsWithLessThanTwoNodes)
                        rails.Add(rail);
                    else
                        Debug.LogWarning("The rail object " + rail.gameObject.name + " has fewer than 2 nodes and is therefore unable to be used by Railcam2DCore");
                }
                else
                    rails.Add(rail);
            }
            return rails;
        }

        /// <summary>Returns a list of Trigger components from the children of the specified parent object.</summary>
        /// <param name="parentObject">The parent object.</param>
        /// <param name="includeGenericTriggers">Include Trigger components with a TriggerEvent value of Generic.</param>
        public static List<Trigger> GetTriggersFromChildren(GameObject parentObject, bool includeGenericTriggers)
        {
            Trigger[] childrenObjects = parentObject.GetComponentsInChildren<Trigger>();
            var triggers = new List<Trigger>();
            foreach (var trigger in childrenObjects)
            {
                if (trigger.Event == TriggerEvent.Generic)
                {
                    if (includeGenericTriggers)
                        triggers.Add(trigger);
                }
                else
                    triggers.Add(trigger);
            }

            return triggers;
        }

        /// <summary>Returns a Vector2 representing screen size of a specified camera in world units.</summary>
        /// <param name="camera">The camera to get the screen size of.</param>
        /// <param name="perspectiveDepth">The distance ahead of the camera to measure screen size at (if perspective camera)</param>
        /// <returns></returns>
        public static Vector2 GetScreenSizeInWorldUnits(Camera camera, float perspectiveDepth = 0f)
        {
            var height = 0f;

            // Height
            if (!camera.orthographic)
            {
                if (perspectiveDepth == 0f)
                    return Vector2.zero;

                height = Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f) * perspectiveDepth * 2f;
            }
            else
                height = camera.orthographicSize * 2f;

            // Width
            var width = height * camera.aspect;

            return new Vector2(width, height);
        }

        /// <summary>Returns true if a given rectangle contains a point.</summary>
        /// <param name="point">The point to in question.</param>
        /// <param name="rectMidpoint">Midpoint of the given rectangle.</param>
        /// <param name="rectWidth">Width of the given rectangle.</param>
        /// <param name="rectHeight">Height of the given rectangle.</param>
        public static bool RectangleContains(Vector2 point, Vector2 rectMidpoint, float rectWidth, float rectHeight)
        {
            if(Mathf.Abs(rectMidpoint.x - point.x) <= rectWidth * 0.5f
                && Mathf.Abs(rectMidpoint.y - point.y) <= rectHeight * 0.5f)
                return true;
            return false;
        }

        /// <summary>Returns true if a given circle contains a point.</summary>
        /// <param name="point">The point in question.</param>
        /// <param name="circleMidpoint">Midpoint of the given circle.</param>
        /// <param name="circleRadius">Radius of the given circle.</param>
        public static bool CircleContains(Vector2 point, Vector2 circleMidpoint, float circleRadius)
        {
            if((circleMidpoint - point).sqrMagnitude <= circleRadius * circleRadius)
                return true;
            return false;
        }

        /// <summary>Returns a value that is exponentially decayed.</summary>
        /// <param name="value">The value to decay.</param>
        /// <param name="time">The time passed since decay began.</param>
        /// <param name="rate">The rate of decay.</param>
        public static float ExponentialDecay(float value, float time, float rate)
        {
            return value * Mathf.Exp(-time * rate);
        }

        #if UNITY_EDITOR

        // Custom Wire Disc Gizmo Method
        public static void GizmosDrawWireDisc(Vector3 position, float radius)
        {
            // vertex count proportional to circumference
            int vertexCount = (int)(radius * 2 * Mathf.PI);
            if (vertexCount < 32) vertexCount = 32;

            // vertex position
            float x, y;

            // starting angle
            float angle = 0f;

            // save previous vertex to draw side between this and current vertex
            var previousVertex = Vector3.zero;

            // calculate vertices and draw faces between them
            for (int i = 0; i <= (vertexCount); i++)
            {
                x = (Mathf.Sin(Mathf.Deg2Rad * angle) * radius) + position.x;
                y = (Mathf.Cos(Mathf.Deg2Rad * angle) * radius) + position.y;

                var currentVertex = new Vector3(x, y, position.z);

                // Draw side
                if (i > 0)
                    Gizmos.DrawLine(previousVertex, currentVertex);

                previousVertex = currentVertex;

                angle += (360f / vertexCount);
            }
        }

        #endif

        #endregion
    }
}
