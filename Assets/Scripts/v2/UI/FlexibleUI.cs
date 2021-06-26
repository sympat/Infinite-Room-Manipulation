using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlexibleUI : MonoBehaviour
{
    private float initDistance;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start");
        if(this.transform.parent != null) {
            initDistance = (this.transform.position - this.transform.parent.position).magnitude;
            StartCoroutine(Adjusting());
        }
    }

    IEnumerator Adjusting() {
        while(true) {

            Vector3 origin = this.transform.parent.position;
            Vector3 direction = this.transform.position - this.transform.parent.position;
            Ray ray = new Ray(origin, direction);
            RaycastHit hit;

            // Debug.DrawRay(ray.origin, ray.direction * initDistance, Color.green);

            if(Physics.Raycast(ray, out hit, initDistance)) {
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.yellow);
                this.transform.position = ray.origin + ray.direction * hit.distance * 0.95f;          
            }
            else {
                this.transform.position = ray.origin + ray.direction * initDistance;          
            }

            yield return new WaitForFixedUpdate();
        }
    }
}
