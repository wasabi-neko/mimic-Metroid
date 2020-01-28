using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Railcam2D
{
    [CustomEditor(typeof(Railcam2DCore))]
    public class Railcam2DCoreEditor : Editor
    {
        #region Private Variables
        // Used for labels and tooltips
        private GUIContent _content;

        // All Rail components managed by the Rail Manager
        private List<Rail> _rails;

        // All Trigger components managed by the Trigger Manager
        private List<Trigger> _triggers;

        // Used to display the StartOnRail value in a Popup
        private int _currentStartOnRailDropDownIndex = 0;
        #endregion

        #region Enable
        public void OnEnable()
        {        
            var railcam2DCore = (Railcam2DCore)target;
            if (target == null)
                return;

            _rails = Utilities.GetRailsFromChildren(railcam2DCore.gameObject, true);
        }
        #endregion

        #region SceneGUI
        public void OnSceneGUI()
        {
            var railcam2DCore = (Railcam2DCore)target;
            if (target == null)
                return;

            var handleSize = HandleUtility.GetHandleSize(Vector3.zero) * 0.2f;
            var snapValue = EditorPrefs.GetFloat(EditorPreferences.SnapKey, EditorPreferences.SnapValue);
            var snapSize = new Vector3(snapValue, snapValue, snapValue);

            // If no Rail Manager component on game object, draw uneditable rails
            if (railcam2DCore.GetComponent<RailManager>() == null)
            {
                _rails = Utilities.GetRailsFromChildren(railcam2DCore.gameObject, true);

                for (int i = 0; i < _rails.Count; i++)
                {
                    var rail = _rails[i];
                    if (rail.Nodes.Count < 2)
                        continue;

                    GUIUtilities.DrawRailOnScene(rail, handleSize, snapSize);
                }
            }

            // If no Trigger Manager component on game object, draw uneditable triggers
            if (railcam2DCore.GetComponent<TriggerManager>() == null)
            {
                _triggers = Utilities.GetTriggersFromChildren(railcam2DCore.gameObject, true);

                for (int i = 0; i < _triggers.Count; i++)
                {
                    var currentTrigger = _triggers[i];
                    GUIUtilities.DrawTriggerOnScene(currentTrigger, handleSize, snapSize);
                }
            }
        }
        #endregion

        #region InspectorGUI
        public override void OnInspectorGUI()
        {
            var railcam2DCore = (Railcam2DCore)target;
            if (target == null)
                return;

            _rails = Utilities.GetRailsFromChildren(railcam2DCore.gameObject, true);

            _currentStartOnRailDropDownIndex = GetStartOnRailDropDownIndex(railcam2DCore);

            DrawMainEditor(railcam2DCore);
        }
        #endregion

        #region Private Methods
        /// <summary>Returns StartOnRailDropDownIndex based on Railcam2D.StartOnRail</summary>
        /// <param name="railcam2DCore">The Railcam2DCore component selected in the Inspector</param>
        private int GetStartOnRailDropDownIndex(Railcam2DCore railcam2DCore)
        {
            if (_rails.Count > 0 && railcam2DCore.StartOnRail != null)
            {
                for (int i = 0; i < _rails.Count; i++)
                {
                    var rail = _rails[i];
                    if (rail == railcam2DCore.StartOnRail)
                        return i + 1;
                }
            }
            return 0;
        }

        /// <summary>Draws the editor in the Inspector</summary>
        /// <param name="railcam2DCore">The Railcam2DCore component selected in the Inspector</param>
        private void DrawMainEditor(Railcam2DCore railcam2DCore)
        {
            // Used to detect changes to values
            var currentTarget = railcam2DCore.Target;
            var currentUpdateMethod = railcam2DCore.UpdateMethod;
            var currentActive = railcam2DCore.Active;
            var currentFollowX = railcam2DCore.FollowX;
            var currentFollowY = railcam2DCore.FollowY;
            var currentOffsetX = railcam2DCore.OffsetX;
            var currentOffsetY = railcam2DCore.OffsetY;
            var currentSmoothX = railcam2DCore.SmoothX;
            var currentSmoothY = railcam2DCore.SmoothY;
            var currentBoundX = railcam2DCore.BoundX;
            var currentBoundY = railcam2DCore.BoundY;
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Active
            _content = new GUIContent("Active", "Determines whether or not the Railcam 2D component controls camera movement");
            var newActive = EditorGUILayout.Toggle(_content, currentActive);
            if (railcam2DCore.Active != newActive)
            {
                Undo.RecordObject(railcam2DCore, "Change Active");
                railcam2DCore.Active = newActive;
            }

            EditorGUILayout.Space();

            // Update Method
            _content = new GUIContent("Update Method", "The Update method used to move the camera");
            var newUpdateMethod = EditorGUILayout.EnumPopup(_content, currentUpdateMethod);
            if (railcam2DCore.UpdateMethod != (UpdateMethod)newUpdateMethod)
            {
                Undo.RecordObject(railcam2DCore, "Change Update Method");
                railcam2DCore.UpdateMethod = (UpdateMethod)newUpdateMethod;
            }

            EditorGUILayout.Space();

            // Start On Rail
            DrawStartOnRailPopup(railcam2DCore);

            EditorGUILayout.Space();

            // Target
            _content = new GUIContent("Target", "The target the camera's rails and triggers use by default, and the target the camera follows when not on a rail");
            var newTarget = EditorGUILayout.ObjectField(_content, currentTarget, typeof(Transform), true);
            if (railcam2DCore.Target != (Transform)newTarget)
            {
                Undo.RecordObject(railcam2DCore, "Change Railcam2D Target");
                railcam2DCore.Target = (Transform)newTarget;
            }

            EditorGUILayout.Space();

            // Follow Label
            _content = new GUIContent("Follow", "Determines whether or not the camera follows the target along the given axis\n(Values disregarded when on a rail)");
            EditorGUILayout.LabelField(_content);

            // Follow X
            _content = new GUIContent("X", "");
            var newFollowX = EditorGUILayout.Toggle(_content, currentFollowX);
            if (railcam2DCore.FollowX != newFollowX)
            {
                Undo.RecordObject(railcam2DCore, "Change Follow X");
                railcam2DCore.FollowX = newFollowX;
            }

            // Follow Y
            _content = new GUIContent("Y", "");
            var newFollowY = EditorGUILayout.Toggle(_content, currentFollowY);
            if (railcam2DCore.FollowY != newFollowY)
            {
                Undo.RecordObject(railcam2DCore, "Change Follow Y");
                railcam2DCore.FollowY = newFollowY;
            }

            EditorGUILayout.Space();

            // Offset Label
            _content = new GUIContent("Offset", "Offsets camera position along the given axis");
            EditorGUILayout.LabelField(_content);

            // Offset X
            _content = new GUIContent("X", "");
            var newOffsetX = EditorGUILayout.Slider(_content, currentOffsetX, -1f, 1f);
            if (railcam2DCore.OffsetX != newOffsetX)
            {
                Undo.RecordObject(railcam2DCore, "Change Offset X");
                railcam2DCore.OffsetX = newOffsetX;
            }

            // Offset Y
            _content = new GUIContent("Y", "");
            var newOffsetY = EditorGUILayout.Slider(_content, currentOffsetY, -1f, 1f);
            if (railcam2DCore.OffsetY != newOffsetY)
            {
                Undo.RecordObject(railcam2DCore, "Change Offset Y");
                railcam2DCore.OffsetY = newOffsetY;
            }

            EditorGUILayout.Space();

            // Smooth Label
            _content = new GUIContent("Smooth", "Smooths camera movement along the given axis");
            EditorGUILayout.LabelField(_content);

            // Smooth X
            _content = new GUIContent("X", "");
            var newSmoothX = EditorGUILayout.FloatField(_content, currentSmoothX);
            if (railcam2DCore.SmoothX != newSmoothX)
            {
                Undo.RecordObject(railcam2DCore, "Change Smooth X");
                railcam2DCore.SmoothX = newSmoothX;
            }

            // Smooth Y
            _content = new GUIContent("Y", "");
            var newSmoothY = EditorGUILayout.FloatField(_content, currentSmoothY);
            if (railcam2DCore.SmoothY != newSmoothY)
            {
                Undo.RecordObject(railcam2DCore, "Change Smooth Y");
                railcam2DCore.SmoothY = newSmoothY;
            }

            EditorGUILayout.Space();

            // Bound Label
            _content = new GUIContent("Bound", "Keeps IntendedPosition within camera window by moving camera along the given axis");
            EditorGUILayout.LabelField(_content);

            // Bound X
            _content = new GUIContent("X", "");
            var newBoundX = EditorGUILayout.Slider(_content, currentBoundX, 0f, 1f);
            if (railcam2DCore.BoundX != newBoundX)
            {
                Undo.RecordObject(railcam2DCore, "Change Bound X");
                railcam2DCore.BoundX = newBoundX;
            }

            // Bound Y
            _content = new GUIContent("Y", "");
            var newBoundY = EditorGUILayout.Slider(_content, currentBoundY, 0f, 1f);
            if (railcam2DCore.BoundY != newBoundY)
            {
                Undo.RecordObject(railcam2DCore, "Change Bound Y");
                railcam2DCore.BoundY = newBoundY;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Line
            GUILayout.Box("", GUILayout.Height(1f), GUILayout.ExpandWidth(true));

            EditorGUILayout.Space();

            // Editing Buttons
            DrawRailEditingButton(railcam2DCore);
            DrawTriggerEditingButton(railcam2DCore);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        /// <summary>Draws a Popup representing Railcam2D.StartOnRail</summary>
        /// <param name="railcam2DCore">The Railcam2DCore component selected in the Inspector</param>
        private void DrawStartOnRailPopup(Railcam2DCore railcam2DCore)
        {
            // List of content
            var dropDownStrings = new List<GUIContent>();

            // Represents null value
            dropDownStrings.Add(new GUIContent("-"));

            // Add all rail children names to list
            for (int i = 0; i < _rails.Count; i++)
                dropDownStrings.Add(new GUIContent(_rails[i].gameObject.name));

            // Draw Popup
            _content = new GUIContent("Start On Rail", "The rail the camera starts the scene attached to. Set as null (\"-\") to prevent the camera starting on a rail");
            var newindex = EditorGUILayout.Popup(_content, _currentStartOnRailDropDownIndex, dropDownStrings.ToArray());
            if (_currentStartOnRailDropDownIndex != newindex)
            {
                Undo.RecordObject(railcam2DCore, "Change Start On Rail");
                _currentStartOnRailDropDownIndex = newindex;
                if (newindex != 0)
                    railcam2DCore.StartOnRail = _rails[newindex - 1];
                else
                    railcam2DCore.StartOnRail = null;
            }
        }

        /// <summary>A Label preceding the buttons for enabling Rail Editing and Trigger Editing</summary>
        /// <param name="text">The label text</param>
        /// <param name="toolTip">The label tooltip</param>
        private void ManagerLabel(string text, string toolTip)
        {
            var style = new GUIStyle("Label");
            style.alignment = TextAnchor.MiddleLeft;
            _content = new GUIContent(text, toolTip);
            GUILayout.Label(_content, style, GUILayout.Height(24f));
        }

        /// <summary>Draws a button that toggles on-scene rail editing and the adding or removing of a Rail Manager component</summary>
        /// <param name="railcam2DCore">The Railcam2DCore component selected in the Inspector</param>
        private void DrawRailEditingButton(Railcam2DCore railcam2DCore)
        {
            EditorGUILayout.BeginHorizontal();
            ManagerLabel("Rail Editing", "Enable on-scene rail editing and add a Rail Manager component to the game object");

            var oldGUIColor = GUI.color;

            if (railcam2DCore.GetComponent(typeof(RailManager)) == null)
            {
                if (GUILayout.Button("Enable", GUILayout.Height(24f), GUILayout.Width(130f)))
                    Undo.AddComponent<RailManager>(railcam2DCore.gameObject);
            }
            else
            {
                GUI.color = new Color32(154, 181, 217, 255);
                if (GUILayout.Button("Enabled", GUILayout.Height(24f), GUILayout.Width(130f)))
                {
                    Undo.DestroyObjectImmediate(railcam2DCore.GetComponent(typeof(RailManager)));
                    GUIUtility.ExitGUI();
                }
            }

            GUI.color = oldGUIColor;
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>Draws a button that toggles on-scene trigger editing and the adding or removing of a Trigger Manager component</summary>
        /// <param name="railcam2DCore">The Railcam2DCore component selected in the Inspector</param>
        private void DrawTriggerEditingButton(Railcam2DCore railcam2DCore)
        {
            EditorGUILayout.BeginHorizontal();
            ManagerLabel("Trigger Editing", "Enable on-scene trigger editing and add a Trigger Manager component to the game object");

            var oldGUIColor = GUI.color;
            if (railcam2DCore.GetComponent(typeof(TriggerManager)) == null)
            {
                if (GUILayout.Button("Enable", GUILayout.Height(24f), GUILayout.Width(130f)))
                    Undo.AddComponent<TriggerManager>(railcam2DCore.gameObject);
            }
            else
            {
                GUI.color = new Color32(154, 181, 217, 255);
                if (GUILayout.Button("Enabled", GUILayout.Height(24f), GUILayout.Width(130f)))
                {
                    Undo.DestroyObjectImmediate(railcam2DCore.GetComponent(typeof(TriggerManager)));
                    GUIUtility.ExitGUI();
                }
            }

            GUI.color = oldGUIColor;
            EditorGUILayout.EndHorizontal();
        }
        #endregion
    }
}