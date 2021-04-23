using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHandler : MonoBehaviour
{
    [TextArea]
    public string paragraph1Text;
    [TextArea]
    public string paragraph2Text;
    [TextArea]
    public string choiceText;
    [TextArea]
    public string endText;


    public void PopUpOkParagraph() {
        GameObject paragraph = this.transform.GetChild(0).gameObject;
        GameObject button_ok = this.transform.GetChild(1).gameObject;

        paragraph.GetComponentInChildren<TextMeshProUGUI>().SetText(paragraph1Text);
        paragraph.SetActive(true);
        button_ok.SetActive(true);
    }

    public void PopUPOK2Paragraph() {
        GameObject paragraph = this.transform.GetChild(0).gameObject;
        GameObject button_ok = this.transform.GetChild(1).gameObject;
        button_ok.gameObject.tag = "OK2Button";

        paragraph.GetComponentInChildren<TextMeshProUGUI>().SetText(paragraph2Text);
        paragraph.SetActive(true);
        button_ok.SetActive(true);
    }

    public void PopUpEndParagraph() {
        GameObject paragraph = this.transform.GetChild(0).gameObject;
        GameObject button_ok = this.transform.GetChild(1).gameObject;
        GameObject button_greater = this.transform.GetChild(2).gameObject;
        GameObject button_smaller = this.transform.GetChild(3).gameObject;

        paragraph.GetComponentInChildren<TextMeshProUGUI>().SetText(endText);
        paragraph.SetActive(true);
        button_ok.SetActive(false);
        button_greater.SetActive(false);
        button_smaller.SetActive(false);
    }

    public void PopUpChoiceParagraph() {
        StartCoroutine("InternalPopUpChoiceParagraph");
    }

    private IEnumerator InternalPopUpChoiceParagraph() {
        yield return new WaitForSeconds(3.0f);

        GameObject paragraph = this.transform.GetChild(0).gameObject;
        GameObject button_greater = this.transform.GetChild(2).gameObject;
        GameObject button_smaller = this.transform.GetChild(3).gameObject;

        paragraph.GetComponentInChildren<TextMeshProUGUI>().SetText(choiceText);
        paragraph.SetActive(true);
        button_greater.SetActive(true);
        button_smaller.SetActive(true);
    }

    public void DisableUI() {
        foreach(Transform child in transform) {
            child.gameObject.SetActive(false);
        }
    }
}
