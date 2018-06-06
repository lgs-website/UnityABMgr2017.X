using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public delegate void LoadAssetBundleCallback(string bundleName);
public delegate void LoadObjectCallBack(Object o);

/// <summary>
/// 单个物体的存取
/// </summary>
public class AssetObj
{
    Object obj = null;

    public Object Obj
    {
        get { return obj; }
    }

    public AssetObj(Object _obj)
    {
        obj = _obj;
        Debug.Log("AssetObj Name = " + _obj.name);
    }

    public void ReleaseObj()
    {
        if (null != obj)
        {
            //从内存卸载指定的资源
            Resources.UnloadAsset(obj);

            Debug.Log("ReleaseObj Name = " + obj.name);
        }
    }
}

/// <summary>
/// 相同AssetBundle的资源都存在这里
/// </summary>
public class AssetResObj
{
    //key : AssetBundle Name    value : 具体的资源
    Dictionary<string, AssetObj> dicAssetObj;

    public AssetResObj(string name, AssetObj _assetObj)
    {
        if (null == dicAssetObj)
            dicAssetObj = new Dictionary<string, AssetObj>();
        dicAssetObj.Add(name, _assetObj);
        Debug.Log("AssetResObj Key = " + name);
    }

    public void AddResObj(string name, AssetObj _assetObj)
    {
        if (null != dicAssetObj && !dicAssetObj.ContainsKey(name))
            dicAssetObj.Add(name, _assetObj);
        Debug.Log("AddResObj Key = " + name);
    }

    /// <summary>
    /// 释放单个资源
    /// </summary>
    /// <param name="name"></param>
    public void ReleaseResObj(string name)
    {
        if (dicAssetObj.ContainsKey(name))
        {
            AssetObj obj = dicAssetObj[name];
            obj.ReleaseObj();
            //dicAssetObj.Remove(name);
        }
        else
        {
            Debug.LogError("release obj name is not exit : " + name);
        }
    }

    /// <summary>
    /// 释放多个资源
    /// </summary>
    public void ReleaseAllResObj()
    {
        List<string> allNameList = new List<string>();
        allNameList.AddRange(dicAssetObj.Keys);
        for (int i = 0, iMax = allNameList.Count; i < iMax; ++i)
            ReleaseResObj(allNameList[i]);
    }

    public Object GetAssetObj(string name)
    {
        if (null != dicAssetObj && dicAssetObj.ContainsKey(name))
            return dicAssetObj[name].Obj;
        else
            return null;
    }
}

/// <summary>
/// 一个AssetBundle对应一个LoadAssetBundleCallbackManager
/// </summary>
public class LoadAssetBundleCallbackManager
{
    string bundleName = null;
    List<LoadAssetBundleCallback> callbackList = null;

    public LoadAssetBundleCallbackManager(string _bundleName)
    {
        bundleName = _bundleName;
        callbackList = new List<LoadAssetBundleCallback>();
    }

    public void AddCallback(LoadAssetBundleCallback callback)
    {
        if (null != callbackList)
            callbackList.Add(callback);
    }

    public void Callback(string bundleName)
    {
        if (null != bundleName && null != callbackList)
        {
            for (int i = 0, iMax = callbackList.Count; i < iMax; ++i)
            {
                callbackList[i](bundleName);
            }
        }
    }
}

public class IABManager : MonoBehaviour
{
    //正在加载的AB
    Dictionary<string, IABRelationManager> preLoadHelper = new Dictionary<string, IABRelationManager>();
    //已经加载完的AB
    Dictionary<string, IABRelationManager> loadedHelper = new Dictionary<string, IABRelationManager>();
    //已经从AB中load出的具体资源, key : AssetBundle Name   value : 资源
    Dictionary<string, AssetResObj> loadObjs = new Dictionary<string, AssetResObj>();
    //AB加载完的回调
    Dictionary<string, LoadAssetBundleCallbackManager> loadBundleCallback = new Dictionary<string, LoadAssetBundleCallbackManager>();

    public static IABManager instance;
    void Awake()
    {
        instance = this;
        StartCoroutine(IABManifestLoader.Instance.LoadManifest());
    }

    /// <summary>
    /// 是否加载了AssetBundle
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool IsLoadingAssetBundle(string bundleName)
    {
        if (!loadedHelper.ContainsKey(bundleName))
            return false;
        else
            return true;
    }

    /// <summary>
    /// 是否加载完毕
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    public bool IsLoadedFinish(string bundleName)
    {
        return loadedHelper.ContainsKey(bundleName);
    }

    public void RemoveAssetBundle(string bundleName)
    {
        if (loadedHelper.ContainsKey(bundleName))
            loadedHelper.Remove(bundleName);
    }

    string GetBundlePath(string bundleName)
    {
        string path = Path.Combine(IPathTools.GetAssetBundlePath(), bundleName);
        path = IPathTools.StandardPath(path);
        Debug.Log(path);
        return path;
    }

    //加载一个具体资源时，先从loadObjs中找，如果没有的话再从loadedHelper中找，再没有的话再根据AssetBundle名字和地址用WWW下载
    public void LoadSingleObject(string bundleName, string resName, LoadObjectCallBack callback)
    {
        //表示是否已近缓存了物体
        if (loadObjs.ContainsKey(bundleName))
        {
            AssetResObj tmpRes = loadObjs[bundleName];
            Object tmpObj = tmpRes.GetAssetObj(resName);
            if (null != tmpObj)
            {
                callback(tmpObj);
                return;
            }
        }

        //表示bundle已经加载过
        if (loadedHelper.ContainsKey(bundleName))
        {
            LoadObjectFromBundle(bundleName, resName, callback);
        }
        else
        {
            StartCoroutine(LoadAssetBundle(bundleName, (tmpBundleName) =>
            {
                LoadObjectFromBundle(bundleName, resName, callback);
            }));
        }

    }

