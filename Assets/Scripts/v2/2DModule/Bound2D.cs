using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Bound2D : Transform2D
{

    public Vector2 initSize;
    public float initHeight;

    protected static int totalID = 0;
    protected int id;

    protected BoxCollider Box {
        get {
            return GetComponent<BoxCollider>();
        }
    }

    public int ID {
        get {
            return id;
        }
    }

    public Vector2 Size
    {
        get
        {
            return CastVector3Dto2D(Box.size);
        }

        set
        {
            UpdateBox(value, this.Height);
        }
    }

    public float Height
    {
        get
        {
            return Box.size.y;
        }

        set
        {
            UpdateBox(this.Size, value);
        }
    }

    public Vector2 Max
    {
        get
        {
            return TransformPoint(this.Localmax);
            // return CastVector3Dto2D(Box.size / 2) + this.Position;
        }
    }

    public Vector2 Min
    {
        get
        {
            return TransformPoint(this.Localmin);
            // return CastVector3Dto2D(-Box.size / 2) + this.Position;
        }
    }

    public Vector2 Localmax {
        get {
            return this.Size / 2;
            // return CastVector3Dto2D(Box.center + Box.size / 2);
        }
    }

    public Vector2 Localmin {
        get
        {
            return -this.Size / 2;
            // return CastVector3Dto2D(Box.center - Box.size / 2);

        }
    }

    public Vector2 Extents
    {
        get
        {
            return this.Size / 2;
            // return CastVector3Dto2D(Box.size / 2);
        }
    }

    public override void Initializing()
    {
        id = totalID++;

        ApplySize();
    }

    public void ApplySize() {

        // transform.localScale = Vector3.one; // ignore scale

        this.Size = initSize;
        this.Height = initHeight;
    }

    protected virtual void UpdateBox(Vector2 size, float height) {
        // update collider
        Box.size = CastVector2Dto3D(size, height);
        Box.center = new Vector3(Box.center.x, height / 2, Box.center.z);
    }

    // TODO: 두 개의 bound를 비교할려면 근본적으로 하나의 bound 좌표계로 변환이 필요.
    // 현재 구현은 두 bound가 항상 axis-aligned 되어있다는 가정하에, global max, min 값을 사용
    public bool IsInsideInXAxis(Bound2D other) // x축 기준으로 this가 other 안에 들어오는지 판단하는 함수
    {
        if ((other.Min.x - this.Min.x <= 0.05f) && (other.Max.x - this.Max.x >= 0.05f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsInsideInYAxis(Bound2D other) // y축 기준 this가 other 안에 들어오는지 판단하는 함수
    {
        if ((other.Min.y - this.Min.y <= 0.05f) && (other.Max.y - this.Max.y >= 0.05f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsInside(Bound2D other) // this가 other 안에 들어오는지 판단하는 함수
    {
        if (IsInsideInXAxis(other) && IsInsideInYAxis(other))
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public bool IsContain(Vector2 point) // this가 point를 포함하는지 판단하는 함수
    {
        if (this.Box.bounds.Contains(CastVector2Dto3D(point, this.Height / 2)))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsIntersectInXAxis(Bound2D other) // x축 기준으로 this 와 otherbox가 교차하는지 판단하는 함수
    {
        if((this.Min.x - other.Max.x < 0.01f) && (this.Max.x - other.Min.x > 0.01f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool IsIntersectInYAxis(Bound2D other) // y축 기준
    {
        if ((this.Min.y - other.Max.y < 0.01f) && (this.Max.y - other.Min.y > 0.01f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsIntersect(Bound2D other) // this와 point가 교차하는지 판단하는 함수
    {
        if (IsIntersectInXAxis(other) && IsIntersectInYAxis(other))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public int GetContactEdge(Bound2D other) {
        if(Mathf.Abs(this.Min.x - other.Max.x) < 0.01f) {
            if(IsIntersectInYAxis(other)) 
                return 1;
        }
        if(Mathf.Abs(this.Max.x - other.Min.x) < 0.01f) {
            if(IsIntersectInYAxis(other))
                return 3;
        }
        if(Mathf.Abs(this.Min.y - other.Max.y) < 0.01f) {
            if(IsIntersectInXAxis(other))
                return 2;
        }
        if(Mathf.Abs(this.Max.y - other.Min.y) < 0.01f) {
            if(IsIntersectInXAxis(other))
                return 0;
        }

        return -1;
    }

    public Vector2 GetEdge2D(int index, Space relativeTo = Space.Self)
    {
        int realIndex = Utility.mod(index, 4);
        Vector2 result;

        switch (realIndex)
        {
            case 0:
                result = (relativeTo == Space.World) ? TransformPoint(new Vector2(0, this.Extents.y)) : new Vector2(0, this.Extents.y);
                break;
            case 1:
                result = (relativeTo == Space.World) ? TransformPoint(new Vector2(-this.Extents.x, 0)) : new Vector2(-this.Extents.x, 0);
                break;
            case 2:
                result = (relativeTo == Space.World) ? TransformPoint(new Vector2(0, -this.Extents.y)) : new Vector2(0, -this.Extents.y);
                break;
            case 3:
                result = (relativeTo == Space.World) ? TransformPoint(new Vector2(this.Extents.x, 0)) : new Vector2(this.Extents.x, 0);
                break;
            default:
                throw new System.NotImplementedException();
        }

        return result;
    }

    public Vector3 GetEdge3D(int index, float height = 0, Space relativeTo = Space.Self) {
        Vector2 result = GetEdge2D(index, relativeTo);
        return CastVector2Dto3D(result, height);
    }

    public Vector2 GetVertex2D(int index, Space relativeTo = Space.Self) {
        int realIndex = Utility.mod(index, 4);
        Vector2 result;

        switch (realIndex)
        {   
            case 0:
                result = (relativeTo == Space.World) ? this.Max : this.Localmax;
                break;
            case 1:
                result = (relativeTo == Space.World) ? TransformPoint(new Vector2(-this.Extents.x, this.Extents.y)) : new Vector2(-this.Extents.x, this.Extents.y);
                break;
            case 2:
                result = (relativeTo == Space.World) ? this.Min : this.Localmin;
                break;
            case 3:
                result = (relativeTo == Space.World) ? TransformPoint(new Vector2(this.Extents.x, -this.Extents.y)) : new Vector2(this.Extents.x, -this.Extents.y);
                break;
            default:
                throw new System.NotImplementedException();
        }

        return result;
    }

    public Vector3 GetVertex3D(int index, float height = 0, Space relativeTo = Space.Self)
    {
        Vector2 result = GetVertex2D(index, relativeTo);
        return CastVector2Dto3D(result, height);
    }

    public Vector2 SamplingPosition(float bound = 0, Space relativeTo = Space.Self) 
    {
        float xSampling = Random.Range(this.Localmin.x + bound, this.Localmax.x - bound);
        float ySampling = Random.Range(this.Localmin.y + bound, this.Localmax.y - bound);

        return (relativeTo == Space.Self) ? new Vector2(xSampling, ySampling) : TransformPoint(new Vector2(xSampling, ySampling));
    }

    public Vector2 DenormalizePosition2D(Vector2 normalizedPos, Space relativeTo = Space.Self) { // output이 relativeTo 좌표계에 있다는 뜻
        float xPos = normalizedPos.x * Extents.x;
        float yPos = normalizedPos.y * Extents.y;

        return (relativeTo == Space.Self) ? new Vector2(xPos, yPos) : TransformPoint(new Vector2(xPos, yPos));
    }

    public Vector3 DenormalizePosition3D(Vector3 normalizedPos, Space relativeTo = Space.Self) {
        float xPos = normalizedPos.x * Extents.x;
        float yPos = normalizedPos.z * Extents.y;
        float height = normalizedPos.y * Height;

        return (relativeTo == Space.Self) ? new Vector3(xPos, height, yPos) : transform.TransformPoint(new Vector3(xPos, height, yPos));
    }

    public Vector2 NormalizedPosition2D(Vector2 pos, Space relativeTo = Space.Self) { // input(pos)이 relativeTo 좌표계에 있다는 뜻
        Vector2 diff = (relativeTo == Space.Self) ? (pos) : (InverseTransformPoint(pos));

        float xPos = diff.x / Extents.x;
        float yPos = diff.y / Extents.y;

        return new Vector2(xPos, yPos);
    }

    public Vector3 NormalizedPosition3D(Vector3 pos, Space relativeTo = Space.Self) { // relativeTo = pos가 어떤 좌표계에 있는지
        Vector3 diff = (relativeTo == Space.Self) ? (pos) : (transform.InverseTransformPoint(pos));

        float xPos = diff.x / Extents.x;
        float yPos = diff.z / Extents.y;
        float height = diff.y / Height;

        return new Vector3(xPos, height, yPos);
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Bound2D objAsRoom = obj as Bound2D;
        if (objAsRoom == null) return false;
        else return Equals(objAsRoom);
    }

    public override int GetHashCode()
    {
        return this.id;
    }

    public bool Equals(Bound2D v)
    {
        if (v == null) return false;
        else return (this.id == v.id);
    }

    public override string ToString()
    {
        string result = "";
        result += string.Format("ID: {0}, Size: {1}, Origin: {2}", id, this.Size, this.Position);
        result += string.Format(", ObjName: {0}", this.gameObject.name);

        return result;
    }
}
