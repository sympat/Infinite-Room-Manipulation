using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserPointer : MonoBehaviour
{
    public float defaultPointerLength = 5.0f;
    public Color pointerColor;
    public GameObject predefinedPointer; // TODO: Transform만 반영
    public Canvas userUI;

    private GameObject pointerObj;

    private void Start() {

        if(predefinedPointer == null) {
            pointerObj = new GameObject();
            pointerObj.name = "pointer";
            pointerObj.transform.parent = this.transform;
            pointerObj.transform.localPosition = Vector3.zero;
            pointerObj.transform.localRotation = Quaternion.identity;
            pointerObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
        else {
            pointerObj = predefinedPointer;
        }

        if(pointerObj.GetComponent<LineRenderer>() == null) {
            pointerObj.AddComponent<LineRenderer>();
        }

        LineRenderer lineRenderer = pointerObj.GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        Material newMaterial = new Material(Shader.Find("Unlit/Color"));
        newMaterial.SetColor("_Color", pointerColor);
        lineRenderer.material = newMaterial;

        // if(pointerObj.GetComponent<Camera>() == null) {
        //     pointerObj.AddComponent<Camera>();
        // }

        // Camera cameraForRayCast = pointerObj.GetComponent<Camera>();
        // cameraForRayCast.fieldOfView = 0.1f;
        // cameraForRayCast.nearClipPlane = 0.01f;
        // cameraForRayCast.clearFlags = CameraClearFlags.Nothing;
        // cameraForRayCast.cullingMask = -1; // -1 means nothing
        // cameraForRayCast.targetDisplay = 2;
        // cameraForRayCast.enabled = false;
        // userUI.worldCamera = cameraForRayCast;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + (transform.forward * defaultPointerLength);

        RaycastHit hit = CreateRaycast(defaultPointerLength);
        if(hit.collider != null)
            endPosition = hit.point;

        LineRenderer lineRenderer = pointerObj.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);
    }

    private RaycastHit CreateRaycast(float length) {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);
        Physics.Raycast(ray, out hit, length);

        return hit;
    }
}
