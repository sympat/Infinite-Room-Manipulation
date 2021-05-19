using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealSpace : Bound2D
{
    public override void Initializing()
    {
        base.Initializing();

        foreach(Transform child in this.transform) {
            Transform2D tf = child.GetComponent<Transform2D>();

            if(tf is RealUser)  {
                tf.Initializing();
            }
        }

        this.gameObject.layer = LayerMask.NameToLayer("Real Space");
        this.gameObject.tag = "Real Space";
    }

}
