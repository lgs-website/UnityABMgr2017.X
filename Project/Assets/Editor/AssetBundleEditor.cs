using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundleEditor
{
    static string outPath = null;
    static string assetPath = @"Assets/Art/";
    static string assetFullPath = Application.dataPath + "/Art/";

    [MenuItem("Itools/BuildAssetBundle")]
    public static void BuildAssetBundle()
    {
        outPath = IPathTools.GetEditeABOutPath();

        //判断给定的路径是否存在，注意只能判断路径（即文件夹）， 不具体到文件！！！
        if (Directory.Exists(outPath))
        {
            DirectoryInfo di = new DirectoryInfo(outPath);
            //指定是否删除子目录和文件,若为 true，则删除此目录、其子目录以及所有文件
            //若为 false,目录不为空会报异常，即只能当目录为空的时候可以传 false
            di.Delete(true);
        }

        //创建目录
        Directory.CreateDirectory(outPath);

        BuildPipeline.BuildAssetBundles(outPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

        //重新导入有更新的资源,加入了新的文件、文件夹之类的，调用该方法可以及时显示在工程中
        AssetDatabase.Refresh();
    }

    [MenuItem("Itools/MarkAssetBundle")]
    public static void MarkAssetBundle()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
        assetFullPath = FixedWindowsPath(assetFullPath);

        Dictionary<string, string> bundlePathDic = new Dictionary<string, string>();

        DirectoryInfo dir = new DirectoryInfo(assetFullPath);
        FileSystemInfo[] fileSystemInfo = dir.GetFileSystemInfos();
        for (int i = 0, iMax = fileSystemInfo.Length; i < iMax; ++i)
        {
            FileSystemInfo tmpFile = fileSystemInfo[i];
            if (tmpFile is DirectoryInfo)
            {
                string tmpPath = Path.Combine(assetFullPath, tmpFile.Name);
                ErgodicDirectory(tmpPath, bundlePathDic);
            }
        }

        AssetDatabase.Refresh();
    }

    //递归遍历目录
    public static void ErgodicDirectory(string path, Dictionary<string, string> bundlePathDic)
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        if (null != dir)
        {
            //返回表示某个目录中所有文件和子目录的强类型 System.IO.FileSystemInfo 项的数组。
            FileSystemInfo[] fileSystemInfo = dir.GetFileSystemInfos();
            for (int i = 0, iMax = fileSystemInfo.Length; i < iMax; ++i)
            {
                FileSystemInfo tmpFile = fileSystemInfo[i];
                string tmpPath = Path.Combine(path, tmpFile.Name);

                if (tmpFile is DirectoryInfo)
                {
                    ErgodicDirectory(tmpPath, bundlePathDic);
                }
                else
                {
                    SetAssetBundleName(tmpFile, bundlePathDic);
                }
            }
        }
        else
        {
            Debug.LogError("the path is not exit");
        }
    }

    //设置AssetBundle name
    public static void SetAssetBundleName(FileSystemInfo fileSystemInfo, Dictionary<string, string> bundlePathDic)
    {
        FileInfo fileInfo = fileSystemInfo as FileInfo;

        //获取表示文件扩展名部分的字符串,这里的意思是不更改meta文件的文件名
        if (fileInfo.Extension == ".meta")
            return;

        string assetBundleName = fileInfo.DirectoryName;
        string fullPath = FixedWindowsPath(fileInfo.DirectoryName);

        int tmpCount1 = fullPath.Length;
        int tmpCount2 = assetFullPath.Length;
        int assetPathCount = tmpCount1 - tmpCount2;

        string assetBundlePath = fullPath.Substring(tmpCount2, assetPathCount);
        string path = assetPath + assetBundlePath + "/" + fileInfo.Name;
        Debug.Log("path = " + path);

        AssetImporter importer = AssetImporter.GetAtPath(path);
        //设置 assetbundle name
        importer.assetBundleName = assetBundlePath;

        //设置 assetbundle 后缀
        if (fileInfo.Extension == ".unity")
            importer.assetBundleVariant = "u3d";
        else
            importer.assetBundleVariant = "ab";

        //保存bundle的相对路径
        int startIndex = fileInfo.FullName.IndexOf("Art");
        int endIndex = fileInfo.FullName.Length;
        int count = endIndex - startIndex;
        string bundlePath = fileInfo.FullName.Substring(startIndex, count);
        if (!bundlePathDic.ContainsKey(assetBundleName))
        {
            bundlePathDic.Add(assetBundleName, bundlePath);
        }
    }

    //修正路径
    public static string FixedWindowsPath(string path)
    {
        path = path.Replace("\\", "/");
        return path;
    }
}
