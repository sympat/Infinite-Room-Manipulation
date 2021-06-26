using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportManager : MonoBehaviour
{
    protected Experiment2 experiment2;

    private void Awake() {
        experiment2 = GetComponent<Experiment2>();
    }

    private void FixedUpdate() {
        experiment2.isLocomotionDone = true;
    }
}
