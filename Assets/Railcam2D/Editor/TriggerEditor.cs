using UnityEngine;
using UnityEditor;

namespace Railcam2D
{
    [CustomEditor(typeof(Trigger))]
    public class TriggerEditor : Editor
    {
        #region SceneGUI
        public void OnSceneGUI()
        {
            var currentTrigger = (Trigger)target;
            if (target == null)
                return;

            var handleSize = HandleUtility.GetHandleSize(Vector3.zero) * 0.2f;
            var snapValue = EditorPrefs.GetFloat(EditorPreferences.SnapKey, EditorPreferences.SnapValue);
            var snapSize = new Vector3(snapValue, snapValue, snapValue);

            // Draw editable trigger
            if (currentTrigger.gameObject.transform.parent != null &&
                currentTrigger.gameObject.GetComponentInParent<TriggerManager>() != null)
                GUIUtilities.DrawTriggerOnScene(currentTrigger, handleSize, snapSize, true);
            // Draw uneditable trigger
            else
                GUIUtilities.DrawTriggerOnScene(currentTrigger, handleSize, snapSize);
        }
        #endregion

        #region InspectorGUI
        public override void OnInspectorGUI()
        {
            var trigger = (Trigger)target;
            if (target == null)
                return;

            GUIUtilities.ComponentHelpBox(trigger, "Use Trigger Manager on the parent object to edit this trigger.");
        }
        #endregion
    }
}