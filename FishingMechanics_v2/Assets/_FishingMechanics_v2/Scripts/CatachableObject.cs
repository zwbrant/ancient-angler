using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CatachableObject : MonoBehaviour {

    public bool IsCaught { get; private set; }

    internal Rigidbody Rbody;

    private void Awake()
    {
        Rbody = GetComponent<Rigidbody>();
    }


}
