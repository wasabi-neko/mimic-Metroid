using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Class: InputManager
 * @bref: A input Tool made with a lot of static methods
*/
public class InputManager
{
    public static Dictionary<string, KeyCode> keyDic;
    readonly private static Dictionary<string, KeyCode> keyDicDefault = new Dictionary<string, KeyCode> {
        {"up", KeyCode.W},
        {"left", KeyCode.A},
        {"right", KeyCode.D},
        {"down", KeyCode.S},
        {"attack", KeyCode.J},
        {"jump", KeyCode.Space}
    };

    public static bool initKeyDic() 
    {
        keyDic = keyDicDefault;
        return true;
    }

    public static KeyCode GetKeyCode(string keyStr) 
    {
        if (keyDic == null) 
        {
            initKeyDic();
        } 
        if (keyDic.ContainsKey(keyStr))
        {
            return keyDic[keyStr];
        }
        else
        {
            Debug.LogError("$ERROR:ketStr:\"{key}\" not found; at 'InputManager.cs'");
            return KeyCode.None;
        }
    }

    public static bool GetKey(string keyStr)
    {
        return Input.GetKey(GetKeyCode(keyStr));
    }

    public static bool GetKeyDown(string keyStr) 
    {
        return Input.GetKeyDown(GetKeyCode(keyStr));
    }

}