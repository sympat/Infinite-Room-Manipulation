using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTransform2D : MonoBehaviour
{

    public GameObject trackedObject;

    private Vector3 relativePosition;
    private Quaternion relativeRotation;

    private void Awake() {
        relativePosition = Utility.ProjectVector3Dto2D(this.transform.position - trackedObject.transform.position);
        relativeRotation = Utility.ProjectRotation3Dto2D(Quaternion.Inverse(this.transform.rotation) * trackedObject.transform.rotation);
    }

    private void Update() {
        if(trackedObject.activeInHierarchy) {
            this.transform.position = Utility.ProjectVector3Dto2D(trackedObject.transform.position + relativePosition);
            this.transform.rotation = Utility.ProjectRotation3Dto2D(trackedObject.transform.rotation * relativeRotation);
        }
    }
}
