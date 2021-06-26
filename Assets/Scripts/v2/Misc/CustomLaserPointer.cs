//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using UnityEngine;
using System.Collections;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class CustomLaserPointer : MonoBehaviour
{
    public SteamVR_Behaviour_Pose pose;

    //public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.__actions_default_in_InteractUI;
    public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractUI");

    public bool active = true;
    public Color color;
    public float thickness = 0.002f;
    public Color clickColor = Color.green;
    public GameObject holder;
    public GameObject pointer;
    public float defaultPointerLength = 5.0f;

    public bool addRigidBody = false;
    public Transform reference;
    // public event PointerEventHandler PointerIn;
    // public event PointerEventHandler PointerOut;
    // public event PointerEventHandler PointerClick;

    Transform previousContact = null;

    // private bool visibleFlag = true;

    public User parentUser {
        get { return transform.parent.GetComponent<User>(); }
    }

    // public void ShowPointer() {
    //     visibleFlag = true;
    // }

    // public void HidePointer() {
    //     visibleFlag = false;
    // }

    private void Start()
    {

        if (pose == null)
            pose = this.GetComponent<SteamVR_Behaviour_Pose>();
        if (pose == null)
            Debug.LogError("No SteamVR_Behaviour_Pose component found on this object", this);

        if (interactWithUI == null)
            Debug.LogError("No ui interaction action has been set on this component.", this);


        holder = new GameObject();
        holder.name = "LaserPointer_Holder";
        holder.transform.parent = this.transform;
        holder.transform.localPosition = Vector3.zero;
        holder.transform.localRotation = Quaternion.identity;

        pointer = new GameObject();
        pointer.name = "LaserPointer";
        pointer.transform.parent = holder.transform;
        pointer.transform.localPosition = Vector3.zero;
        pointer.transform.localRotation = Quaternion.identity;
        // pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
        // pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
        // pointer.transform.localRotation = Quaternion.identity;

        Material newMaterial = new Material(Shader.Find("Unlit/Color"));
        newMaterial.SetColor("_Color", color);

        // BoxCollider collider = pointer.GetComponent<BoxCollider>();
        // if (addRigidBody)
        // {
        //     if (collider)
        //     {
        //         collider.isTrigger = true;
        //     }
        //     Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
        //     rigidBody.isKinematic = true;
        // }
        // else
        // {
        //     if (collider)
        //     {
        //         Object.Destroy(collider);
        //     }
        // }
        // pointer.GetComponent<MeshRenderer>().material = newMaterial;

        pointer.AddComponent<LineRenderer>();
        LineRenderer lineRenderer = pointer.GetComponent<LineRenderer>();
        lineRenderer.material = newMaterial;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;

        // if(!active) {
        //     HidePointer();
        // }
    }

    // public virtual void OnPointerIn(PointerEventArgs e)
    // {
    //     if (PointerIn != null)
    //         PointerIn(this, e);
    // }

    // public virtual void OnPointerClick(PointerEventArgs e)
    // {
    //     if (PointerClick != null) 
    //         PointerClick(this, e);   
    // }

    // public virtual void OnPointerOut(PointerEventArgs e)
    // {
    //     if (PointerOut != null)
    //         PointerOut(this, e);
    // }

    private void Update()
    {        
        if(active) {
            pointer.SetActive(true);
        }
        else {
            pointer.SetActive(false);
        }

        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + (transform.forward * defaultPointerLength);

        int layerMask = 1 << LayerMask.NameToLayer("UI");
        bool bHit = Physics.Raycast(raycast, out hit, Mathf.Infinity, layerMask);

        // Debug.DrawRay(transform.position, transform.forward, Color.yellow);
        // Debug.Log(bHit);
        // if(bHit) Debug.Log(hit.transform.gameObject);

        // bool bHit = CreateRaycast(defaultPointerLength);
        // RaycastHit hit = CreateRaycast(defaultPointerLength);

        if (previousContact && previousContact != hit.transform)
        {
            // PointerEventArgs args = new PointerEventArgs();
            // args.fromInputSource = pose.inputSource;
            // args.distance = 0f;
            // args.flags = 0;
            // args.target = previousContact;
            // UserEventArgs caller = new UserEventArgs(previousContact);
            // OnPointerOut(args);
            // parentUser.CallEvent(caller, UserEventType)

            CustomVRInputModule.instance.HoverEnd( previousContact.gameObject );

            previousContact = null;
        }

        if (bHit && previousContact != hit.transform)
        {
            // PointerEventArgs argsIn = new PointerEventArgs();
            // argsIn.fromInputSource = pose.inputSource;
            // argsIn.distance = hit.distance;
            // argsIn.flags = 0;
            // argsIn.target = hit.transform;
            // OnPointerIn(argsIn);

            CustomVRInputModule.instance.HoverBegin( hit.transform.gameObject );

            previousContact = hit.transform;

        }
        if (!bHit)
        {
            previousContact = null;
        }
        if (bHit && hit.distance < defaultPointerLength)
        {
            endPosition = hit.point;
        }

        LineRenderer lineRenderer = pointer.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);

        if (bHit && interactWithUI.GetStateDown(pose.inputSource))
        {
            // PointerEventArgs argsClick = new PointerEventArgs();
            // argsClick.fromInputSource = pose.inputSource;
            // argsClick.distance = hit.distance;
            // argsClick.flags = 0;
            // argsClick.target = hit.transform;
            // OnPointerClick(argsClick);

            CustomVRInputModule.instance.Click( hit.transform.gameObject );
        }

        if (interactWithUI != null && interactWithUI.GetState(pose.inputSource))
        {
            // pointer.transform.localScale = new Vector3(thickness * 5f, thickness * 5f, dist);
            pointer.GetComponent<LineRenderer>().material.color = clickColor;
        }
        else
        {
            // pointer.transform.localScale = new Vector3(thickness, thickness, dist);
            pointer.GetComponent<LineRenderer>().material.color = color;
        }

        // pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);
    }
}

public struct PointerEventArgs
{
    public SteamVR_Input_Sources fromInputSource;
    public uint flags;
    public float distance;
    public Transform target;
}

public delegate void PointerEventHandler(object sender, PointerEventArgs e);