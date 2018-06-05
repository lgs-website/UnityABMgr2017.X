using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

/// <summary>
/// 工具类
/// </summary>
public class IPathTools
{
    public static string GetPlatformFoldName(RuntimePlatform platform)
    {
        switch (platform)
        {
            case RuntimePlatform.Android:
                return "Android";
            case RuntimePlatform.IPhonePlayer:
                return "IOS";
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                return "windows";
            default:
                return null;
        }
    }

    public static string GetAppFilePath()
    {
        string path = null;

        #if UNITY_ANDROID  
            path = "jar:file://" + Application.streamingAssetsPath;  
        #elif UNITY_IPHONE  
            path = Application.streamingAssetsPath;  
        #elif UNITY_STANDALONE_WIN || UNITY_EDITOR  
            path = "file://" + Application.streamingAssetsPath;  
        #else
            path = string.Empty;
        #endif

        return path;
    }

    public static string GetAssetBundlePath()
    {
        string platFolder = GetPlatformFoldName(Application.platform);

        //组合后的路径。 如果指定的路径之一是零长度字符串，则该方法返回其他路径。 如果 path2 包含绝对路径，则该方法返回 path2
        string allPath = Path.Combine(GetAppFilePath(), platFolder);
        allPath = StandardPath(allPath);
        return allPath;
    }

    public static string GetEditeABOutPath()
    {
        string tempPath = string.Empty;
        if (Application.platform == RuntimePlatform.WindowsEditor)
            tempPath = Application.streamingAssetsPath;
        else
            tempPath = Application.persistentDataPath;

        return Path.Combine(tempPath, GetPlatformFoldName(Application.platform));
    }

    public static string GetABMainfestName()
    {
        string name = GetPlatformFoldName(Application.platform);
        string path = GetAssetBundlePath();
        return StandardPath(Path.Combine(path, name));
    }

    public static string StandardPath(string oldPath)
    {
        string newPath = oldPath;
        if (oldPath.Contains("\\"))
            newPath = oldPath.Replace("\\", "/");
        return newPath;
    }
}
