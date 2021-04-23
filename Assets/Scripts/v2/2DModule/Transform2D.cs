using UnityEngine;

public class Transform2D : MonoBehaviour {
    public static Vector3 CastVector2Dto3D(Vector2 vec2, float height = 0)
    {
        int significantDigit = 5; // 유효숫자
        float significant = Mathf.Pow(10, significantDigit);

        float xValue = Mathf.Floor(vec2.x * significant) / significant;
        float yValue = Mathf.Floor(vec2.y * significant) / significant;

        return new Vector3(xValue, height, yValue);
    }

    public static Vector2 CastVector3Dto2D(Vector3 vec3)
    {
        int significantDigit = 5;
        float significant = Mathf.Pow(10, significantDigit);

        float xValue = Mathf.Floor(vec3.x * significant) / significant; // 0.6666667 이면 0.666666 으로 버림
        float zValue = Mathf.Floor(vec3.z * significant) / significant;

        return new Vector2(xValue, zValue);
    }

    public static Quaternion CastRotation2Dto3D(float degree)
    {
        return Quaternion.Euler(0, -degree, 0);
    }

    public static float CastRotation3Dto2D(Quaternion rotation)
    {
        return -rotation.eulerAngles.y;
    }

    public Vector2 Forward
    {
        get { return CastVector3Dto2D(transform.forward); }
        set
        {
            if (value.magnitude > 1)
                value = value.normalized;

            transform.forward = CastVector2Dto3D(value);
        }
    }

    public Vector2 Right
    {
        get { return CastVector3Dto2D(transform.right); }
        set
        {
            if (value.magnitude > 1)
                value = value.normalized;

            transform.right = CastVector2Dto3D(value);
        }
    }

    public Vector2 Position 
    {
        get
        {
            return CastVector3Dto2D(transform.position);
        }

        set
        {
            transform.position = CastVector2Dto3D(value);
        }
    }

    public Vector2 LocalPosition
    {
        get { return CastVector3Dto2D(transform.localPosition); }
        set
        {
            transform.localPosition = CastVector2Dto3D(value);
        }
    }

    public float Rotation
    {
        get { return CastRotation3Dto2D(transform.rotation); }
        set
        {
            transform.rotation = CastRotation2Dto3D(value);
        }
    }

    public float LocalRotation
    {
        get { return CastRotation3Dto2D(transform.localRotation); }
        set
        {
            transform.localRotation = CastRotation2Dto3D(value);
        }
    }

    public Vector2 LocalScale
    {
        get { return CastVector3Dto2D(transform.localScale); }
        set
        {
            transform.localScale = CastVector2Dto3D(value, transform.localScale.y);
        }
    }

    public virtual void Initializing() {}

    public void Translate(Vector2 translation, Space relativeTo = Space.Self)
    {
        transform.Translate(CastVector2Dto3D(translation), relativeTo);
    }

    public void Rotate(float degree, Space relativeTo = Space.Self)
    {
        transform.Rotate(new Vector3(0, -degree, 0), relativeTo);
    }

    public void RotateAround(Vector2 point, float angle) {
        transform.RotateAround(CastVector2Dto3D(point), Vector3.up, angle);
    }

    public Vector2 TransformPoint(Vector2 localPoint) // this local 좌표계 있는 point를 global 좌표계로 변환
    {
        Vector3 castedPoint = Utility.CastVector2Dto3D(localPoint);
        return Utility.CastVector3Dto2D(transform.TransformPoint(castedPoint));
    }

    public Vector2 InverseTransformPoint(Vector2 globalPoint) // this global 좌표계 있는 point를 this local 좌표계로 변환
    {
        Vector3 castedPoint = Utility.CastVector2Dto3D(globalPoint);
        return Utility.CastVector3Dto2D(transform.InverseTransformPoint(castedPoint));
    }

    public Vector2 TransformPointToOtherLocal(Vector2 localPoint, Transform2D other) // this local 좌표계 있는 point를 other local 좌표계로 변환
    {
        return other.InverseTransformPoint(this.TransformPoint(localPoint));
    }

    public override string ToString()
    {
        return string.Format("position: {0}, rotation: {1}, localscale: {2}\n" +
            "localPosition: {3}, localRotation: {4}, localScale: {5}, " +
            "forward: {6}", Position, Rotation, LocalScale, LocalPosition, LocalRotation, LocalScale, Forward);
    }
}