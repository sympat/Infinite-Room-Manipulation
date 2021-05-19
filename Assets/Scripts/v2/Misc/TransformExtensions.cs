using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    public static List<GameObject> FindObjectsWithTag(this Transform parent, string tag)
    {
        List<GameObject> taggedGameObjects = new List<GameObject>();

        foreach(Transform child in parent) {
            if (child.tag == tag)
            {
                taggedGameObjects.Add(child.gameObject);
            }
            if (child.childCount > 0)
            {
                taggedGameObjects.AddRange(FindObjectsWithTag(child, tag));
            }
        }

        return taggedGameObjects;
    }

    public static GameObject FindObjectWithTag(this Transform parent, string tag)
    {
        GameObject taggedGameObject = null;

        foreach(Transform child in parent) {
            
            if(child.tag == tag) {
                taggedGameObject = child.gameObject;
                break;
            }
            if(child.childCount > 0) {
                taggedGameObject = FindObjectWithTag(child, tag);
            }
        }

        return taggedGameObject;
    }

    public static List<GameObject> FindObjectsWithLayer(this Transform parent, string layerName)
    {
        List<GameObject> layerObjects = new List<GameObject>();

        foreach(Transform child in parent) {
            if (child.gameObject.layer == LayerMask.NameToLayer(layerName))
            {
                layerObjects.Add(child.gameObject);
            }
            if (child.childCount > 0)
            {
                layerObjects.AddRange(FindObjectsWithLayer(child, layerName));
            }
        }

        return layerObjects;
    }

    public static GameObject FindObjectWithLayer(this Transform parent, string layerName) {

        GameObject layerObject = null;

        foreach(Transform child in parent)
        {
            if(child.gameObject.layer == LayerMask.NameToLayer(layerName)) {
                layerObject = child.gameObject;
                break;
            }
            if(child.childCount > 0) {
                layerObject = FindObjectWithLayer(child, layerName);
            }

        }

        return layerObject;
    }

    public static List<T> FindComponents<T>(this Transform parent, bool allowRecursive = false) {
        List<T> components = new List<T>();

        foreach(Transform child in parent) {
            T thisComponent = child.GetComponent<T>();

            if(thisComponent != null) {
                components.Add(thisComponent);
            }
            if(allowRecursive && child.childCount > 0) {
                components.AddRange(FindComponents<T>(child, allowRecursive));
            }
        }

        return components;
    }

    public static T FindComponent<T>(this Transform parent, bool allowRecursive = false) {
        T component = default(T);

        foreach(Transform child in parent) {
            T thisComponent = child.GetComponent<T>();

            if(thisComponent != null) {
                component = thisComponent;
                break;
            }
            if(allowRecursive && child.childCount > 0) {
                component = FindComponent<T>(child, allowRecursive);
            }
        }

        return component;

    }

}
