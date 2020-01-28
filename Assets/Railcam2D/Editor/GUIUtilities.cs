using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Railcam2D
{
    public static class GUIUtilities
    {
        #region Misc GUI
        // Used by RemoveChildObject
        private static EditorApplication.CallbackFunction callbackRemoveObject = new EditorApplication.CallbackFunction(Remove);
        private static GameObject childObjectToDelete;

        private static void Remove()
        {
            Undo.DestroyObjectImmediate(childObjectToDelete);
            EditorApplication.delayCall -= callbackRemoveObject;
        }

        /// <summary>Removes an object and allows the process to be undone</summary>
        /// <param name="objectToRemove">The object to remove</param>
        public static void RemoveChildObject(GameObject objectToRemove)
        {
            childObjectToDelete = objectToRemove;
            EditorApplication.delayCall += callbackRemoveObject;
            GUIUtility.ExitGUI();
        }

        /// <summary>Returns the lowest available index to be appended to a new rail or trigger object's name</summary>
        /// <param name="childNames">All current rail or trigger names</param>
        /// <param name="preText">The text before the index, either "Rail " or "Trigger "</param>
        public static int GetUniqueChildObjectIndex(List<string> childNames, string preText)
        {
            // Limits the loop to 1000 iterations to prevent excessive load
            if (childNames.Count > 0 && childNames.Count <= 1000)
            {
                for (int i = 0; i >= 0;)
                {
                    string searchQuery = preText + i.ToString();
                    for (int j = 0; j <= childNames.Count - 1; j++)
                    {
                        if (Equals(childNames[j], searchQuery))
                        {
                            i++;
                            break;
                        }
                        else
                            if (j == childNames.Count - 1)
                            return i;
                    }
                }
            }

            // If more than 1000 names in list, just return an index of 0
            return childNames.Count;
        }

        /// <summary>Displays a HelpBox on a Rail or Trigger component</summary>
        /// <param name="component">The component to display the HelpBox on</param>
        /// <param name="text">The text to display if the component has a parent object with a Railcam2D component</param>
        public static void ComponentHelpBox(Component component, string text)
        {
            string message = "";
            MessageType messageType = MessageType.None;

            if (component.transform.parent == null)
            {
                message = "This component requires a parent object with a Railcam2D component attached.";
                messageType = MessageType.Error;
            }
            else if (component.transform.parent.GetComponent<Railcam2DCore>() == null)
            {
                message = "Railcam2D component not found on the parent object.";
                messageType = MessageType.Error;
            }
            else
            {
                message = text;
                messageType = MessageType.Info;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(message, messageType);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        /// <summary>Creates a header box label</summary>
        /// <param name="text">The header text</param>
        public static void EditorHeader(string text)
        {
            var style = new GUIStyle("Label");
            style.fontSize = 18;
            style.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(text, style);
        }

        /// <summary>Draws a Label at the specified position with the specified text</summary>
        /// <param name="text">The text to draw</param>
        /// <param name="position">The position of the Label</param>
        /// <param name="color">The color of the text</param>
        public static void DrawTextAtPosition(string text, Vector2 position, Color color, bool backgroundVisible)
        {
            GUIStyle style = new GUIStyle("Label");
            style.normal.textColor = color;

            if(backgroundVisible)
            {
                var background = new Texture2D(1, 1);
                background.SetPixels(new Color[] { new Color32(0, 0, 0, 64) });
                style.normal.background = background;
            }

            int fontSize = 10;
            style.fontSize = fontSize;
            Handles.Label(position, text, style);
        }

        /// <summary>Returns the position of the scene view pivot rounded to an integer</summary>
        public static Vector2 GetRoundedScenePivot()
        {
            var pivot = Vector2.zero;

            if(SceneView.lastActiveSceneView != null)
                pivot = SceneView.lastActiveSceneView.pivot;

            var roundX = Mathf.Round(pivot.x);
            var roundY = Mathf.Round(pivot.y);
            return new Vector2(roundX, roundY);
        }

        #endregion

        #region Rail GUI
        /// <summary>The primary method for drawing a Rail component on the scene view</summary>
        /// <param name="rail">The rail to draw</param>
        /// <param name="handleSize">Handle size</param>
        /// <param name="snapSize">Snap Size</param>
        /// <param name="snapActive">Indicates whether positioning should snap to a grid</param>
        /// <param name="nodeRemovalActive">Indicates whether nodes are able to be removed</param>
        /// <param name="drawEditorHandles">Indicates whether interactive handles should be drawn</param>
        public static void DrawRailOnScene(Rail rail, float handleSize, Vector3 snapSize, bool snapActive = false, bool nodeRemovalActive = false, bool drawEditorHandles = false)
        {
            Handles.color = EditorPreferences.GetColor32(EditorPreferences.RailColorKey, EditorPreferences.RailColorValue);

            var nodes = rail.Nodes;

            // Editor Handles
            if (drawEditorHandles)
            {
                if (nodeRemovalActive && nodes.Count > 2)
                {
                    DrawRailNodeRemoveButtons(rail, handleSize);
                    Handles.color = EditorPreferences.GetColor32(EditorPreferences.RailColorKey, EditorPreferences.RailColorValue);
                }
                else
                {
                    DrawRailNodeInsertButtons(rail, handleSize);
                    DrawRailNodeAddButton(rail, snapActive, handleSize, snapSize.x);
                    DrawRailNodeInsertAtStartButton(rail, snapActive, handleSize, snapSize.x);
                    DrawRailNodeFreeMoveHandles(rail, handleSize, snapSize);
                }
            }


            // Node Index Labels
            Vector2 nodeIndexLabelOffset = new Vector2(-handleSize * 0.2f, handleSize * 1.2f);
            for (int j = 0; j < nodes.Count; j++)
                DrawTextAtPosition(j.ToString(), nodes[j].Position + nodeIndexLabelOffset, Handles.color, false);

            // Rail Name Label
            DrawTextAtPosition(rail.gameObject.name, nodes[0].Position, Handles.color, true);
        }

        /// <summary>Draws a line between each element of a Vector2 List in the specified color</summary>
        /// <param name="points">A Vector2 list of points</param>
        /// <param name="color">The color of the PolyLine</param>
        public static void DrawPolyLineFromNodes(List<Vector2> points, Color color)
        {
            var pointsAsVector3 = new List<Vector3>();
            for (int i = 0; i < points.Count; i++)
                pointsAsVector3.Add(points[i]);
            var pointsArray = pointsAsVector3.ToArray();
            Handles.color = color;
            Handles.DrawPolyLine(pointsArray);
        }

        /// <summary>Draws buttons used for inserting nodes on the rail</summary>
        /// <param name="rail">The rail to insert nodes on</param>
        /// <param name="handleSize">Handle size</param>
        public static void DrawRailNodeInsertButtons(Rail rail, float handleSize)
        {
            var buttonSize = 0.25f * handleSize;
            var nodes = rail.Nodes;

            for (int i = 1; i < nodes.Count; i++)
            {
                var buttonPosition = (nodes[i].Position + nodes[i - 1].Position) * 0.5f;
                if (Handles.Button(buttonPosition, Quaternion.identity, buttonSize, buttonSize, Handles.RectangleHandleCap))
                    InsertRailNode(rail, i, buttonPosition);
            }
        }

        /// <summary>Draws a button used for adding a node to the end of the rail</summary>
        /// <param name="rail">The Rail to add a node to</param>
        /// <param name="snapActive">Indicates whether positioning should snap to a grid</param>
        /// <param name="handleSize">Handle size</param>
        public static void DrawRailNodeAddButton(Rail rail, bool snapActive, float handleSize, float snapSize)
        {
            var buttonSize = 0.25f * handleSize;

            var buttonOffsetVector = handleSize * 1.5f;
            if (snapActive)
            {
                buttonOffsetVector = Mathf.Round(buttonOffsetVector / snapSize) * snapSize;
                if (buttonOffsetVector < snapSize)
                    buttonOffsetVector = snapSize;
            }

            var buttonPosition = rail.Nodes[rail.Nodes.Count - 1].Position + new Vector2(buttonOffsetVector, buttonOffsetVector);
            
            if (Handles.Button(buttonPosition, Quaternion.identity, buttonSize, buttonSize, Handles.RectangleHandleCap))
                AddRailNode(rail, buttonPosition);
        }

        /// <summary>Draws a button used for inserting a node at the start of the rail</summary>
        /// <param name="rail">The rail to insert a node at the start of</param>
        /// <param name="snapActive">Indicates whether positioning should snap to a grid</param>
        /// <param name="handleSize">Handle size</param>
        public static void DrawRailNodeInsertAtStartButton(Rail rail, bool snapActive, float handleSize, float snapSize)
        {
            var buttonSize = 0.25f * handleSize;

            var buttonOffsetVector = handleSize * 1.5f;
            if (snapActive)
            {
                buttonOffsetVector = Mathf.Round(buttonOffsetVector / snapSize) * snapSize;
                if (buttonOffsetVector < snapSize)
                    buttonOffsetVector = snapSize;
            }

            var buttonPosition = rail.Nodes[0].Position - new Vector2(buttonOffsetVector, buttonOffsetVector);

            if (Handles.Button(buttonPosition, Quaternion.identity, buttonSize, buttonSize, Handles.RectangleHandleCap))
                InsertRailNode(rail, 0, buttonPosition);
        }

        /// <summary>Draws Free Move Handles for the specified rail</summary>
        /// <param name="rail">The rail to draw handles for</param>
        /// <param name="handleSize">Handle size</param>
        /// <param name="snapSize">Snap size</param>
        public static void DrawRailNodeFreeMoveHandles(Rail rail, float handleSize, Vector3 snapSize)
        {
            var moveHandleSize = 0.5f * handleSize;
            var nodes = rail.Nodes;
            for(int i = 0; i < nodes.Count; i++)
            {
                Vector2 newNodePosition = Handles.FreeMoveHandle(nodes[i].Position, Quaternion.identity, moveHandleSize, snapSize, Handles.RectangleHandleCap);
                if (nodes[i].Position != newNodePosition)
                {
                    Undo.RecordObject(rail, "Move Node");
                    nodes[i].Position = newNodePosition;
                }
            }
        }

        /// <summary>Draws buttons that remove nodes from the specified Rail</summary>
        /// <param name="rail">The rail to draw handles for</param>
        /// <param name="handleSize">Handle size</param>
        public static void DrawRailNodeRemoveButtons(Rail rail, float handleSize)
        {
            Handles.color = Color.red;
            var buttonSize = 0.5f * handleSize;
            var nodes = rail.Nodes;
            for (int i = 0; i < rail.Nodes.Count; i++)
                if (Handles.Button(nodes[i].Position, Quaternion.identity, buttonSize, buttonSize, Handles.DotHandleCap))
                {
                    RemoveRailNode(rail, i);
                    return;
                }
        }

        /// <summary>Creates a game object with a Rail component as a child of the specified object</summary>
        /// <param name="parentObject">The game object to add a child with a Rail component to</param>
        /// <param name="newIndex">The index to append to the child object's name</param>
        public static void AddRailObjectAsChild(GameObject parentObject, int newIndex)
        {
            var railObj = new GameObject();
            railObj.transform.parent = parentObject.transform;
            railObj.transform.position = parentObject.transform.position;
            railObj.name = "Rail " + newIndex.ToString();

            var railComponent = railObj.AddComponent<Rail>();
            Vector2 firstNodePos = GetRoundedScenePivot();
            railComponent.Nodes.Add(new RailNode() { Position = firstNodePos });
            railComponent.Nodes.Add(new RailNode() { Position = firstNodePos + new Vector2(10f, 0f) });

            Undo.RegisterCreatedObjectUndo(railObj, "Add Rail Object");
        }

        /// <summary>Inserts a rail node on the specified rail</summary>
        /// <param name="rail">The rail to insert a node on</param>
        /// <param name="nodeIndex">The index of the node to insert</param>
        /// <param name="nodePosition">The position of the node to insert</param>
        public static void InsertRailNode(Rail rail, int nodeIndex, Vector2 nodePosition)
        {
            Undo.RecordObject(rail, "Insert Node");
            var nodes = rail.Nodes;
            var node = new RailNode() { Position = nodePosition };
            nodes.Insert(nodeIndex, node);
        }

        /// <summary>Adds a rail node to the end of the specified rail</summary>
        /// <param name="rail">The rail to add a node to</param>
        /// <param name="nodePosition">The position of the node to add</param>
        public static void AddRailNode(Rail rail, Vector2 nodePosition)
        {
            Undo.RecordObject(rail, "Add Node");
            var node = new RailNode() { Position = nodePosition };
            rail.Nodes.Add(node);
        }

        /// <summary>Removes a rail node from the specified rail</summary>
        /// <param name="rail">The rail to remove the node from</param>
        /// <param name="nodeIndex">The index of the node to remove</param>
        public static void RemoveRailNode(Rail rail, int nodeIndex)
        {
            Undo.RecordObject(rail, "Remove Node");
            rail.Nodes.RemoveAt(nodeIndex);
        }
        #endregion

        #region Trigger GUI
        /// <summary>The primary method for drawing a Trigger component on the scene view</summary>
        /// <param name="trigger">The trigger to draw</param>
        /// <param name="handleSize">Handle size</param>
        /// <param name="snapSize">Snap Size</param>
        /// <param name="drawEditorHandles">Indicates whether interactive handles should be drawn</param>
        public static void DrawTriggerOnScene(Trigger trigger, float handleSize, Vector3 snapSize, bool drawEditorHandles = false)
        {
            Handles.color = EditorPreferences.GetColor32(EditorPreferences.TriggerColorKey, EditorPreferences.TriggerColorValue);

            if (drawEditorHandles)
                DrawTriggerFreeMoveHandles(trigger, handleSize, snapSize);
            else
                Handles.DrawWireCube(trigger.Position, new Vector2(handleSize * 0.25f, handleSize * 0.25f));

            // Trigger Shape
            if (trigger.Shape == TriggerShape.Rectangle)
            {
                // Draw Rectangle Editor Handles
                if(drawEditorHandles)
                    DrawDynamicRectangleTrigger(trigger, handleSize, snapSize);
            }
            else if(trigger.Shape == TriggerShape.Circle)
            {
                // Draw Circle Editor Handles
                if(drawEditorHandles)
                    DrawDynamicCircleTrigger(trigger, handleSize, snapSize);
            }

            // Trigger Name Text
            string triggerText = trigger.gameObject.name;

            // Trigger Event Text
            if (drawEditorHandles)
                triggerText += GetTriggerEventText(trigger);

            DrawTextAtPosition(triggerText, trigger.Position, Handles.color, true);
        }

        /// <summary>Returns a string detailing the trigger's events</summary>
        /// <param name="trigger">The trigger whose event to draw</param>
        public static string GetTriggerEventText(Trigger trigger)
        {
            string triggerEventText = "";

            switch (trigger.Event)
            {
                case TriggerEvent.ConnectToSelectedRail:
                    if (trigger.SelectedRail != null)
                        triggerEventText = "\nConnect: " + trigger.SelectedRail.name;
                    break;
                case TriggerEvent.DisconnectFromSelectedRail:
                    if (trigger.SelectedRail != null)
                        triggerEventText = "\nDisconnect: " + trigger.SelectedRail.name;
                    break;
                case TriggerEvent.DisconnectFromAllRails:
                    triggerEventText = "\nDisconnect: ALL";
                    break;
                case TriggerEvent.Generic:
                    triggerEventText = "\nGeneric";
                    break;
                default:
                    break;
            }

            return triggerEventText;
        }

        /// <summary>Draws Free Move Handles for the specified trigger</summary>
        /// <param name="trigger">The trigger to draw handles for</param>
        /// <param name="handleSize">Handle size</param>
        /// <param name="snapSize">Snap size</param>
        public static void DrawTriggerFreeMoveHandles(Trigger trigger, float handleSize, Vector3 snapSize)
        {
            Handles.color = EditorPreferences.GetColor32(EditorPreferences.TriggerColorKey, EditorPreferences.TriggerColorValue);
            var moveHandleSize = 0.5f * handleSize;
            Vector2 newPosition = Handles.FreeMoveHandle(trigger.Position,
                Quaternion.identity,moveHandleSize, snapSize, Handles.RectangleHandleCap);
            if (trigger.Position != newPosition)
            {
                Undo.RecordObject(trigger, "Move Trigger");
                trigger.Position = newPosition;
            }
        }

        /// <summary>Draws a rectangle with interactive handles for size</summary>
        /// <param name="trigger">The trigger to draw as a rectangle</param>
        /// <param name="handleSize">Handle size</param>
        public static void DrawDynamicRectangleTrigger(Trigger trigger, float handleSize, Vector3 snapSize)
        {
            Handles.color = EditorPreferences.GetColor32(EditorPreferences.TriggerColorKey, EditorPreferences.TriggerColorValue);
            Handles.DrawLine(trigger.Position + new Vector2(0.5f * handleSize, 0f),
                trigger.Position + new Vector2(trigger.Size.x * 0.5f, 0f));
            Handles.DrawLine(trigger.Position + new Vector2(0f, 0.5f * handleSize),
                trigger.Position + new Vector2(0f, trigger.Size.y * 0.5f));

            float newSizeX = 2f *
                (Handles.FreeMoveHandle((trigger.Position + new Vector2(trigger.Size.x * 0.5f, 0f)),
                Quaternion.identity, 0.25f * handleSize, snapSize,
                Handles.RectangleHandleCap).x - trigger.Position.x);
            if (trigger.Size.x != newSizeX)
            {
                Undo.RecordObject(trigger, "Change Trigger Size");
                if (newSizeX < 0f)
                    newSizeX = 0f;
                trigger.Size.x = newSizeX;
            }

            float newSizeY = 2f *
                (Handles.FreeMoveHandle((trigger.Position + new Vector2(0f, trigger.Size.y * 0.5f)),
                Quaternion.identity, 0.25f * handleSize, snapSize,
                Handles.RectangleHandleCap).y - trigger.Position.y);
            if (trigger.Size.y != newSizeY)
            {
                Undo.RecordObject(trigger, "Change Trigger Size");
                if (newSizeY < 0f)
                    newSizeY = 0f;
                trigger.Size.y = newSizeY;
            }
        }

        /// <summary>Draws a circle with interactive handles for radius</summary>
        /// <param name="trigger">The trigger to draw as a circle</param>
        /// <param name="handleSize">Handle size</param>
        public static void DrawDynamicCircleTrigger(Trigger trigger, float handleSize, Vector3 snapSize)
        {
            Handles.color = EditorPreferences.GetColor32(EditorPreferences.TriggerColorKey, EditorPreferences.TriggerColorValue);
            Handles.DrawLine(trigger.Position + new Vector2(0.5f * handleSize, 0f),
                trigger.Position + new Vector2(trigger.Radius, 0f));
            float newRadius = Handles.FreeMoveHandle((trigger.Position + new Vector2(trigger.Radius, 0f)),
                Quaternion.identity, 0.25f * handleSize, snapSize,
                Handles.RectangleHandleCap).x - trigger.Position.x;
            if (trigger.Radius != newRadius)
            {
                Undo.RecordObject(trigger, "Change Trigger Radius");
                if (newRadius < 0f)
                    newRadius = 0f;
                trigger.Radius = newRadius;
            }
        }

        /// <summary>Creates a game object with a Trigger component as a child of the specified object</summary>
        /// <param name="parentObject">The game object to add a child with a Trigger component to</param>
        /// <param name="newIndex">The index to append to the child object's name</param>
        public static void AddTriggerObjectAsChild(GameObject parentObject, int newIndex)
        {
            var triggerObj = new GameObject();
            triggerObj.transform.parent = parentObject.transform;
            triggerObj.transform.position = parentObject.transform.position;
            triggerObj.name = "Trigger " + newIndex.ToString();

            var triggerComponent = triggerObj.AddComponent<Trigger>();
            triggerComponent.Position = GetRoundedScenePivot();
            triggerComponent.StartActive = true;
            triggerComponent.ScanInterval = 0.1f;
            triggerComponent.Radius = 5f;
            triggerComponent.Size = new Vector2(10f, 10f);

            Undo.RegisterCreatedObjectUndo(triggerObj, "Add Trigger Object");
        }
        #endregion
    }
}