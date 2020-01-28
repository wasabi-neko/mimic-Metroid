using UnityEngine;
using UnityEditor;

namespace Railcam2D
{
    public static class PreferencesGUI
    {
        private static bool _prefsLoaded = false;

        // Preferences
        private static Color32 _cameraTargetColor;
        private static Color32 _cameraBoundColor;
        private static Color32 _railColor;
        private static Color32 _railFXColor;
        private static Color32 _triggerColor;
        private static float _snapInterval;

        private static string _cameraTargetColorKey = EditorPreferences.CameraTargetColorKey;
        private static string _cameraBoundColorKey = EditorPreferences.CameraBoundColorKey;
        private static string _railColorKey = EditorPreferences.RailColorKey;
        private static string _railFXColorKey = EditorPreferences.RailFXColorKey;
        private static string _triggerColorKey = EditorPreferences.TriggerColorKey;
        private static string _snapIntervalKey = EditorPreferences.SnapKey;

        private static Color32 _cameraTargetColorValue = EditorPreferences.CameraTargetColorValue;
        private static Color32 _cameraBoundColorValue = EditorPreferences.CameraBoundColorValue;
        private static Color32 _railColorValue = EditorPreferences.RailColorValue;
        private static Color32 _railFXColorValue = EditorPreferences.RailFXColorValue;
        private static Color32 _triggerColorValue = EditorPreferences.TriggerColorValue;
        private static float _snapIntervalValue = EditorPreferences.SnapValue;

        // Draw Preference Panel
        [PreferenceItem("Railcam 2D")]
        private static void CustomPreferencesGUI()
        {
            if(!_prefsLoaded)
            {
                _cameraTargetColor = EditorPreferences.GetColor32(_cameraTargetColorKey, _cameraTargetColorValue);
                _cameraBoundColor = EditorPreferences.GetColor32(_cameraBoundColorKey, _cameraBoundColorValue);
                _railColor = EditorPreferences.GetColor32(_railColorKey, _railColorValue);
                _railFXColor = EditorPreferences.GetColor32(_railFXColorKey, _railFXColorValue);
                _triggerColor = EditorPreferences.GetColor32(_triggerColorKey, _triggerColorValue);
                _snapInterval = EditorPrefs.GetFloat(_snapIntervalKey, _snapIntervalValue);

                _prefsLoaded = true;
            }

            EditorGUILayout.LabelField("Gizmo Colors", EditorStyles.boldLabel);

            _cameraTargetColor = EditorGUILayout.ColorField(_cameraTargetColorKey, _cameraTargetColor);
            _cameraBoundColor = EditorGUILayout.ColorField(_cameraBoundColorKey, _cameraBoundColor);
            _railColor = EditorGUILayout.ColorField(_railColorKey, _railColor);
            _railFXColor = EditorGUILayout.ColorField(_railFXColorKey, _railFXColor);
            _triggerColor = EditorGUILayout.ColorField(_triggerColorKey, _triggerColor);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);

            _snapInterval = EditorGUILayout.FloatField(_snapIntervalKey, _snapInterval);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button("Restore defaults"))
            {
                _cameraTargetColor = _cameraTargetColorValue;
                _cameraBoundColor = _cameraBoundColorValue;
                _railColor = _railColorValue;
                _railFXColor = _railFXColorValue;
                _triggerColor = _triggerColorValue;
                _snapInterval = _snapIntervalValue;
            }

            if (GUI.changed)
            {
                EditorPreferences.SetColor32(_cameraTargetColorKey, _cameraTargetColor);
                EditorPreferences.SetColor32(_cameraBoundColorKey, _cameraBoundColor);
                EditorPreferences.SetColor32(_railColorKey, _railColor);
                EditorPreferences.SetColor32(_railFXColorKey, _railFXColor);
                EditorPreferences.SetColor32(_triggerColorKey, _triggerColor);
                EditorPrefs.SetFloat(_snapIntervalKey, _snapInterval);
            }
        }
    }
}
