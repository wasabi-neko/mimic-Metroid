using UnityEngine;
using UnityEditor;

namespace Railcam2D
{
    [CustomEditor(typeof(Rail))]
    public class RailEditor : Editor
    {
        #region SceneGUI
        public void OnSceneGUI()
        {
            var currentRail = (Rail)target;
            if (target == null)
                return;

            if (currentRail.Nodes.Count < 2)
                return;

            var handleSize = HandleUtility.GetHandleSize(Vector3.zero) * 0.2f;
            var snapValue = EditorPrefs.GetFloat(EditorPreferences.SnapKey, EditorPreferences.SnapValue);
            var snapSize = new Vector3(snapValue, snapValue, snapValue);

            // Draw editable rail
            if (currentRail.gameObject.transform.parent != null &&
                currentRail.gameObject.GetComponentInParent<RailManager>() != null)
            {
                // Allows node removal
                var nodeRemovalIsActive = false;
                if (Event.current.shift)
                    nodeRemovalIsActive = true;

                // Allows new nodes to be snapped to grid when added
                var snapActive = false;
                if (Event.current.control)
                    snapActive = true;

                GUIUtilities.DrawRailOnScene(currentRail, handleSize, snapSize, snapActive, nodeRemovalIsActive, true);
            }
            // Draw uneditable rail
            else
                GUIUtilities.DrawRailOnScene(currentRail, handleSize, snapSize);
        }
        #endregion

        #region InspectorGUI
        public override void OnInspectorGUI()
        {
            var rail = (Rail)target;
            if (target == null)
                return;

            GUIUtilities.ComponentHelpBox(rail, "Use Rail Manager on the parent object to edit this rail.");
        }
        #endregion
    }
}

