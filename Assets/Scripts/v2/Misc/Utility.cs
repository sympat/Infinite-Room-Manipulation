using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public static class Utility
{
    public static GameObject GetChildWithLayer(this GameObject parent, string layer) {
           Transform t = parent.transform;
           foreach(Transform tr in t)
           {
                  if(tr.gameObject.layer == LayerMask.NameToLayer(layer))
                  {
                       return tr.gameObject;
                  }
           }
           return null;
    }

    public static List<GameObject> GetChildrenWithLayer(this GameObject parent, string layer) {
            List<GameObject> result = new List<GameObject>();
            Transform t = parent.transform;
           
            foreach(Transform tr in t)
            {
                if(tr.gameObject.layer == LayerMask.NameToLayer(layer))
                {
                    result.Add(tr.gameObject);
                }
            }
            
            return result;
    }

    public static int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public static Vector3 ProjectVector3Dto2D(Vector3 vec3) {
        int significantDigit = 5;
        float significant = Mathf.Pow(10, significantDigit);

        float xValue = Mathf.Floor(vec3.x * significant) / significant; // 0.6666667 이면 0.666666 으로 버림
        float zValue = Mathf.Floor(vec3.z * significant) / significant;

        return new Vector3(xValue, 0, zValue);
    }

    public static Quaternion ProjectRotation3Dto2D(Quaternion rotation) {
        return Quaternion.Euler(0, rotation.eulerAngles.y, 0);
    }

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

    public static Vector2 RotateVector2(Vector2 vec, float degree)
    {
        Vector2 rotated = CastVector3Dto2D(CastRotation2Dto3D(degree) * CastVector2Dto3D(vec));
        return rotated;
    }

    public static float GetCCWAngle(Vector2 prevDir, Vector2 currDir)
    {
        float angle = Vector2.SignedAngle(prevDir, currDir);

        if (angle < 0)
            return 360.0f - Mathf.Abs(angle);
        else
            return angle;
    }

    public static Vector2[] ProjectionVertices(Vector3[] vertices)
    {
        Vector2[] vertices2D = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices2D[i] = Utility.CastVector3Dto2D(vertices[i]);
        }

        return vertices2D;
    }

    public static Vector2[] GetConnectedVertexFromTriangles(Vector2[] vertices, int[] triangle)
    {
        Vector2[] test = new Vector2[3];

        for (int i = 0; i < triangle.Length; i++)
            test[i] = vertices[triangle[i]];

        return test.Distinct().ToArray();
    }

    public static float sampleUniform(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public static int sampleUniform(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public static float sampleNormal(float mu = 0, float sigma = 1, float min = float.MinValue, float max = float.MaxValue)
    {
        // From: http://stackoverflow.com/questions/218060/random-gaussian-variables
        float r1 = UnityEngine.Random.value;
        float r2 = UnityEngine.Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(r1)) * Mathf.Sin(2.0f * Mathf.PI * r2); // Random Normal(0, 1)
        float randNormal = mu + randStdNormal * sigma;
        return Mathf.Max(Mathf.Min(randNormal, max), min);
    }

    public static int[] sampleWithoutReplacement(int sampleSize, int min, int max) {  // TODO: O(n^2) 보다 최적화 작업 진행
        int[] result = new int[sampleSize];
        bool equalFlag;

        for(int i=0; i<sampleSize; i++) {
            while(true) {
                result[i] = sampleUniform(min, max);
                equalFlag = false;

                for(int j=0; j<i; j++) {
                    if(result[i] == result[j]) {
                        equalFlag = true;
                        break;
                    }
                }
                
                if(!equalFlag) break;
            }
        }

        return result;
    }

    public static float GetSignedAngle(Vector3 prevDir, Vector3 currDir)
    {
        return Mathf.Sign(Vector3.Cross(prevDir, currDir).y) * Vector3.Angle(prevDir, currDir);
    }
}
