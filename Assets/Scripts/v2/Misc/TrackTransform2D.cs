using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTransform2D : MonoBehaviour
{

    public GameObject[] trackedObjects;

    private Vector3[] relativePosition;
    private Quaternion[] relativeRotation;

    private void Awake() {
        relativePosition = new Vector3[trackedObjects.Length];
        relativeRotation = new Quaternion[trackedObjects.Length];

        for(int i=0; i<trackedObjects.Length; i++) {
            relativePosition[i] = Utility.ProjectVector3Dto2D(this.transform.position - trackedObjects[i].transform.position);
            relativeRotation[i] = Utility.ProjectRotation3Dto2D(Quaternion.Inverse(this.transform.rotation) * trackedObjects[i].transform.rotation);
        }
    }

    private void Update()
    {
        for(int i=0; i<trackedObjects.Length; i++) {
            if(trackedObjects[i].activeInHierarchy) {
                this.transform.position = Utility.ProjectVector3Dto2D(trackedObjects[i].transform.position + relativePosition[i]);
                this.transform.rotation = Utility.ProjectRotation3Dto2D(trackedObjects[i].transform.rotation * relativeRotation[i]);
                break;
            }
        }
    }
}
