using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    protected VirtualEnvironment _virtualEnvironment;
    protected Users _users;
    protected RealSpace _realSpace;

    public VirtualEnvironment virtualEnvironment {
        get { return _virtualEnvironment; }
    }

    public Users users {
        get { return _users; }
    }

    public RealSpace realSpace {
        get { return _realSpace; }
    }

    // Start is called before the first frame update
    public virtual void Awake()
    {
        foreach(Transform child in transform) {
            Transform2D tf = child.GetComponent<Transform2D>();

            if(tf is VirtualEnvironment)
                _virtualEnvironment = tf as VirtualEnvironment;
            else if(tf is Users)
                _users = tf as Users;
            else if(tf is RealSpace)
                _realSpace = tf as RealSpace;
        }

        foreach(Transform child in transform) {
            Transform2D tf = child.GetComponent<Transform2D>();
            if(tf != null) tf.Initializing();
        }
    }

}
