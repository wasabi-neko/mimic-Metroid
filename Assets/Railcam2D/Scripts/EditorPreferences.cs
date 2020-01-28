using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Railcam2D
{
    public static class EditorPreferences
    {
        // Key Value pairs
        public static string CameraTargetColorKey = "Camera Target";
        public static Color32 CameraTargetColorValue = new Color32(63, 111, 0, 255);

        public static string CameraBoundColorKey = "Camera Bound";
        public static Color32 CameraBoundColorValue = new Color32(111, 63, 0, 255);

        public static string RailColorKey = "Rail";
        public static Color32 RailColorValue = new Color32(255, 255, 95, 255);

        public static string RailFXColorKey = "Rail FX";
        public static Color32 RailFXColorValue = new Color32(159, 159, 159, 255);

        public static string TriggerColorKey = "Trigger";
        public static Color32 TriggerColorValue = new Color32(95, 255, 255, 255);

        public static string SnapKey = "Grid Snap";
        public static float SnapValue = 0.5f;

        // Get data from EditorPrefs
        public static Color32 GetColor32(string key, Color32 defaultValue)
        {
            #if UNITY_EDITOR

            if(EditorPrefs.HasKey(key))
                return GetColor32(key);
            return defaultValue;

            #else

            return GetColor32(key);

            #endif
        }

        public static Color32 GetColor32(string key)
        {
            #if UNITY_EDITOR

            // Color32 stored as 12 character string
            var defaultString = "255255255255";
            var valueString = EditorPrefs.GetString(key, defaultString);

            if (valueString.Length != 12)
                return new Color32(255, 255, 255, 255);

            var colorByteList = new List<byte>();

            for(int i = 0; i < valueString.Length; i += 3)
            {
                var ss = valueString.Substring(i, 3);

                int ssAsInt = 0;
                if(!int.TryParse(ss, out ssAsInt))
                    ssAsInt = 255;

                if (ssAsInt > 255 || ssAsInt < 0)
                    ssAsInt = 255;

                colorByteList.Add((byte)ssAsInt);
            }

            return new Color32(colorByteList[0], colorByteList[1], colorByteList[2], colorByteList[3]);

            #else

            return new Color32(255, 255, 255, 255);

            #endif
        }

        // Set data in EditorPrefs
        public static void SetColor32(string key, Color32 color)
        {
            #if UNITY_EDITOR

            var colorByteList = new List<byte>();
            colorByteList.Add(color.r);
            colorByteList.Add(color.g);
            colorByteList.Add(color.b);
            colorByteList.Add(color.a);

            string valueString = "";

            for(int i = 0; i < colorByteList.Count; i++)
            {
                valueString += colorByteList[i].ToString("000");
            }

            EditorPrefs.SetString(key, valueString);

            #endif
        }
    }
}
