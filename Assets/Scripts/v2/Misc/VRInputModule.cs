using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class VRInputModule : BaseInputModule
{
    public SteamVR_Input_Sources targetSource;
    public SteamVR_Action_Boolean clickAction;

    private GameObject currentObject = null;
    private PointerEventData data = null;

    private void Awake() {
        base.Awake();

        data = new PointerEventData(eventSystem);
    }

    public override void Process()
    {
        data.Reset();
        
        eventSystem.RaycastAll(data, m_RaycastResultCache);
        data.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        currentObject = data.pointerCurrentRaycast.gameObject;

        m_RaycastResultCache.Clear();

        HandlePointerExitAndEnter(data, currentObject);

        if(clickAction.GetStateDown(targetSource))
            ProcessPress(data);

        if(clickAction.GetStateUp(targetSource))
            ProcessRelease(data);
    }

    public PointerEventData GetData() {
        return data;
    }

    private void ProcessPress(PointerEventData data) {

    }
    
    private void ProcessRelease(PointerEventData data) {
        
    }
}
