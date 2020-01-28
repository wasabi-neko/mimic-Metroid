using System.Collections.Generic;
using UnityEngine;

namespace Railcam2D
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class Railcam2DCore : MonoBehaviour
    {
        #region Public Variables

        /// <summary>Determines whether or not Railcam 2D Core controls camera movement.</summary>
        public bool Active = true;

        /// <summary>The Update method used to move the camera.</summary>
        public UpdateMethod UpdateMethod;

        /// <summary>Determines whether or not the camera starts on a rail.</summary>
        public Rail StartOnRail;

        /// <summary>The target the camera follows.</summary>
        public Transform Target;

        /// <summary>Determines whether or not the camera follows the target along the x-axis.</summary>
        public bool FollowX = true;

        /// <summary>Determines whether or not the camera follows the target along the y-axis.</summary>
        public bool FollowY = true;

        /// <summary>Offsets camera position along the x-axis.</summary>
        [Range(-1.0f, 1.0f)]
        public float OffsetX;

        /// <summary>Offsets camera position along the y-axis.</summary>
        [Range(-1.0f, 1.0f)]
        public float OffsetY;

        /// <summary>Smooths camera movement along the x-axis.</summary>
        public float SmoothX = 1.5f;

        /// <summary>Smooths camera movement along the y-axis.</summary>
        public float SmoothY = 1.5f;

        /// <summary>Keeps IntendedPosition within camera window by moving camera along the x-axis.</summary>
        [Range(-1.0f, 1.0f)]
        public float BoundX = 0.67f;

        /// <summary>Keeps IntendedPosition within camera window by moving camera along the y-axis.</summary>
        [Range(-1.0f, 1.0f)]
        public float BoundY = 0.67f;

        #endregion

        #region Properties

        /// <summary>The Camera moved by Railcam 2D Core.</summary>
        public Camera Camera { get { return _camera; } }
        private Camera _camera;

        /// <summary>The list of Rail components used by Railcam 2D Core.</summary>
        public List<Rail> Rails { get { return _rails; } }
        private List<Rail> _rails;

        /// <summary>The list of Trigger components used by Railcam 2D Core.</summary>
        public List<Trigger> Triggers { get { return _triggers; } }
        private List<Trigger> _triggers;

        /// <summary>The camera's half screen width in world units.</summary>
        public Vector2 ScreenSizeInWorldUnits { get { return _screenSizeInWorldUnits; } }
        private Vector2 _screenSizeInWorldUnits;

        /// <summary>The position the camera moves towards, without applying offset.</summary>
        public Vector2 IntendedPositionWithoutOffset { get { return _intendedPositionWithoutOffset; } }
        private Vector2 _intendedPositionWithoutOffset;

        /// <summary>The position the camera moves towards, with offset applied.</summary>
        public Vector2 IntendedPosition { get { return _intendedPosition; } }
        private Vector2 _intendedPosition;

        /// <summary>Returns true if the camera is on a rail.</summary>
        public bool OnRail { get { return (_currentRail != null); } }

        /// <summary>The Rail component the camera is currently on.</summary>
        public Rail CurrentRail
        {
            get { return _currentRail; }
            set
            {
                for (int i = 0; i < _rails.Count; i++)
                {
                    if (_rails[i] == value)
                    {
                        _currentRail = value;
                        return;
                    }
                }
                Debug.LogError("The Rail you are trying to set is not a child of the Railcam2DCore game object");
            }
        }
        private Rail _currentRail;

        /// <summary>The Trigger component the camera is currently within.</summary>
        public Trigger CurrentTrigger { get { return _currentTrigger; } }
        private Trigger _currentTrigger;

        #endregion

        #region Private Variables

        private float _cameraPositionZ;

        private float _cameraAspect;

        private float _cameraFieldOfView;
        
        private bool _resetSmoothing;

        private bool _staticPosition;

        private Vector2 _smoothedPosition;

        private Vector2 _boundPosition;

        // allows SmoothX and SmoothY values to resemble seconds
        private const float _smoothingConstant = 0.2f;

        #endregion

        #region MonoBehaviour
        private void Awake()
        {
            // Set camera
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
                if (_camera == null)
                    _camera = gameObject.AddComponent<Camera>();
            }

            SetRails();

            SetTriggers();

            if (StartOnRail != null)
                CurrentRail = StartOnRail;

            SetScreenSize();

            // Set camera values to detect changes in screen size
            _cameraPositionZ = transform.position.z;
            _cameraAspect = _camera.aspect;
            _cameraFieldOfView = _camera.fieldOfView;

            // Sets camera at OffsetIntendedPosition for first frame
            if (Active)
                _resetSmoothing = true;

            _intendedPositionWithoutOffset = transform.position;
            _intendedPosition = transform.position;
            _smoothedPosition = transform.position;
        }

        private void FixedUpdate()
        {
            if (UpdateMethod == UpdateMethod.FixedUpdate && Active)
                UpdatePosition();
        }

        private void LateUpdate()
        {
            if (UpdateMethod == UpdateMethod.LateUpdate && Active)
                UpdatePosition();
        }
        #endregion

        #region Public Methods
        /// <summary>Prevents smoothing on the next frame, therefore the camera instantly moves to its IntendedPosition.</summary>
        public void ResetSmoothing()
        {
            if (!_resetSmoothing)
                _resetSmoothing = true;
        }

        /// <summary>Moves the camera to a static specified position.</summary>
        /// <param name="position">The position to move the camera.</param>
        /// <param name="applyOffset">Apply an offset to the specified position.</param>
        /// <param name="applySmoothing">Apply smoothing to the camera movement.</param>
        public void StaticPosition(Vector2 position, bool applyOffset = true, bool applySmoothing = true)
        {
            if (!_staticPosition)
                _staticPosition = true;

            _intendedPositionWithoutOffset = position;
            _intendedPosition = (applyOffset) ? GetIntendedPosition() : _intendedPositionWithoutOffset;

            if (!applySmoothing)
                ResetSmoothing();
        }

        /// <summary>Releases the camera from a static position to a dynamic position that is determined by target position.</summary>
        /// <param name="applySmoothing">Apply smoothing to the camera movement.</param>
        public void DynamicPosition(bool applySmoothing = true)
        {
            if (_staticPosition)
            {
                if (!applySmoothing)
                    ResetSmoothing();
                _staticPosition = false;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>The primary method that moves the camera. This method is called by the selected Update Method.</summary>
        private void UpdatePosition()
        {
            CheckForScreenSizeChange();

            // Check if on a rail
            _currentRail = GetCurrentRail();

            if (!_staticPosition)
            {
                // Return if no available targets
                if (Target == null && (_currentRail == null || _currentRail.Target == null))
                    return;

                // Get intended position without offset
                _intendedPositionWithoutOffset = GetIntendedPositionWithoutOffset();

                // Get intended position with offset
                if (OffsetX != 0f || OffsetY != 0f)
                    _intendedPosition = GetIntendedPosition();
                else
                    _intendedPosition = _intendedPositionWithoutOffset;
            }

            // Smooth
            if (!_resetSmoothing && (SmoothX > 0f || SmoothY > 0f))
                _smoothedPosition = GetSmoothedPosition();
            else
            {
                _smoothedPosition = _intendedPosition;
                if (_resetSmoothing)
                    _resetSmoothing = false;
            }

            // Correct the Smoothed Position with Bounds
            if (BoundX > 0f || BoundY > 0f)
                _smoothedPosition = GetBoundPosition();

            // Set camera position
            SetPosition(_smoothedPosition);
        }

        /// <summary>Sets a list of all Rail components attached to this object's children with a node count higher than 1.</summary>
        private void SetRails()
        {
            _rails = Utilities.GetRailsFromChildren(gameObject, false);
        }

        /// <summary>Sets a list of all Trigger components attached to this object's children except for Generic Triggers.</summary>
        private void SetTriggers()
        {
            _triggers = Utilities.GetTriggersFromChildren(gameObject, false);
        }

        /// <summary>Sets the screen size used for calculating Offset and Bound values</summary>
        private void SetScreenSize()
        {
            Transform currentTarget;

            if (_currentRail != null && _currentRail.Target != null)
                currentTarget = _currentRail.Target;
            else
                currentTarget = Target;

            var zDisplacementToTarget = 0f;

            if (currentTarget != null)
                zDisplacementToTarget = Mathf.Abs(transform.position.z - currentTarget.position.z);

            _screenSizeInWorldUnits = Utilities.GetScreenSizeInWorldUnits(_camera, zDisplacementToTarget);
        }

        /// <summary>Checks if screen size has changed during the last frame</summary>
        private void CheckForScreenSizeChange()
        {
            if (_cameraPositionZ != _camera.transform.position.z || _camera.aspect != _cameraAspect || _camera.fieldOfView != _cameraFieldOfView)
            {
                if(_cameraPositionZ != _camera.transform.position.z)
                    _cameraPositionZ = _camera.transform.position.z;

                if(_cameraAspect != _camera.aspect)
                    _cameraAspect = _camera.aspect;

                if (_cameraFieldOfView != _camera.fieldOfView)
                    _cameraFieldOfView = _camera.fieldOfView;

                SetScreenSize();
            }
        }

        /// <summary>Returns a Rail component depending on which Trigger component detects its target.</summary>
        private Rail GetCurrentRail()
        {
            if (_rails.Count < 1)
                return null;

            // Iterate through _triggers
            for (int i = 0; i < _triggers.Count; i++)
            {
                // If an active trigger detects its target
                if (_triggers[i].Active && _triggers[i].TargetDetected)
                {
                    _currentTrigger = _triggers[i];
                    _currentTrigger.TargetDetected = false;

                    switch (_currentTrigger.Event)
                    {
                        case (TriggerEvent.ConnectToSelectedRail):
                            if (_currentTrigger.SelectedRail != null)
                                if (_currentRail != _currentTrigger.SelectedRail)
                                    return _currentTrigger.SelectedRail;
                            break;
                        case (TriggerEvent.DisconnectFromSelectedRail):
                            if (_currentTrigger.SelectedRail != null)
                                if (_currentRail == _currentTrigger.SelectedRail)
                                    return null;
                            break;
                        case (TriggerEvent.DisconnectFromAllRails):
                            if (_currentRail != null)
                                return null;
                            break;
                        default:
                            return _currentRail;
                    }
                    return _currentRail;
                }
            }
            // If no trigger detects its target
            _currentTrigger = null;
            return _currentRail;
        }

        /// <summary>Returns a Vector2 representing the position the camera should be on the next frame without offset.</summary>
        private Vector2 GetIntendedPositionWithoutOffset()
        {
            if (_currentRail != null)
                return _currentRail.GetIntendedPositionOnRail();
            else
            {
                if (!FollowX && !FollowY)
                    return transform.position;

                var intendedPosWithoutOffset = Target.position;

                if (!FollowX)
                    intendedPosWithoutOffset.x = transform.position.x;
                if (!FollowY)
                    intendedPosWithoutOffset.y = transform.position.y;

                return intendedPosWithoutOffset;
            }
        }

        /// <summary>Returns a Vector2 representing the position the camera should be on the next frame with offset.</summary>
        private Vector2 GetIntendedPosition()
        {
            Vector2 IntendedPos = _intendedPositionWithoutOffset;

            // Apply offset
            if ((FollowX || _currentRail != null || _staticPosition) && OffsetX != 0f)
                IntendedPos.x += OffsetX * _screenSizeInWorldUnits.x * 0.5f;

            if ((FollowY || _currentRail != null || _staticPosition) && OffsetY != 0f)
                IntendedPos.y += OffsetY * _screenSizeInWorldUnits.y * 0.5f;

            return IntendedPos;
        }

        /// <summary>Returns a Vector2 representing the position the camera should be on the next frame with smoothing.</summary>
        private Vector2 GetSmoothedPosition()
        {
            var smoothedPosition = _intendedPosition;

            float deltaTime;
            if (UpdateMethod == UpdateMethod.FixedUpdate)
                deltaTime = Time.fixedDeltaTime;
            else
                deltaTime = Time.deltaTime;

            Vector2 distanceToIntendedPos = _intendedPosition - _smoothedPosition;

            // Horizontal
            if (SmoothX > 0f)
                smoothedPosition.x = _intendedPosition.x - Utilities.ExponentialDecay(distanceToIntendedPos.x, deltaTime, 1f / (SmoothX * _smoothingConstant));

            // Vertical
            if (SmoothY > 0f)
                smoothedPosition.y = _intendedPosition.y - Utilities.ExponentialDecay(distanceToIntendedPos.y, deltaTime, 1f / (SmoothY * _smoothingConstant));

            return smoothedPosition;
        }

        /// <summary>Returns a Vector2 representing the bound position the camera should be on the next frame.</summary>
        private Vector2 GetBoundPosition()
        {
            var boundPosition = _smoothedPosition;

            if (BoundX > 0f)
            {
                var distanceToIntendedPosX = _intendedPosition.x - boundPosition.x;
                var boundWidth = _screenSizeInWorldUnits.x * 0.5f * BoundX;
                if (Mathf.Abs(distanceToIntendedPosX) > boundWidth)
                    boundPosition.x = (distanceToIntendedPosX > 0) ? _intendedPosition.x - boundWidth : _intendedPosition.x + boundWidth;
            }

            if (BoundY > 0f)
            {
                var distanceToIntendedPosY = _intendedPosition.y - boundPosition.y;
                var boundHeight = _screenSizeInWorldUnits.y * 0.5f * BoundY;
                if (Mathf.Abs(distanceToIntendedPosY) > boundHeight)
                    boundPosition.y = (distanceToIntendedPosY > 0) ? _intendedPosition.y - boundHeight : _intendedPosition.y + boundHeight;
            }

            return boundPosition;
        }

        /// <summary>Moves camera to the specified position.</summary>
        /// <param name="position">The position to move the camera to.</param>
        private void SetPosition(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
        #endregion

        #if UNITY_EDITOR
        
        private void OnDrawGizmosSelected()
        {
            if (Target == null && (_currentRail == null || _currentRail.Target == null))
                return;

            var camera = GetComponent<Camera>();
            var gizmoZPos = transform.position.z + Mathf.Abs(transform.position.z) * transform.forward.z;

            Gizmos.color = EditorPreferences.GetColor32(EditorPreferences.CameraTargetColorKey, EditorPreferences.CameraTargetColorValue);

            // Current camera position
            var screenSizeY = Utilities.GetScreenSizeInWorldUnits(camera, Mathf.Abs(transform.position.z)).y;
            var cameraPos = new Vector3(transform.position.x, transform.position.y, gizmoZPos);
            Utilities.GizmosDrawWireDisc(cameraPos, screenSizeY * 0.02f);
            Utilities.GizmosDrawWireDisc(cameraPos, screenSizeY * 0.015f);
            Gizmos.DrawRay(cameraPos - new Vector3(screenSizeY * 0.02f, 0f, 0f), new Vector2(screenSizeY * 0.04f, 0f));
            Gizmos.DrawRay(cameraPos - new Vector3(0f, screenSizeY * 0.02f, 0f), new Vector2(0f, screenSizeY * 0.04f));

            // Intended Position
            var intendedPos = new Vector3(_intendedPosition.x, _intendedPosition.y, gizmoZPos);
            Utilities.GizmosDrawWireDisc(intendedPos, screenSizeY * 0.01f);
            Gizmos.DrawRay(intendedPos - new Vector3(screenSizeY * 0.01f, 0f, 0f), new Vector2(screenSizeY * 0.02f, 0f));
            Gizmos.DrawRay(intendedPos - new Vector3(0f, screenSizeY * 0.01f, 0f), new Vector2(0f, screenSizeY * 0.02f));
            Gizmos.DrawLine(cameraPos, intendedPos);

            // Offset
            if (OffsetX > 0f || OffsetY > 0f)
            {
                var intendedPosWithoutOffset = new Vector3(_intendedPositionWithoutOffset.x, _intendedPositionWithoutOffset.y, gizmoZPos);
                Gizmos.DrawWireSphere(intendedPosWithoutOffset, screenSizeY * 0.0125f);
                Gizmos.DrawLine(intendedPosWithoutOffset, intendedPos);
            }

            // Smoothing Bounds
            if(SmoothX > 0f && BoundX > 0f)
            {
                Gizmos.color = EditorPreferences.GetColor32(EditorPreferences.CameraBoundColorKey, EditorPreferences.CameraBoundColorValue);

                if (SmoothX > 0f && BoundY > 0f)
                    Gizmos.DrawWireCube(cameraPos, new Vector3(BoundX * _screenSizeInWorldUnits.x, BoundY * _screenSizeInWorldUnits.y, 0f));
                else
                {
                    var bottomLeft = cameraPos - new Vector3(_screenSizeInWorldUnits.x * 0.5f * BoundX, _screenSizeInWorldUnits.y * 0.5f, 0f);
                    var bottomRight = cameraPos - new Vector3(-_screenSizeInWorldUnits.x * 0.5f * BoundX, _screenSizeInWorldUnits.y * 0.5f, 0f);
                    Gizmos.DrawRay(bottomLeft, new Vector3(0f, _screenSizeInWorldUnits.y * 0.5f * 2f, 0f));
                    Gizmos.DrawRay(bottomRight, new Vector3(0f, _screenSizeInWorldUnits.y * 0.5f * 2f, 0f));
                }
            }
            else if(SmoothY > 0f && BoundY > 0f)
            {
                Gizmos.color = EditorPreferences.GetColor32(EditorPreferences.CameraBoundColorKey, EditorPreferences.CameraBoundColorValue);

                var bottomLeft = cameraPos - new Vector3(_screenSizeInWorldUnits.x * 0.5f, _screenSizeInWorldUnits.y * 0.5f * BoundY, 0f);
                var topLeft = cameraPos - new Vector3(_screenSizeInWorldUnits.x * 0.5f, -_screenSizeInWorldUnits.y * 0.5f * BoundY, 0f);
                Gizmos.DrawRay(bottomLeft, new Vector3(_screenSizeInWorldUnits.x, 0f, 0f));
                Gizmos.DrawRay(topLeft, new Vector3(_screenSizeInWorldUnits.x, 0f, 0f));
            }
        }

        #endif
    }
}