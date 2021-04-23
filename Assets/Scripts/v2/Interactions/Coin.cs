using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public float rotationSpeed = 30.0f;

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0), Space.World);
    }
}
