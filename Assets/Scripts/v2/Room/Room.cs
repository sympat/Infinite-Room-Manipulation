using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using System.Linq;

public class Room : Bound2D
{

    [HideInInspector]
    public Room previousRoom;

    private Vector2 prevPosition;
    private float prevHeight;
    private Vector2 prevSize;

    public override void Initializing()
    {
        base.Initializing();
        this.gameObject.layer = LayerMask.NameToLayer("Room");
    }

    protected override void UpdateBox(Vector2 size, float height) {
        prevPosition = this.Position;
        prevSize = this.Size;
        prevHeight = this.Height;

        base.UpdateBox(size, height);

        // update furniture
        UpdateFurnitures();

        // update mesh
        UpdateMesh();
    }

    public void ToggleTeleportArea(bool enabled) {
        TeleportArea tArea = GetComponentInChildren<TeleportArea>();

        if(tArea != null)
            tArea.enabled = enabled;
    }

    private void UpdateFurnitures() {
        foreach(Transform child in transform) {

            Transform2D tf = child.GetComponent<Transform2D>();
            if(tf is Furniture) {
                Furniture furniture = tf as Furniture;

                if(furniture.isMovable) {
                    furniture.LocalPosition = Vector2.Scale(this.Size / prevSize, furniture.LocalPosition) + (this.Position - prevPosition);
                    if(furniture.allowZAxis) furniture.transform.localPosition = new Vector3(furniture.LocalPosition.x, this.Height / prevHeight * furniture.transform.localPosition.y, furniture.LocalPosition.y);
                }
                if(furniture.isScalable) {
                    furniture.LocalScale = Vector2.Scale(this.Size / prevSize, furniture.LocalScale);
                    if(furniture.allowZAxis) furniture.transform.localScale = new Vector3(furniture.LocalScale.x, this.Height / prevHeight * furniture.transform.localScale.y, furniture.LocalScale.y);
                }
            }
        }
    }

    private void UpdateMesh() {

        Vector3[] vertices = new Vector3[24];
        int[,] vertexIndex = new int[,] {
            {0,15,19}, // 0, -
            {11,12,18}, // 1, - 
            {7,8,17}, // 2, -
            {3,4,16}, // 3, -
            {1,14,22}, // 0, +
            {10,13,23}, // 1, +
            {6,9,20}, // 2, +
            {2,5,21}, // 3, +
        };

        for(int i=0; i<vertexIndex.GetLength(0); i++) {
            switch((i%4)) {
                case 0:
                    for(int j=0; j<vertexIndex.GetLength(1); j++) 
                        vertices[vertexIndex[i,j]] = (i / 4 == 0) ? GetVertex3D(0) : GetVertex3D(0, this.Height);
                    break;
                case 1:
                    for(int j=0; j<vertexIndex.GetLength(1); j++) 
                        vertices[vertexIndex[i,j]] = (i / 4 == 0) ? GetVertex3D(1) : GetVertex3D(1, this.Height);
                    break;
                case 2:
                    for(int j=0; j<vertexIndex.GetLength(1); j++) 
                        vertices[vertexIndex[i,j]] = (i / 4 == 0) ? GetVertex3D(2) : GetVertex3D(2, this.Height);
                    break;
                case 3:
                    for(int j=0; j<vertexIndex.GetLength(1); j++) 
                        vertices[vertexIndex[i,j]] = (i / 4 == 0) ? GetVertex3D(3) : GetVertex3D(3, this.Height);
                    break;
                default:
                    throw new System.Exception("Invalid Vertex Index");
            }
        }

        int[] triangles = new int[] // index 지정
        {
            0,1,2,0,2,3,  // right 
            4,5,6,4,6,7, // front
            8,9,10,8,10,11, // left
            12,13,14,12,14,15, // back
            16,17,18,16,18,19, // bottom
            20,21,22,20,22,23, // top
        };

        Vector3[] normals = new Vector3[24];
        int[,] normalVertex = new int[,] {
            {0,1,2,3}, // right 
            {4,5,6,7}, // front 
            {8,9,10,11}, // left
            {12,13,14,15}, // back
            {16,17,18,19}, // bottom
            {20,21,22,23}, // top
        };

        for(int i=0; i<normalVertex.GetLength(0); i++) {
            switch((i)) {
                case 0:
                    for(int j=0; j<normalVertex.GetLength(1); j++) 
                        normals[normalVertex[i,j]] = new Vector3(-1,0,0);
                    break;
                case 1:
                    for(int j=0; j<normalVertex.GetLength(1); j++) 
                        normals[normalVertex[i,j]] = new Vector3(0,0,1);
                    break;
                case 2:
                    for(int j=0; j<normalVertex.GetLength(1); j++) 
                        normals[normalVertex[i,j]] = new Vector3(1,0,0);
                    break;
                case 3:
                    for(int j=0; j<normalVertex.GetLength(1); j++) 
                        normals[normalVertex[i,j]] = new Vector3(0,0,-1);
                    break;
                case 4:
                    for(int j=0; j<normalVertex.GetLength(1); j++) 
                        normals[normalVertex[i,j]] = new Vector3(0,1,0);
                    break;
                case 5:
                    for(int j=0; j<normalVertex.GetLength(1); j++) 
                        normals[normalVertex[i,j]] = new Vector3(0,-1,0);
                    break;
                default:
                    throw new System.Exception("Invalid Vertex Index");
            }
        }

        Vector2[] uvs = new Vector2[] {
            Vector2.zero, Vector2.up, Vector2.one, Vector2.right,
            Vector2.zero, Vector2.up, Vector2.one, Vector2.right,
            Vector2.zero, Vector2.up, Vector2.one, Vector2.right,
            Vector2.zero, Vector2.up, Vector2.one, Vector2.right,
            Vector2.right, Vector2.zero, Vector2.up, Vector2.one,
            Vector2.zero, Vector2.right, Vector2.one, Vector2.up,
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;
        if(GetComponent<MeshFilter>().sharedMesh != null) mesh.uv2 = GetComponent<MeshFilter>().sharedMesh.uv2; // for lightmap

        GetComponent<MeshFilter>().mesh = mesh;

        
    }

    public void MoveEdge(int index, float translate) // box 형태를 유지하기 위해 wall의 1차원 움직임만 허용 (translate 부호 기준은 2차원 좌표계)
    {
        int realIndex = Utility.mod(index, 4);

        if (realIndex == 0) // N (+y)
        {
            this.Size = this.Size + Vector2.up * translate;
            this.Position = this.Position + this.Forward * translate / 2;
        }
        else if (realIndex == 1) // W (-x)
        {
            this.Size = this.Size + (-Vector2.right) * translate;
            this.Position = this.Position + (this.Right) * translate / 2;

        }
        else if (realIndex == 2) // S (-y)
        {
            this.Size = this.Size + (-Vector2.up) * translate;
            this.Position = this.Position + (this.Forward) * translate / 2;
        }
        else if (realIndex == 3) // E (+x)
        {
            this.Size = this.Size + Vector2.right * translate;
            this.Position = this.Position + this.Right * translate / 2;
        }
        else
        {
            throw new System.NotImplementedException();
        }
    }
}
