using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Room : Bound2D
{
    private static int totalID = 0;
    private int roomID;

    private Vector2 originSize;
    [HideInInspector]
    public bool isSmallerInX = false;
    [HideInInspector]
    public bool isSmallerInY = false;
    [HideInInspector]
    public Room previousRoom;
    public Vector2 OriginSize {
        get {
            return originSize;
        }
    }

    public override void Initializing()
    {
        base.Initializing();
        // Debug.Log("Initialzing room " + this.gameObject.name);

        originSize = this.Size;

        this.gameObject.layer = LayerMask.NameToLayer("Room");

        // transform.GetChild(0).position = new Vector3(transform.GetChild(0).position.x, Height, transform.GetChild(0).position.z); // 전등
        // if(transform.childCount > 1) transform.GetChild(1).localScale = Vector3.Scale(transform.GetChild(1).localScale, originScale); // Teleport
    }

    protected override void UpdateBox(Vector2 size, float height) {
        base.UpdateBox(size, height);

        transform.GetChild(0).position = new Vector3(transform.GetChild(0).position.x, Height, transform.GetChild(0).position.z); // 전등
        if(transform.childCount > 1) transform.GetChild(1).localScale = Vector3.Scale(transform.GetChild(1).localScale, originScale); // Teleport


        // update mesh
        UpdateMesh();
    }

    private void UpdateMesh() {

        Mesh mesh = new Mesh();
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

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.uv2 = GetComponent<MeshFilter>().sharedMesh.uv2;

        GetComponent<MeshFilter>().mesh = mesh;
        
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

}
