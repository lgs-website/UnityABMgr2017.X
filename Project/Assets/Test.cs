using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public AssetBundle manifestLoader = null;
    public AssetBundleManifest assetManifest = null;
    public Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();

    string manifestPath = string.Empty;
    bool isLoadManifestFinished = false;

    // Use this for initialization
    void Start()
    {
        manifestPath = IPathTools.GetABMainfestName();
        StartCoroutine(LoadManifest());

        string path = IPathTools.GetAssetBundlePath() + "/load.ab";
        StartCoroutine(LoadAssetBundle(path));
    }

    IEnumerator LoadManifest()
    {
        WWW www = new WWW(manifestPath);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log(www.error);
        }
        else
        {
            //progress : 0到1的值，0表示任何数据都没有开始下载，1表示下载完毕。
            if (www.progress >= 1.0f)
            {
                isLoadManifestFinished = true;
                manifestLoader = www.assetBundle;
                assetManifest = manifestLoader.LoadAsset("AssetBundleManifest") as AssetBundleManifest;

                //输出所有AssetBundle的name
                string[] allBundle = assetManifest.GetAllAssetBundles();
                if (null != allBundle)
                {
                    for (int i = 0, iMax = allBundle.Length; i < iMax; ++i)
                    {
                        //Debug.Log(allBundle[i]);
                    }
                }
            }
        }
    }

    IEnumerator LoadAssetBundle(string path)
    {
        while (!isLoadManifestFinished)
            yield return null;

        WWW www = new WWW(path);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log(www.error);
        }
        else
        {
            AssetBundle ab = www.assetBundle;
            //Debug.Log("ab.name = " + ab.name);
            if (!abDic.ContainsKey(ab.name))
                abDic.Add(ab.name, ab);

            string[] dependence = assetManifest.GetAllDependencies(ab.name);
            for (int i = 0, iMax = dependence.Length; i < iMax; ++i)
            {
                Debug.Log(dependence[i]);
                if (!abDic.ContainsKey(dependence[i]))
                {
                    string _path = IPathTools.GetAssetBundlePath() + "/" + dependence[i];
                    yield return LoadAssetBundle(_path);
                }
            }

            //实例化 Cube
            if (ab.name == "load.ab")
            {
                AssetBundle bundle = abDic[ab.name];
                Object obj = bundle.LoadAsset("Cube");
                Instantiate(obj, Vector3.zero, Quaternion.identity);
            }


            www.Dispose();
        }
    }

    //循环加载依赖项
    public IEnumerator LoadDependencesAssetBundle(string path)
    {
        while (!isLoadManifestFinished)
            yield return null;
        using (WWW www = new WWW(path))
        {
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log(www.error);
            }
            else
            {
                if (www.progress >= 1.0)
                {
                    AssetBundle ab = www.assetBundle;
                    if (!abDic.ContainsKey(ab.name))
                    {
                        abDic.Add(ab.name, ab);
                        Debug.Log("依赖ab.name = " + ab.name);
                    }

                    string[] dependences = assetManifest.GetAllDependencies(ab.name);
                    for (int i = 0, iMax = dependences.Length; i < iMax; ++i)
                    {
                        string abPath = IPathTools.GetAssetBundlePath() + "/" + dependences[i];
                        yield return LoadDependencesAssetBundle(abPath);
                    }
                }
            }
        }
    }
}
