using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IABManifestLoader
{
    public AssetBundleManifest assetBundleManifest = null;
    public string assetManifestName = null;
    public AssetBundle manifestloader = null;

    bool isFinishedLoad = false;


    public IABManifestLoader()
    {
        assetBundleManifest = null;
        manifestloader = null;
        isFinishedLoad = false;
        assetManifestName = IPathTools.GetABMainfestName();
    }

    static IABManifestLoader instance = null;

    public static IABManifestLoader Instance
    {
        get
        {
            if (null == instance)
                instance = new IABManifestLoader();
            return IABManifestLoader.instance;
        }
    }

    public IEnumerator LoadManifest()
    {
        //这里获取到了文件的路径
        assetManifestName = IPathTools.StandardPath(assetManifestName);

        WWW manifest = new WWW(assetManifestName);
        yield return manifest;

        if (string.IsNullOrEmpty(manifest.error))
        {
            if (manifest.progress >= 1.0)
            {
                manifestloader = manifest.assetBundle;
                assetBundleManifest = manifestloader.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
                isFinishedLoad = true;
            }
        }
        else
        {
            Debug.LogError(manifest.error);
        }
    }

    public string[] GetDepences(string name)
    {
        if (null != assetBundleManifest)
            return assetBundleManifest.GetAllDependencies(name);
        return null;
    }

    public void UnloadManifest()
    {
        if (null != manifestloader)
            manifestloader.Unload(false);
    }

    public bool IsFinishedLoad
    {
        get { return isFinishedLoad; }
    }
}
