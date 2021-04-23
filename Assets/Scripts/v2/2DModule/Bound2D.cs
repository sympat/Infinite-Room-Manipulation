using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bound2D : Transform2D
{
    public bool useScaleAsSize = true;
    public Vector2 initSize;
    public float height;

    protected static int totalID = 0;
    protected int id;
    protected BoxCollider box;
    protected Vector3 originScale;

    public int ID {
        get {
            return id;
        }
    }

    public Vector2 Size
    {
        get
        {
            return Utility.CastVector3Dto2D(box.size);
        }

        set
        {
            UpdateBox(value, Height);
        }
    }

    public float Height
    {
        get
        {
            return box.size.y;
        }

        set
        {
            UpdateBox(Size, value);
        }
    }

    public Vector2 Max
    {
        get
        {

            return Utility.CastVector3Dto2D(box.size / 2) + this.Position;
        }
    }

    public Vector2 Min
    {
        get
        {
            return Utility.CastVector3Dto2D(-box.size / 2) + this.Position;

        }
    }

    public Vector2 Localmax {
        get {
            return Utility.CastVector3Dto2D(box.center + box.size / 2);
        }
    }

    public Vector2 Localmin {
        get
        {
            return Utility.CastVector3Dto2D(box.center - box.size / 2);

        }
    }

    public Vector2 Extents
    {
        get
        {
            return Utility.CastVector3Dto2D(box.size / 2);
        }
    }

    public override void Initializing()
    {
        // Debug.Log("Initialzing room base");
        id = totalID++;

        box = GetComponent<BoxCollider>();
        if(box == null) box = this.gameObject.AddComponent<BoxCollider>();

        ApplySize();
    }

    protected void ApplySize() {
        if(useScaleAsSize) {
            box.size = Vector3.Scale(box.size, transform.localScale);
            originScale= transform.localScale;
            transform.localScale = Vector3.one;
            this.Size = CastVector3Dto2D(box.size);
            this.Height = box.size.y;
        }
        else {
            originScale= transform.localScale;
            transform.localScale = Vector3.one;
            this.Size = initSize;
            this.Height = height;
        }
    }

    protected virtual void UpdateBox(Vector2 size, float height) {
        // update collider
        box.size = CastVector2Dto3D(size, height);
        box.center = new Vector3(box.center.x, height / 2, box.center.z);
    }
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
        if (this.box.bounds.Contains(Utility.CastVector2Dto3D(point, this.Height / 2)))
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

    public int GetContactEdge(Room other) {
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
                result = (relativeTo == Space.World) ? new Vector2(0, this.Extents.y) + this.Position : new Vector2(0, this.Extents.y);
                break;
            case 1:
                result = (relativeTo == Space.World) ? new Vector2(-this.Extents.x, 0) + this.Position : new Vector2(-this.Extents.x, 0);
                break;
            case 2:
                result = (relativeTo == Space.World) ? new Vector2(0, -this.Extents.y) + this.Position : new Vector2(0, -this.Extents.y);
                break;
            case 3:
                result = (relativeTo == Space.World) ? new Vector2(this.Extents.x, 0) + this.Position : new Vector2(this.Extents.x, 0);
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
                result = (relativeTo == Space.World) ? new Vector2(-this.Extents.x, this.Extents.y) + this.Position : new Vector2(-this.Extents.x, this.Extents.y);
                break;
            case 2:
                result = (relativeTo == Space.World) ? this.Min : this.Localmin;
                break;
            case 3:
                result = (relativeTo == Space.World) ? new Vector2(this.Extents.x, -this.Extents.y) + this.Position : new Vector2(this.Extents.x, -this.Extents.y);
                break;
            default:
                throw new System.NotImplementedException();
        }

        return result;
    }

    public Vector3 GetVertex3D(int index, float height = 0, Space relativeTo = Space.Self)
    {
        Vector2 result = GetVertex2D(index, relativeTo);
        return Utility.CastVector2Dto3D(result, height);
    }

    public Vector2 SamplingPosition(float bound = 0)
    {
        float xSampling = Random.Range(this.Min.x + bound, this.Max.x - bound);
        float ySampling = Random.Range(this.Min.y + bound, this.Max.y - bound);

        return new Vector2(xSampling, ySampling);
    }

    public Vector2 DenormalizePosition2D(Vector2 normalizedPos) {
        float xPos = (normalizedPos.x + 1) * (Max.x - Min.x) / 2 + Min.x;
        float yPos = (normalizedPos.y + 1) * (Max.y - Min.y) / 2 + Min.y;

        return new Vector2(xPos, yPos);
    }

    public Vector3 DenormalizePosition3D(Vector2 normalizedPos, float height = 0) {
        Vector2 result = DenormalizePosition2D(normalizedPos);
        return Utility.CastVector2Dto3D(result, height);
    }

    public void MoveEdge(int index, float translate) // box 형태를 유지하기 위해 wall의 1차원 움직임만 허용 (translate 부호 기준은 2차원 좌표계)
    {
        int realIndex = Utility.mod(index, 4);
        float newCenterX = this.Position.x,
            newCenterY = this.Position.y,
            newSizeX = this.Size.x,
            newSizeY = this.Size.y;

        if (realIndex == 0) // N (+y)
        {
            newCenterY = this.Position.y + translate / 2;
            newSizeY = this.Size.y + translate;
        }
        else if (realIndex == 1) // W (-x)
        {
            newCenterX = this.Position.x + translate / 2;
            newSizeX = this.Size.x - translate;
        }
        else if (realIndex == 2) // S (-y)
        {
            newCenterY = this.Position.y + translate / 2;
            newSizeY = this.Size.y - translate;
        }
        else if (realIndex == 3) // E (+x)
        {
            newCenterX = this.Position.x + translate / 2;
            newSizeX = this.Size.x + translate;
        }
        else
        {
            throw new System.NotImplementedException();
        }

        this.Position = new Vector2(newCenterX, newCenterY);
        this.Size = new Vector2(newSizeX, newSizeY);
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Room objAsRoom = obj as Room;
        if (objAsRoom == null) return false;
        else return Equals(objAsRoom);
    }

    public override int GetHashCode()
    {
        return this.id;
    }

    public bool Equals(Room v)
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
