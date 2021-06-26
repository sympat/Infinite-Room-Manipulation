using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICanvas : Transform2D
{
    private void Awake() {
        foreach(Transform child in this.transform) {
            if(child.GetComponent<BaseUI>() == null) {
                child.gameObject.AddComponent<BaseUI>();
            }
        }
    }
}
