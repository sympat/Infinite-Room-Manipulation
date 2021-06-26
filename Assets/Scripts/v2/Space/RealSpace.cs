using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealSpace : Bound2D
{
    private RealUser _realUser;

    public RealUser realUser {
        get { return _realUser; }
    }

    public override void Initializing()
    {
        base.Initializing();

        foreach(Transform child in this.transform) {
            Transform2D tf = child.GetComponent<Transform2D>();

            if(tf is RealUser)  {
                _realUser = tf as RealUser;
                _realUser.Initializing();
            }
        }

        this.gameObject.layer = LayerMask.NameToLayer("RealSpace");
        // this.gameObject.tag = "Real Space";
    }
}
