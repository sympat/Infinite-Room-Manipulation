using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInitializable {
    void Initializing();
}

public static class ObjectExtensions
{
    public static T Initializing<T>(this GameObject original) where T : MonoBehaviour, IInitializable {
        T result = GameObject.Instantiate(original).GetComponent<T>();
        result.Initializing();
        return result;
    }

    public static T Initializing<T>(this GameObject original, Transform parent) where T : MonoBehaviour, IInitializable {
        T result = GameObject.Instantiate(original, parent).GetComponent<T>();
        result.Initializing();
        return result;
    }

    public static T Initializing<T>(this GameObject original, Transform parent, bool instantiateInWorldSpace) where T : MonoBehaviour, IInitializable {
        T result = GameObject.Instantiate(original, parent, instantiateInWorldSpace).GetComponent<T>();
        result.Initializing();
        return result;
    }

    public static T Initializing<T>(this GameObject original, Vector3 position, Quaternion rotation) where T : MonoBehaviour, IInitializable {
        T result = GameObject.Instantiate(original, position, rotation).GetComponent<T>();
        result.Initializing();
        return result;
    }

    public static T Initializing<T>(this GameObject original, Vector3 position, Quaternion rotation, Transform parent) where T : MonoBehaviour, IInitializable {
        T result = GameObject.Instantiate(original, position, rotation, parent).GetComponent<T>();
        result.Initializing();
        return result;
    }
}
