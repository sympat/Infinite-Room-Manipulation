using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Transform2D tf = GetComponent<Transform2D>();

        // tf.Rotate(30);
        tf.RotateAround(tf.Position + tf.Right * 5, 30);
    }

    // Update is called once per fra
}
