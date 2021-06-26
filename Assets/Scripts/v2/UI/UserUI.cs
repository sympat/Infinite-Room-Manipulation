using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class UserUI : MonoBehaviour
{
    public GameObject paragraph;
    public GameObject buttonOK;
    public GameObject buttonYes;
    public GameObject buttonNo;
    public GameObject buttonYes2;
    public GameObject buttonNo2;

    public void DisableUI() {
        foreach(Transform child in transform) {
            child.gameObject.SetActive(false);
        }
    }

    public void PopUpParagraph(string text) {
        paragraph.GetComponentInChildren<TextMeshProUGUI>().SetText(text);
        paragraph.SetActive(true);
    }

    public void PopUpOkButton() {
        buttonOK.SetActive(true);
    }

    public void PopUpYesButton() {
        buttonYes.SetActive(true);
    }

    public void PopUpNoButton() {
        buttonNo.SetActive(true);
    }

    public void PopUpYes2Button() {
        buttonYes2.SetActive(true);
    }

    public void PopUpNo2Button() {
        buttonNo2.SetActive(true);
    }
}
