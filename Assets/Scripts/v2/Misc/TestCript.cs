using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class TestCript : MonoBehaviour
{

    private void Awake() {
        this.gameObject.GetComponent<Users>().Initializing();
    }
    // private void Awake() {
    //     foreach(var element in Enumeration.GetAll<TestEnum1>()) {
    //         Debug.Log(element);
    //     }

    //     TestEnum1 testEnum1 = new TestEnum1();
    // }
}
