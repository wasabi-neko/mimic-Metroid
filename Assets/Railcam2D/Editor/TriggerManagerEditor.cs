using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Railcam2D
{
    [CustomEditor(typeof(TriggerManager))]
    public class TriggerManagerEditor : Editor
    {
        #region Private Variables
        // Used for labels and tooltips
        private GUIContent _content;

        // All Trigger components managed by the Trigger Manager
        private List<Trigger> _triggers;

        // All Rails components selectable as a Selected Rail
        private List<Rail> _rails;

        // The index of the trigger currently being edited
        private int _currentTriggerIndex = -1;

        // Selected Rail
        private int _currentSelectedRailDropDownIndex = 0;

        // Forces SceneView.RepaintAll() due to OnSceneGUI Handles of other editors being influenced by this editor
        private bool _repaintScene;
        #endregion

        #region Enable
        public void OnEnable()
        {
            var triggerManager = (TriggerManager)target;
            if (target == null)
                return;

            _triggers = Utilities.GetTriggersFromChildren(triggerManager.gameObject, true);
            _rails = Utilities.GetRailsFromChildren(triggerManager.gameObject, true);
        }
        #endregion

        #region SceneGUI
        public void OnSceneGUI()
        {
            var triggerManager = (TriggerManager)target;
            if (target == null)
                return;

            _triggers = Utilities.GetTriggersFromChildren(triggerManager.gameObject, true);

            var handleSize = HandleUtility.GetHandleSize(Vector3.zero) * 0.2f;
            var snapValue = EditorPrefs.GetFloat(EditorPreferences.SnapKey, EditorPreferences.SnapValue);
            var snapSize = new Vector3(snapValue, snapValue, snapValue);

            // Draw Triggers
            for (int i = 0; i < _triggers.Count; i++)
            {
                var trigger = _triggers[i];
                GUIUtilities.DrawTriggerOnScene(trigger, handleSize, snapSize, true);
            }

            // Sync editor values with scene if trigger is displayed
            if (_currentTriggerIndex != -1)
                Repaint();
        }
        #endregion

        #region InspectorGUI
        public override void OnInspectorGUI()
        {
            var triggerManager = (TriggerManager)target;
            if (target == null)
                return;

            _triggers = Utilities.GetTriggersFromChildren(triggerManager.gameObject, true);
            _rails = Utilities.GetRailsFromChildren(triggerManager.gameObject, true);

            // Prevents index out of range exceptions if the adding of a trigger is undone
            // while its values are displayed in the editor
            if (_currentTriggerIndex >= _triggers.Count)
                _currentTriggerIndex = -1;

            // Occurs when values have been changed that are represented visually on the Scene View
            if (_repaintScene)
            {
                SceneView.RepaintAll();
                _repaintScene = false;
            }

            // Editor Layout
            EditorGUILayout.Space();
            AddNewTriggerButton(triggerManager);
            ListTriggerButtons();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            InitializeMainEditor(triggerManager);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
        #endregion

        #region Private Methods
        /// <summary>Returns an index to display the specified trigger's SelectedRail in a Popup</summary>
        /// <param name="trigger">The trigger whose SelectedRail to display</param>
        private int GetSelectedRailDropDownIndex(Trigger trigger)
        {
            if (_rails.Count > 0 && trigger.SelectedRail != null)
            {
                for (int i = 0; i < _rails.Count; i++)
                {
                    var rail = _rails[i];
                    if (rail == trigger.SelectedRail)
                        return i + 1;
                }
            }
            return 0;
        }

        /// <summary>Creates a Button that adds a child object with a Trigger component to the game object</summary>
        /// <param name="triggerManager">The TriggerManager component selected in the Inspector</param>
        private void AddNewTriggerButton(TriggerManager triggerManager)
        {
            // Disallows triggers being added when Editor in Play Mode
            if (EditorApplication.isPlaying)
                GUI.enabled = false;

            // Button to add a trigger
            _content = new GUIContent("Add New Trigger", "");
            if (GUILayout.Button(_content, GUILayout.Height(24f)))
            {
                // Checks for next available index to append to trigger object name
                var triggerNames = new List<string>();
                for (int i = 0; i < _triggers.Count; i++)
                    triggerNames.Add(_triggers[i].gameObject.name);
                var newTriggerIndex = GUIUtilities.GetUniqueChildObjectIndex(triggerNames, "Trigger ");

                // Adds the trigger object
                GUIUtilities.AddTriggerObjectAsChild(triggerManager.gameObject, newTriggerIndex);
                _repaintScene = true;
            }

            if (EditorApplication.isPlaying)
                GUI.enabled = true;
        }

        /// <summary>Displays a list of Trigger components as a set of Buttons</summary>
        private void ListTriggerButtons()
        {
            for (int i = 0; i < _triggers.Count; i++)
            {
                var trigger = _triggers[i];

                GUILayout.BeginHorizontal();

                // Button to find trigger in Scene View
                _content = new GUIContent("\u003e\u003e", "Find in scene");
                if (GUILayout.Button(_content, GUILayout.Width(30f)))
                    FindInScene(trigger);

                // Button to load trigger values into editor
                _content = new GUIContent(trigger.gameObject.name, "");
                if (GUILayout.Button(_content))
                {
                    _currentTriggerIndex = i;
                    _currentSelectedRailDropDownIndex = GetSelectedRailDropDownIndex(trigger);
                }

                // Button to remove trigger from Scene
                _content = new GUIContent("\u2212", "Remove");
                if (GUILayout.Button(_content, GUILayout.Width(30f)))
                    RemoveTrigger(trigger);

                GUILayout.EndHorizontal();
            }
        }

        /// <summary>Moves the Scene view to the specified Trigger</summary>
        /// <param name="triggerToFind">The Trigger to find in Scene</param>
        private void FindInScene(Trigger triggerToFind)
        {
            if (SceneView.lastActiveSceneView != null)
            {
                float sceneViewHeight = 0;
                float sceneViewPadding = 5f;

                if (triggerToFind.Shape == TriggerShape.Circle)
                {
                    if (triggerToFind.Radius > 0f)
                        sceneViewHeight = triggerToFind.Radius;
                }
                else
                {
                    if (triggerToFind.Size.y > 0f && triggerToFind.Size.x > 0f)
                    {
                        if (triggerToFind.Size.x / SceneView.lastActiveSceneView.camera.aspect > triggerToFind.Size.y)
                            sceneViewHeight = triggerToFind.Size.x / SceneView.lastActiveSceneView.camera.aspect;
                        else
                            sceneViewHeight = triggerToFind.Size.y;
                    }
                }
                if (sceneViewHeight != 0)
                    SceneView.lastActiveSceneView.size = sceneViewHeight * sceneViewPadding;

                SceneView.lastActiveSceneView.pivot = triggerToFind.Position;
            }
        }

        /// <summary>Destroys a game object with the specified Trigger component attached</summary>
        /// <param name="trigger">The Trigger component whose game object to destroy</param>
        private void RemoveTrigger(Trigger trigger)
        {
            if (EditorUtility.DisplayDialog("Remove Trigger?", "Do you really want to remove " + trigger.gameObject.name + " from the scene?", "Remove", "Cancel"))
            {
                _currentTriggerIndex = -1;
                GUIUtilities.RemoveChildObject(trigger.gameObject);
            }
        }

        /// <summary>Sets the currently displayed trigger and text in the main editor</summary>
        /// <param name="triggerManager">The TriggerManager component selected in the Inspector</param>
        private void InitializeMainEditor(TriggerManager triggerManager)
        {
            // Default values
            var heading = "NO TRIGGERS FOUND";
            Trigger currentTrigger = null;

            // If a trigger is supposed to be displayed
            if (_currentTriggerIndex != -1 && _triggers[_currentTriggerIndex] != null)
            {
                currentTrigger = _triggers[_currentTriggerIndex];
                heading = currentTrigger.gameObject.name.ToUpper();
            }

            // If no trigger is displayed, but triggers are available
            else if (_triggers.Count > 0)
                heading = "SELECT TRIGGER TO EDIT";

            DrawMainEditor(currentTrigger, heading);
        }

        /// <summary>Draws the main editor for the TriggerManager</summary>
        /// <param name="trigger">The Trigger component whose values to display</param>
        /// <param name="heading">The string to display in the EditorHeader</param>
        private void DrawMainEditor(Trigger trigger, string heading)
        {
            // Default values used when trigger == null.
            Color currentColor = Color.grey;
            Transform currentTarget = null;
            TriggerEvent currentEvent = TriggerEvent.Generic;
            float currentScanInterval = 0f;
            bool currentStartActive = false;
            TriggerShape currentShape = TriggerShape.Circle;
            Vector2 currentPosition = Vector2.zero;
            float currentRadius = 0f;
            Vector2 currentSize = Vector2.zero;

            if (trigger != null)
            {
                currentTarget = trigger.Target;
                currentEvent = trigger.Event;
                currentScanInterval = trigger.ScanInterval;
                currentStartActive = trigger.StartActive;
                currentShape = trigger.Shape;
                currentPosition = trigger.Position;
                currentRadius = trigger.Radius;
                currentSize = trigger.Size;
            }
            else
                GUI.enabled = false;

            // Header - displays name of trigger being edited
            GUIUtilities.EditorHeader(heading);

            EditorGUILayout.Space();

            // Target
            _content = new GUIContent("Target", "The target the trigger detects. If null, the Railcam 2D target is used by default");
            var newTarget = EditorGUILayout.ObjectField(_content, currentTarget, typeof(Transform), true);
            if (trigger != null)
                if (trigger.Target != (Transform)newTarget)
                {
                    Undo.RecordObject(trigger, "Change Trigger Target");
                    trigger.Target = (Transform)newTarget;
                }

            EditorGUILayout.Space();

            // Event
            _content = new GUIContent("Event", "The event that occurs when the trigger detects its target");
            var newEvent = EditorGUILayout.EnumPopup(_content, currentEvent);
            if (trigger != null)
                if (trigger.Event != (TriggerEvent)newEvent)
                {
                    Undo.RecordObject(trigger, "Change Trigger Event");
                    trigger.Event = (TriggerEvent)newEvent;
                    _repaintScene = true;
                }

            // Selected Rail
            if(trigger != null && (trigger.Event == TriggerEvent.ConnectToSelectedRail || currentEvent == TriggerEvent.DisconnectFromSelectedRail))
                DrawSelectedRailPopup(trigger);

            EditorGUILayout.Space();

            // Scan Interval
            _content = new GUIContent("Scan Interval", "The time in seconds the trigger waits between each scan for the target (reduces load)");
            var newScanInterval = EditorGUILayout.FloatField(_content, currentScanInterval);
            if (trigger != null)
                if (trigger.ScanInterval != newScanInterval)
                {
                    Undo.RecordObject(trigger, "Change Trigger Scan Interval");
                    if (newScanInterval < 0f)
                        newScanInterval = 0f;
                    trigger.ScanInterval = newScanInterval;
                }

            EditorGUILayout.Space();

            // Start Active
            _content = new GUIContent("Start Active", "Determines whether or not the trigger starts the scene with scanning active. Use Activate() and Deactivate() to activate and deactivate the trigger during runtime");
            var newStartActive = EditorGUILayout.Toggle(_content, currentStartActive);
            if (trigger != null)
                if (trigger.StartActive != newStartActive)
                {
                    Undo.RecordObject(trigger, "Change Trigger Start Active");
                    trigger.StartActive = newStartActive;
                }

            EditorGUILayout.Space();

            // Shape
            _content = new GUIContent("Shape", "The shape of the trigger's detection area");
            var newShape = EditorGUILayout.EnumPopup(_content, currentShape);
            if (trigger != null)
                if (trigger.Shape != (TriggerShape)newShape)
                {
                    Undo.RecordObject(trigger, "Change Trigger Shape");
                    trigger.Shape = (TriggerShape)newShape;
                    _repaintScene = true;
                }

            EditorGUILayout.Space();

            // Position
            _content = new GUIContent("Position", "The position of the trigger");
            var newPosition = EditorGUILayout.Vector2Field(_content, currentPosition);
            if (trigger != null)
                if (trigger.Position != newPosition)
                {
                    Undo.RecordObject(trigger, "Move Trigger");
                    trigger.Position = newPosition;
                }

            if (trigger != null)
            {
                if(trigger.Shape == TriggerShape.Circle)
                {
                    // Radius
                    _content = new GUIContent("Radius", "The radius of the trigger when its shape is a circle");
                    var newRadius = EditorGUILayout.FloatField(_content, currentRadius);
                    if (trigger.Radius != newRadius)
                    {
                        Undo.RecordObject(trigger, "Change Trigger Radius");
                        if (newRadius < 0f)
                            newRadius = 0f;
                        trigger.Radius = newRadius;
                        _repaintScene = true;
                    }
                }
                else if (trigger.Shape == TriggerShape.Rectangle)
                {
                    // Size
                    _content = new GUIContent("Size", "The size of the trigger when its shape is a rectangle");
                    var newSize = EditorGUILayout.Vector2Field(_content, currentSize);
                    if (trigger.Size != newSize)
                    {
                        Undo.RecordObject(trigger, "Change Trigger Size");
                        if (newSize.x < 0f)
                            newSize.x = 0f;
                        if (newSize.y < 0f)
                            newSize.y = 0f;
                        trigger.Size = newSize;
                        _repaintScene = true;
                    }
                }
            }
        }

        /// <summary>Draws a Popup representing TriggerManager.SelectedRail</summary>
        /// <param name="trigger">The Trigger component whose Selected Rail to show</param>
        private void DrawSelectedRailPopup(Trigger trigger)
        {
            var dropDownStrings = new List<GUIContent>();
            dropDownStrings.Add(new GUIContent("-"));
            for (int i = 0; i < _rails.Count; i++)
                dropDownStrings.Add(new GUIContent(_rails[i].gameObject.name));

            _content = new GUIContent("Selected Rail", "The rail associated with the trigger's event");
            var newindex = EditorGUILayout.Popup(_content, _currentSelectedRailDropDownIndex, dropDownStrings.ToArray());
            if (_currentSelectedRailDropDownIndex != newindex)
            {
                Undo.RecordObject(trigger, "Change Trigger Selected Rail");
                _currentSelectedRailDropDownIndex = newindex;
                if (newindex != 0)
                    trigger.SelectedRail = _rails[newindex - 1];
                else
                    trigger.SelectedRail = null;
                _repaintScene = true;
            }
        }
        #endregion
    }
}
