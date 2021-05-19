using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    protected VirtualEnvironment virtualEnvironment;
    protected Users users;
    protected RealSpace realSpace;

    // Start is called before the first frame update
    public virtual void Awake()
    {
        foreach(Transform child in transform) {
            Transform2D tf = child.GetComponent<Transform2D>();
            if(tf != null) tf.Initializing();

            if(tf is VirtualEnvironment)
                virtualEnvironment = tf as VirtualEnvironment;
            else if(tf is Users)
                users = tf as Users;
            else if(tf is RealSpace)
                realSpace = tf as RealSpace;
        }
    }

}
