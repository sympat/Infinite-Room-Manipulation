using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCript : MonoBehaviour
{
    BoxCollider box;

    private void Start() {
        // box = GetComponent<BoxCollider>().bounds;
        GetComponent<BoxCollider>().enabled = true;
        box = GetComponent<BoxCollider>();
        
    }

    private void Update() {
        Debug.Log($"box.size {box.size}");
        Debug.Log($"box.center {this.transform.TransformPoint(box.center)}");
        Debug.Log($"box.max {this.transform.TransformPoint(box.center + box.size / 2)}");
        Debug.Log($"box.min {this.transform.TransformPoint(box.center - box.size / 2)}");
    }
}
