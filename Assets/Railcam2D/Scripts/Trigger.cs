using System.Collections;
using UnityEngine;

namespace Railcam2D
{
    [DisallowMultipleComponent]
    public class Trigger : MonoBehaviour
    {
        #region Public Variables

        /// <summary>The Target the Trigger detects.</summary>
        public Transform Target;

        /// <summary>The Event that occurs when the Target is detected.</summary>
        public TriggerEvent Event;

        /// <summary>The Rail component related to the Trigger's Event.</summary>
        public Rail SelectedRail;

        /// <summary>The time period the Trigger waits for between scans.</summary>
        public float ScanInterval;

        /// <summary>Determines whether or not the Trigger starts the scene with scanning active.</summary>
        public bool StartActive;

        /// <summary>The shape of the Trigger</summary>
        public TriggerShape Shape;

        /// <summary>The position of the Trigger in the Scene.</summary>
        public Vector2 Position;

        /// <summary>Defines the radius of the Trigger when Shape is set to Circle.</summary>
        public float Radius;

        /// <summary>Defines the size of the Trigger when Shape is set to Rectangle.</summary>
        public Vector2 Size;

        #endregion

        #region Properties

        /// <summary>Indicates whether the Trigger is scanning.</summary>
        public bool Active { get { return _active; } }
        private bool _active;

        /// <summary>Indicates whether the Target is within the Trigger area.</summary>
        public bool TargetDetected
        {
            get { return _targetDetected; }
            set
            {
                if (_targetDetected && value == false)
                    _targetDetected = value;
            }
        }
        private bool _targetDetected;

        #endregion

        #region Private Variables

        private Railcam2DCore _core;

        #endregion

        #region MonoBehaviour

        void Awake()
        {
            if (_core == null)
            {
                if (transform.parent == null)
                    Debug.LogError("The game object " + gameObject.name + " has no parent. An object with a Trigger component requires a parent with a Railcam2DCore component");
                else
                {
                    _core = GetComponentInParent<Railcam2DCore>();
                    if (_core == null)
                        Debug.LogError("Railcam2DCore component not found on the parent of the game object " + gameObject.name);
                }
            }

            if (StartActive)
                Activate();
        }

        #endregion

        #region Public Methods

        /// <summary>Starts the Trigger component scanning for its target.</summary>
        public void Activate()
        {
            if (!_active)
            {
                StartCoroutine(ScanForTargetCoroutine());
                _active = true;
            }
        }

        /// <summary>Stops the Trigger component scanning for its target.</summary>
        public void Deactivate()
        {
            if (_active)
            {
                StopCoroutine(ScanForTargetCoroutine());
                _active = false;
            }
        }

        /// <summary>Initiates a scan to see if the target's position is within the Trigger component's area, then sets TargetDetected to true if it is.</summary>
        public void ScanForTarget()
        {
            if (_core == null || (_core.Target == null && Target == null))
                return;

            Vector2 targetPosition = Vector2.zero;

            if (Target == null)
                targetPosition = _core.Target.position;
            else
                targetPosition = Target.position;

            // Set TargetDetected to true if targetPosition is within trigger shape
            if (Shape == TriggerShape.Rectangle)
            {
                if (Utilities.RectangleContains(targetPosition, Position, Size.x, Size.y))
                {
                    if (!TargetDetected)
                        _targetDetected = true;
                }
                else
                {
                    if (TargetDetected)
                        _targetDetected = false;
                }
            }
            else if (Shape == TriggerShape.Circle)
            {
                if (Utilities.CircleContains(targetPosition, Position, Radius))
                {
                    if (!TargetDetected)
                        _targetDetected = true;
                }
                else
                {
                    if (TargetDetected)
                        _targetDetected = false;
                }
            }
        }

        #endregion

        #region Private Coroutines

        /// <summary>Coroutine that initiates a scan for the target.</summary>
        private IEnumerator ScanForTargetCoroutine()
        {
            // WaitForEndOfFrame() allows rendering to finish.
            yield return new WaitForEndOfFrame();
            var waitForScanInterval = new WaitForSeconds(ScanInterval);

            // Random.Range allows target checking for multiple triggers to be
            // spread across different frames, reducing load.
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.05f));
            while (true)
            {
                ScanForTarget();
                yield return waitForScanInterval;
            }
        }

        #endregion

        #if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = EditorPreferences.GetColor32(EditorPreferences.TriggerColorKey, EditorPreferences.TriggerColorValue);

            // Trigger Shape
            if (Shape == TriggerShape.Circle)
                Utilities.GizmosDrawWireDisc(Position, Radius);
            else
                Gizmos.DrawWireCube(Position, Size);
        }

        #endif
    }
}