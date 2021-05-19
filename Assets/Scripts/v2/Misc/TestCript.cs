using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestCript : MonoBehaviour
{

    private void Start() {
        Debug.Log(this.transform.FindObjectWithTag("OKButton"));
    }
    // public void OnPointerDown(PointerEventData eventData)
    // {
    //     Debug.Log("Clicked: " + eventData.pointerCurrentRaycast.gameObject.name);
    // }

    // public void OnPointerClick(PointerEventData pointerEventData)
    // {
    //     //Output to console the clicked GameObject's name and the following message. You can replace this with your own actions for when clicking the GameObject.
    //     Debug.Log(name + " Game Object Clicked!");
    // }
}
