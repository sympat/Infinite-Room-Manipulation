using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle2D : Transform2D
{
    public float initRadius;
    public float initHeight;

    protected CapsuleCollider Circle {
        get {
            return GetComponent<CapsuleCollider>();
        }
    }

    public float Radius
    {
        get
        {
            return Circle.radius;
        }

        set
        {
            UpdateCircle(value, this.Height);
        }
    }

    public float Height
    {
        get
        {
            return Circle.height;
        }

        set
        {
            UpdateCircle(this.Radius, value);
        }
    }

    public override void Initializing()
    {
        ApplySize();
    }

    public void ApplySize() {

        // transform.localScale = Vector3.one; // ignore scale

        this.Radius = initRadius;
        this.Height = initHeight;
    }


    protected virtual void UpdateCircle(float radius, float height) {
        // update collider
        Circle.radius = radius;
        Circle.height = height;
        Circle.center = new Vector3(Circle.center.x, height / 2, Circle.center.z);
    }
}
