using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IABRelationManager
{
    /// <summary>
    /// 依赖关系
    /// </summary>
    List<string> dependenceBundle;

    /// <summary>
    /// 被依赖关系
    /// </summary>
    List<string> referBundle;

    AssetBundle bundle;
    string bundleName;
    bool isLoadFinish = false;
    bool canAutoUnload = false;

    public IABRelationManager()
    {
        dependenceBundle = new List<string>();
        referBundle = new List<string>();
    }

    public string GetBundleName()
    {
        return bundleName;
    }

    public bool IsLoadFinish()
    {
        return isLoadFinish;
    }

    public void Initial(string _bundleName, bool _canAutoUnload = true)
    {
        isLoadFinish = false;
        bundleName = _bundleName;
        canAutoUnload = _canAutoUnload;
    }

    public void SetAssetBundle(AssetBundle _bundle)
    {
        bundle = _bundle;
        isLoadFinish = true;
    }

    public void SetDependences(string[] dependence)
    {
        if (null != dependence && dependence.Length > 0)
            dependenceBundle.AddRange(dependence);
    }

    public List<string> GetDependences()
    {
        return dependenceBundle;
    }

    /// <summary>
    /// 添加被依赖关系
    /// </summary>
    public void AddRefference(string bundleName)
    {
        if (null != referBundle)
            referBundle.Add(bundleName);
    }

    /// <summary>
    /// 获取被依赖关系
    /// </summary>
    /// <returns></returns>
    public List<string> GetRefference()
    {
        return referBundle;
    }

    public Object GetSingleResource(string resName)
    {
        return bundle.LoadAsset(resName);
    }  

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns>自己是否被释放了</returns>
    public bool RemoveReference(string bundleName)
    {
        int index = -1;

        if (null != referBundle && null != bundleName)
        {
            for (int i = 0, iMax = referBundle.Count; i < iMax; ++i)
            {
                if (referBundle[i] == bundleName)
                {
                    index = i;
                    break;
                }
            }
        }

        if (index > -1)
        {
            referBundle.RemoveAt(index);
        }

        if (index < 0)
            return Dispose();

        return true;
    }

    public bool Dispose()
    {
        if (canAutoUnload)
        {
            if (null != bundle)
            {
                bundle.Unload(false);
                IABManager.instance.RemoveAssetBundle(bundleName);
            }

            return true;
        }
        else
        {
            return false;
        }
    }
}
