using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class BaseUI : MonoBehaviour, IPointerClickHandler
{
    private Dictionary<UIBehaviour, UnityEvent> uiEvents;

    private void Awake() {
        foreach(Transform child in this.transform) {
            if(child.GetComponent<BaseUI>() == null) {
                child.gameObject.AddComponent<BaseUI>();
            }
        }

        uiEvents = new Dictionary<UIBehaviour, UnityEvent>();

        foreach(UIBehaviour behave in Enum.GetValues(typeof(UIBehaviour))) {
            uiEvents.Add(behave, new UnityEvent());
        }
    }

    public void AddEvent(UIBehaviour eventType, UnityAction call) {
        uiEvents[eventType].AddListener(call);
    }

    public void RemoveEvent(UIBehaviour eventType, UnityAction call) {
        uiEvents[eventType].RemoveListener(call);
    }

    public void OnPointerClick(PointerEventData pointerEventData) {
        // Debug.Log("ONPointerClick!");
        uiEvents[UIBehaviour.Click].Invoke();
    }

    // public BaseUI AssignText(string text) {
    //     this.transform.GetComponentInChildren<TextMeshProUGUI>().SetText(text);

    //     return this;
    // }

    // public BaseUI AddClickEvent(int index, UnityAction call)  {
    //     Button[] buttons = this.transform.GetComponentsInChildren<Button>();

    //     buttons[index].onClick.AddListener(call);

    //     return this;
    // }

    // public BaseUI AddClickEvent(UnityAction call) {
    //     Button[] buttons = this.transform.GetComponentsInChildren<Button>();

    //     foreach(Button button in buttons) {
    //         button.onClick.AddListener(call);
    //     }

    //     return this;
    // }

    // public BaseUI RemoveClickEvent(int index, UnityAction call) {
    //     Button[] buttons = this.transform.GetComponentsInChildren<Button>();

    //     buttons[index].onClick.RemoveListener(call);

    //     return this;

    // }

    // public BaseUI RemoveClickEvent(UnityAction call) {
    //     Button[] buttons = this.transform.GetComponentsInChildren<Button>();

    //     foreach(Button button in buttons) {
    //         button.onClick.RemoveListener(call);
    //     }

    //     return this;
    // }
}
