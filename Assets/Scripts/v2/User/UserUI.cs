using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class UserUI : MonoBehaviour
{
    // [TextArea]
    // public string paragraph1Text;
    // [TextArea]
    // public string paragraph2Text;
    // [TextArea]
    // public string choiceText;
    // [TextArea]
    // public string endText;

    // [TextArea]
    // public List<string> texts;

    public GameObject paragraph;
    public GameObject buttonOK;
    public GameObject buttonYes;
    public GameObject buttonNo;

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
}
