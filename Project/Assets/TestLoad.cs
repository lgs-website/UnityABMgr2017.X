using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLoad : MonoBehaviour
{
    const string oldMatResName = "m_Material",
                 newMatResName = "m_Material_2",
                 objName = "Cube";

    GameObject go;

    void OnGUI()
    {
        if (GUI.Button(new Rect(20, 20, 200, 50), "LoadCube"))
        {
            if (null == go)
                Load(objName);
        }

        if (GUI.Button(new Rect(20, 80, 200, 50), "ChangeCubeSkin"))
        {
            RefreshCubeMatByMatName(newMatResName);
        }

        if (GUI.Button(new Rect(20, 140, 200, 50), "ResetCubeSkin"))
        {
            RefreshCubeMatByMatName(oldMatResName);
        }

        if (GUI.Button(new Rect(20, 200, 200, 50), "DestoryCube"))
        {

        }
    }

    void Load(string objName)
    {
        IABManager.instance.LoadSingleObject("load.ab", objName, (o) =>
        {
            if (null != o)
            {
                go = Instantiate(o, Vector3.zero, Quaternion.identity) as GameObject;
            }
        });
    }

    void RefreshCubeMatByMatName(string matResName)
    {
        IABManager.instance.LoadSingleObject("material.ab", matResName, (o) =>
        {
            SetCubeMaterial(o as Material);
        });
    }

    void SetCubeMaterial(Material mat)
    {
        if (null != mat && null != go)
        {
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            mr.material = mat;
        }
    }

    void DisposeCube()
    {

    }
}
