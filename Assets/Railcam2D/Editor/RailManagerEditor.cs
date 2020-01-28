using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Railcam2D
{
    [CustomEditor(typeof(RailManager))]
    public class RailManagerEditor : Editor
    {
        #region Private Variables
        // Used for labels and tooltips
        private GUIContent _content;

        // All Rail components managed by the Rail Manager
        private List<Rail> _rails;

        // The index of the rail currently being edited
        private int _currentRailIndex = -1;

        // Forces SceneView.RepaintAll() due to SceneGUI of other editors being influenced by this editor
        private bool _repaintScene;
        #endregion

        #region Enable
        public void OnEnable()
        {
            var railManager = (RailManager)target;
            if (target == null)
                return;

            _rails = Utilities.GetRailsFromChildren(railManager.gameObject, true);
        }
        #endregion

        #region SceneGUI
        public void OnSceneGUI()
        {
            var railManager = (RailManager)target;
            if (target == null)
                return;

            _rails = Utilities.GetRailsFromChildren(railManager.gameObject, true);

            var handleSize = HandleUtility.GetHandleSize(Vector3.zero) * 0.2f;
            var snapValue = EditorPrefs.GetFloat(EditorPreferences.SnapKey, EditorPreferences.SnapValue);
            var snapSize = new Vector3(snapValue, snapValue, snapValue);

            // Allows node removal
            var nodeRemovalActive = false;
            if (Event.current.shift)
                nodeRemovalActive = true;

            // Allows new nodes to be snapped to grid when added
            var snapActive = false;
            if (Event.current.control)
                snapActive = true;

            // Draw rails
            for (int i = 0; i < _rails.Count; i++)
            {
                var rail = _rails[i];
                if (rail.Nodes.Count < 2)
                    continue;
                GUIUtilities.DrawRailOnScene(rail, handleSize, snapSize, snapActive, nodeRemovalActive, true);
            }

            // Sync editor values with scene if rail is displayed in editor
            if (_currentRailIndex != -1)
                Repaint();
        }
        #endregion

        #region InspectorGUI
        public override void OnInspectorGUI()
        {
            var railManager = (RailManager)target;
            if (target == null)
                return;

            _rails = Utilities.GetRailsFromChildren(railManager.gameObject, true);

            // Prevents index out of range exceptions if the adding of a rail is undone
            // while its values are displayed in the editor
            if (_currentRailIndex >= _rails.Count)
                _currentRailIndex = -1;

            // Occurs when values have been changed that are represented visually on the Scene View
            if (_repaintScene)
            {
                SceneView.RepaintAll();
                _repaintScene = false;
            }

            // Editor Layout
            EditorGUILayout.Space();
            AddNewRailButton(railManager);
            ListRailButtons();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            InitializeMainEditor(railManager);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
        #endregion

        #region Private Methods
        /// <summary>Creates a Button that adds a child object with a Rail component to the game object</summary>
        /// <param name="railManager">The RailManager component selected in the Inspector</param>
        private void AddNewRailButton(RailManager railManager)
        {
            // Disallows rails being added when Editor in Play Mode
            if (EditorApplication.isPlaying)
                GUI.enabled = false;

            // Button to add a rail
            _content = new GUIContent("Add New Rail", "");
            if (GUILayout.Button(_content, GUILayout.Height(24f)))
            {
                // Checks for next available index to append to rail object name
                var railNames = new List<string>();
                for (int i = 0; i < _rails.Count; i++)
                    railNames.Add(_rails[i].gameObject.name);
                var newRailIndex = GUIUtilities.GetUniqueChildObjectIndex(railNames, "Rail ");

                // Adds the rail object
                GUIUtilities.AddRailObjectAsChild(railManager.gameObject, newRailIndex);
                _repaintScene = true;
            }

            if (EditorApplication.isPlaying)
                GUI.enabled = true;
        }

        /// <summary>Displays a list of Rail components as a set of Buttons</summary>
        private void ListRailButtons()
        {
            for(int i = 0; i < _rails.Count; i++)
            {
                var rail = _rails[i];

                GUILayout.BeginHorizontal();

                // Button to find rail in Scene View
                _content = new GUIContent("\u003e\u003e", "Find in scene");
                if (GUILayout.Button(_content, GUILayout.Width(30f)))
                    FindInScene(rail);

                // Button to load rail values into editor
                _content = new GUIContent(rail.gameObject.name, "");
                if (GUILayout.Button(_content))
                    _currentRailIndex = i;

                // Button to remove rail from Scene
                _content = new GUIContent("\u2212", "Remove");
                if (GUILayout.Button(_content, GUILayout.Width(30f)))
                    RemoveRail(rail);

                GUILayout.EndHorizontal();
            }
        }

        /// <summary>Moves the Scene View to the specified rail</summary>
        /// <param name="rail">The rail to find in Scene</param>
        private void FindInScene(Rail rail)
        {
            if (rail.Nodes.Count > 1 && SceneView.lastActiveSceneView != null)
            {
                float sceneViewHeight = 0f;
                float sceneViewPadding = 1.5f;
                float railDisplacementX = Mathf.Abs(rail.Nodes[0].Position.x - rail.Nodes[rail.Nodes.Count - 1].Position.x);
                float railDisplacementY = Mathf.Abs(rail.Nodes[0].Position.y - rail.Nodes[rail.Nodes.Count - 1].Position.y);
                if (railDisplacementX != 0f || railDisplacementY != 0f)
                {
                    if (railDisplacementX / SceneView.lastActiveSceneView.camera.aspect > railDisplacementY)
                        sceneViewHeight = railDisplacementX / SceneView.lastActiveSceneView.camera.aspect;
                    else
                        sceneViewHeight = railDisplacementY;

                    if (sceneViewHeight != 0)
                        SceneView.lastActiveSceneView.size = sceneViewHeight * sceneViewPadding;
                }
                SceneView.lastActiveSceneView.pivot = (rail.Nodes[0].Position + rail.Nodes[rail.Nodes.Count - 1].Position) * 0.5f;
            }
        }

        /// <summary>Destroys a game object with the specified Rail component attached</summary>
        /// <param name="rail">The Rail component whose game object to destroy</param>
        private void RemoveRail(Rail rail)
        {
            if (EditorUtility.DisplayDialog("Remove Rail?", "Do you really want to remove " + rail.gameObject.name + " from the scene?", "Remove", "Cancel"))
            {
                _currentRailIndex = -1;
                GUIUtilities.RemoveChildObject(rail.gameObject);
            }
        }

        /// <summary>Sets the currently displayed rail and text in the main editor</summary>
        /// <param name="railManager">The RailManager component selected in the Inspector</param>
        private void InitializeMainEditor(RailManager railManager)
        {
            // Default values
            var heading = "NO RAILS FOUND";
            Rail currentRail = null;

            // If a rail is supposed to be displayed
            if(_currentRailIndex != -1 && _rails[_currentRailIndex] != null)
            {
                currentRail = _rails[_currentRailIndex];
                heading = currentRail.gameObject.name.ToUpper();
            }

            // If no rail is displayed, but rails are available
            else if (_rails.Count > 0)
                heading = "SELECT RAIL TO EDIT";

            DrawMainEditor(currentRail, heading);
        }

        /// <summary>Draws the main editor used for editing rail values</summary>
        /// <param name="rail">The Rail component whose values to display</param>
        /// <param name="heading">The string to display in the EditorHeader</param>
        private void DrawMainEditor(Rail rail, string heading)
        {
            // Default values used when rail == null.
            Color currentColor = Color.grey;
            Transform currentTarget = null;
            RailOrientation currentOrientation = RailOrientation.Horizontal;
            float currentLeadIn = 0f;
            float currentTrailOut = 0f;

            if (rail != null)
            {
                currentTarget = rail.Target;
                currentOrientation = rail.Orientation;
                currentLeadIn = rail.LeadIn;
                currentTrailOut = rail.TrailOut;
            }
            else
                GUI.enabled = false;

            // Header - displays name of rail being edited
            GUIUtilities.EditorHeader(heading);

            EditorGUILayout.Space();

            // Target
            _content = new GUIContent("Target", "The target the camera follows while on the rail. If null, the Railcam 2D target is used by default");
            var newTarget = EditorGUILayout.ObjectField(_content, currentTarget, typeof(Transform), true);
            if (rail != null)
                if (rail.Target != (Transform)newTarget)
                {
                    Undo.RecordObject(rail, "Change Rail Target");
                    rail.Target = (Transform)newTarget;
                }

            EditorGUILayout.Space();

            // Orientation
            _content = new GUIContent("Orientation", "The axis used to calculate camera position while on the rail");
            var newOrientation = EditorGUILayout.EnumPopup(_content, currentOrientation);
            if (rail != null)
                if (rail.Orientation != (RailOrientation)newOrientation)
                {
                    Undo.RecordObject(rail, "Change Rail Orientation");
                    rail.Orientation = (RailOrientation)newOrientation;
                }

            EditorGUILayout.Space();

            // Lead-In
            _content = new GUIContent("FX: Lead-In", "The camera leads with reduced velocity between the first two nodes");
            var newLeadIn = EditorGUILayout.Slider(_content, currentLeadIn, 0f, 1f);
            if (rail != null)
                if (rail.LeadIn != newLeadIn)
                {
                    Undo.RecordObject(rail, "Change Rail Lead-In");
                    rail.LeadIn = newLeadIn;
                    _repaintScene = true;
                }

            // Trail-Out
            _content = new GUIContent("FX: Trail-Out", "The camera trails with reduced velocity between the final two nodes");
            var newTrailOut = EditorGUILayout.Slider(_content, currentTrailOut, 0f, 1f);
            if (rail != null)
                if (rail.TrailOut != newTrailOut)
                {
                    Undo.RecordObject(rail, "Change Rail Trail-Out");
                    rail.TrailOut = newTrailOut;
                    _repaintScene = true;
                }

            EditorGUILayout.Space();

            // Rail Nodes
            if(rail != null)
                ListRailNodes(rail);
        }
        
        /// <summary>Displays a list of nodes for the current rail</summary>
        /// <param name="rail">The Rail component whose nodes to display</param>
        private void ListRailNodes(Rail rail)
        {
            List<RailNode> nodes = rail.Nodes;

            // Column Labels
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Node #", GUILayout.Width(60f));
            EditorGUILayout.LabelField("Position", GUILayout.MinWidth(105f));
            _content = new GUIContent("FX: Threshold", "Increased velocity transition effect\n\u2611 to invert");
            EditorGUILayout.LabelField(_content, GUILayout.MinWidth(156f), GUILayout.MaxWidth(156f));
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                EditorGUILayout.BeginHorizontal();

                // Row Label
                EditorGUILayout.LabelField("Node " + i.ToString(), GUILayout.Width(60f));

                // Position
                var newPosition = EditorGUILayout.Vector2Field(GUIContent.none, node.Position, GUILayout.MinWidth(105f));
                if (node.Position != newPosition)
                {
                    Undo.RecordObject(rail, "Move Node");
                    node.Position = newPosition;
                }

                // Disables editing of the final node's Threshold FX as the value produces no effect
                if (i == nodes.Count - 1)
                    GUI.enabled = false;

                var newInvertThreshold = EditorGUILayout.Toggle(GUIContent.none, node.InvertThreshold, GUILayout.Width(12f));
                if (node.InvertThreshold != newInvertThreshold)
                {
                    Undo.RecordObject(rail, "Invert Node Threshold");
                    node.InvertThreshold = newInvertThreshold;
                }

                var newThreshold = EditorGUILayout.Slider(GUIContent.none, node.Threshold, 0f, 1f, GUILayout.Width(50f));
                if (node.Threshold != newThreshold)
                {
                    Undo.RecordObject(rail, "Change Node Threshold");
                    node.Threshold = newThreshold;
                }

                if (i == nodes.Count - 1)
                    GUI.enabled = true;

                // Buttons for shifting, inserting, and removing nodes
                NodeEditorButtons(rail, i);
                EditorGUILayout.EndHorizontal();
            }

            // Button to add node
            _content = new GUIContent("\u002b", "Add node");
            if (GUILayout.Button(_content))
            {
                Vector2 newNodePos;
                if (nodes.Count > 0)
                    newNodePos = nodes[nodes.Count - 1].Position + new Vector2(2f, 2f);
                else
                    newNodePos = GUIUtilities.GetRoundedScenePivot();

                GUIUtilities.AddRailNode(rail, newNodePos);
                _repaintScene = true;
            }
        }

        /// <summary>Creates Buttons for organising a rail's nodes</summary>
        /// <param name="rail">The rail whose nodes are being edited</param>
        /// <param name="i">Index of the node to be edited</param>
        private void NodeEditorButtons(Rail rail, int i)
        {
            var nodes = rail.Nodes;

            // Shift Up
            _content = new GUIContent("\u25b2", "Shift up");
            if (GUILayout.Button(_content, EditorStyles.miniButtonLeft, GUILayout.Width(20f)))
                if (i > 0)
                {
                    Undo.RecordObject(rail, "Shift Node Up");
                    var node = nodes[i];
                    nodes.RemoveAt(i);
                    nodes.Insert(i - 1, node);
                    _repaintScene = true;
                }

            // Shift Down
            _content = new GUIContent("\u25bc", "Shift down");
            if (GUILayout.Button(_content, EditorStyles.miniButtonMid, GUILayout.Width(20f)))
                if (i < nodes.Count - 1)
                {
                    Undo.RecordObject(rail, "Shift Node Down");
                    var node = nodes[i];
                    nodes.RemoveAt(i);
                    nodes.Insert(i + 1, node);
                    _repaintScene = true;
                }

            // Insert
            _content = new GUIContent("\u002b", "Insert");
            if (GUILayout.Button(_content, EditorStyles.miniButtonMid, GUILayout.Width(20f)))
            {
                Vector2 newNodePos;
                if (i == 0)
                    newNodePos = nodes[i].Position - new Vector2(2f, 2f);
                else
                    newNodePos = (nodes[i].Position + nodes[i - 1].Position) * 0.5f;

                GUIUtilities.InsertRailNode(rail, i, newNodePos);
                _repaintScene = true;
            }

            // Remove
            _content = new GUIContent("\u2212", "Remove");
            if (GUILayout.Button(_content, EditorStyles.miniButtonRight, GUILayout.Width(20f)))
                if (nodes.Count > 2)
                {
                    GUIUtilities.RemoveRailNode(rail, i);
                    _repaintScene = true;
                }
        }
        #endregion
    }
}