    void LoadObjectFromBundle(string bundleName, string resName, LoadObjectCallBack callback)
    {
        if (loadedHelper.ContainsKey(bundleName))
        {
            IABRelationManager relation = loadedHelper[bundleName];
            Object tempObj = relation.GetSingleResource(resName);

            //缓存 tempObj
            AssetObj assetObj = new AssetObj(tempObj);

            if (!loadObjs.ContainsKey(bundleName))
            {
                AssetResObj tmpRes = new AssetResObj(resName, assetObj);
                loadObjs.Add(bundleName, tmpRes);
            }
            else
            {
                AssetResObj resObj = loadObjs[bundleName];
                resObj.AddResObj(resName, assetObj);
            }

            if (null != callback)
                callback(tempObj);
        }
        else
        {
            if (null != callback)
                callback(null);
        }
    }

    IEnumerator LoadAssetBundleDependences(string bundleName, string refName)
    {
        yield return null;
        if (!loadedHelper.ContainsKey(bundleName))
            yield return LoadAssetBundle(bundleName);

        if (null != refName)
        {
            IABRelationManager loader = loadedHelper[bundleName];
            loader.AddRefference(refName);
        }
    }

    public IEnumerator LoadAssetBundle(string bundleName, LoadAssetBundleCallback callback = null)
    {
        if (IABManifestLoader.Instance.IsFinishedLoad)
            yield return null;

        //管理AB加载完成后的回调函数
        if (null != callback)
        {
            if (loadBundleCallback.ContainsKey(bundleName))
            {
                LoadAssetBundleCallbackManager callbackManager = loadBundleCallback[bundleName];
                callbackManager.AddCallback(callback);
            }
            else
            {
                LoadAssetBundleCallbackManager callbackManager = new LoadAssetBundleCallbackManager(bundleName);
                callbackManager.AddCallback(callback);
                loadBundleCallback.Add(bundleName, callbackManager);
            }
        }

        if (preLoadHelper.ContainsKey(bundleName))
        {
            yield return null;
        }
        else
        {
            IABRelationManager relation = new IABRelationManager();
            relation.Initial(bundleName);
            preLoadHelper.Add(bundleName, relation);
            string bundlePath = GetBundlePath(bundleName);
            WWW www = new WWW(bundlePath);
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
                preLoadHelper.Remove(bundleName);
            }
            else
            {
                AssetBundle ab = www.assetBundle;
                string[] dependences = IABManifestLoader.Instance.assetBundleManifest.GetAllDependencies(ab.name);
                IABRelationManager tmpRelation = preLoadHelper[ab.name];
                tmpRelation.SetAssetBundle(ab);
                tmpRelation.SetDependences(dependences);

                if (!loadedHelper.ContainsKey(ab.name))
                {
                    loadedHelper.Add(ab.name, relation);
                    preLoadHelper.Remove(bundleName);
                }

                for (int i = 0, iMax = dependences.Length; i < iMax; ++i)
                {
                    if (!loadedHelper.ContainsKey(dependences[i]))
                        yield return LoadAssetBundleDependences(dependences[i], ab.name);
                }

                if (loadBundleCallback.ContainsKey(ab.name))
                {
                    loadBundleCallback[ab.name].Callback(ab.name);
                }
            }
            www.Dispose();
        }
    }

    #region 对外接口

    //释放一个bundle中的一个资源
    public void DisposeResObj(string bundleName, string resName)
    {
        if (loadObjs.ContainsKey(bundleName))
        {
            AssetResObj tmpRes = loadObjs[bundleName];
            tmpRes.ReleaseResObj(resName);
        }
    }

    //释放整个bundle中的资源
    public void DisposeResObj(string bundleName)
    {
        if (loadObjs.ContainsKey(bundleName))
        {
            AssetResObj tmpRes = loadObjs[bundleName];
            tmpRes.ReleaseAllResObj();
            Resources.UnloadUnusedAssets();
        }
    }

    //释放所有的bundle加载出来的资源
    public void DisposeAllObj()
    {
        List<string> keys = new List<string>();
        keys.AddRange(loadObjs.Keys);

        for (int i = 0, iMax = keys.Count; i < iMax; ++i)
            DisposeResObj(keys[i]);

        loadObjs.Clear();
    }

    public void DisposeBundle(string bundleName)
    {
        if (loadedHelper.ContainsKey(bundleName))
        {
            IABRelationManager loader = loadedHelper[bundleName];
            List<string> dependences = loader.GetDependences();
            for (int i = 0, iMax = dependences.Count; i < iMax; ++i)
            {
                if (loadedHelper.ContainsKey(dependences[i]))
                {
                    IABRelationManager dependence = loadedHelper[dependences[i]];
                    if (dependence.RemoveReference(bundleName))
                        DisposeBundle(dependence.GetBundleName());
                    dependence.RemoveReference(bundleName);
                }
            }

            if (loader.GetRefference().Count <= 0)
            {
                loader.Dispose();
                loadedHelper.Remove(bundleName);
            }
        }
    }

    public void DisposeAllBundle()
    {
        List<string> keys = new List<string>();
        keys.AddRange(loadedHelper.Keys);
        for (int i = 0, iMax = keys.Count; i < iMax; ++i)
        {
            IABRelationManager loader = loadedHelper[keys[i]];
            loader.Dispose();
        }
        loadedHelper.Clear();
    }

    public void DisposeAllBundleAndRes()
    {
        DisposeAllObj();
        DisposeAllBundle();
    }

    #endregion
}
