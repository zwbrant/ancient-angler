using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityUtilities {
    /// <summary>
    /// Attempts to resolve references to needed components. 
    /// If the provided component is not null, it will be returned, otherwise
    /// this method will attempt to find a component of that type on the given gameobject and its children
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    /// <param name="neededComponent"></param>
    /// <param name="dependentComponent"></param>
    public static TComponent TryResolveDependency<TComponent>(TComponent neededComponent, GameObject parentObject)
        where TComponent : Behaviour
    {
        if (neededComponent != null)
        {
            return neededComponent;
        }

        neededComponent = parentObject.GetComponent<TComponent>();

        if (neededComponent == null)
        {
            neededComponent = parentObject.GetComponentInChildren<TComponent>();
        }

        if (neededComponent != null)
        {
            return neededComponent;
        }
        else
        {
            Debug.LogException(new System.NullReferenceException());
            return null;
        }
    }

    public static TComponent TryResolveDependencyInScene<TComponent>(TComponent neededComponent, GameObject parentObject)
    where TComponent : Behaviour
    {
        if (neededComponent != null)
        {
            return neededComponent;
        }

        neededComponent = parentObject.GetComponent<TComponent>();

        if (neededComponent == null)
        {
            neededComponent = parentObject.GetComponentInChildren<TComponent>();
        }

        if (neededComponent == null)
        {
            neededComponent = GameObject.FindObjectOfType<TComponent>();
        }

        if (neededComponent != null)
        {
            return neededComponent;
        }
        else
        {
            Debug.LogException(new System.NullReferenceException());
            return null;
        }
    }
}
