using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.EventSystems;

[System.Serializable]
public class UIContainer {
    public UICanvas canvas;
    public Vector3 initPosition;
    public Vector3 initRotation;
    public bool attachToUser;
    public bool applyFlexibleUI;
    [HideInInspector]
    public Transform parent;
}

public enum UIBehaviour {
    Click
}

public class UIManager : Singleton<UIManager>
{
    private Dictionary<string, UICanvas> ui = new Dictionary<string, UICanvas>();
    private Dictionary<string, UIContainer> initInfo = new Dictionary<string, UIContainer>();
    // private RootManager _rootManager;

    // public RootManager rootManager {
    //     get { return _rootManager; }
    // }

    // public void Initializing(RootManager root) {
    //     _rootManager = root;
    // }

    public void GenerateUI(string name, UIContainer uiInfo) {
        UICanvas instantiatedUI = Instantiate(uiInfo.canvas);

        if(uiInfo.parent) {
            instantiatedUI.transform.SetParent(uiInfo.parent.transform);

            instantiatedUI.transform.localPosition = uiInfo.initPosition;
            instantiatedUI.transform.localRotation = Quaternion.Euler(uiInfo.initRotation);
        }
        else {
            instantiatedUI.transform.position = uiInfo.initPosition;
            instantiatedUI.transform.rotation = Quaternion.Euler(uiInfo.initRotation);
        }

        // string nameWithID = name + "_" + instantiatedUI.GetInstanceID();

        if(uiInfo.applyFlexibleUI) instantiatedUI.gameObject.AddComponent<FlexibleUI>();

        instantiatedUI.name = name;
        ui.Add(name, instantiatedUI);
        initInfo.Add(name, uiInfo);
        ui[name].gameObject.SetActive(false);
    }

    public UICanvas GetUI(string name) {
        return ui[name];
    }

    public void DestroyUI(string name) {
        Destroy(ui[name]);
        ui.Remove(name);
    }

    // public void EnableUI(string name, bool useLocal = false) {
    //     if(useLocal) {
    //         Debug.Log(ui[name].transform.parent.TransformPoint(initPosition[name]));
    //         ui[name].transform.position = ui[name].transform.parent.TransformPoint(initPosition[name]);
    //         ui[name].transform.SetParent(this.transform);
    //     }
    // }

    // public void DisableUI(string name) {
    //     ui[name].transform.position = ui[name].transform.parent.TransformPoint(initPosition[name]);
    //     ui[name].transform.SetParent(this.transform);
    // }

    public void ToggleUI(string name, bool enabled, User user, bool useLocal = false) {

        if(enabled) {
            if(useLocal) {
                // Debug.Log(user.Face.transform.TransformPoint(initInfo[name].initPosition));
                ui[name].transform.SetParent(this.transform);
                ui[name].transform.position = user.Face.transform.TransformPoint(initInfo[name].initPosition);
            }
        }
        else {
            if(useLocal) {
                ui[name].transform.SetParent(user.Face.transform);
                ui[name].transform.localPosition = initInfo[name].initPosition;
                ui[name].transform.localRotation = Quaternion.Euler(initInfo[name].initRotation);
            }
        }
        
        ui[name].gameObject.SetActive(enabled);
    }

    public void AddEvent(UIBehaviour eventType, string name, UnityAction call, params string[] childName) {
        Transform target = ui[name].transform;

        foreach(string targetName in childName) {
            target = target.Find(targetName);
        }

        target.GetComponent<BaseUI>().AddEvent(eventType, call);
    }

    public void RemoveEvent(UIBehaviour eventType, string name, UnityAction call, params string[] childName) {
        Transform target = this.transform;
  
        foreach(string targetName in childName) {
            target = target.Find(targetName);
        }

        target.GetComponent<BaseUI>().RemoveEvent(eventType, call);
    }
}
