using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))] 
public class LureInteractions : MonoBehaviour {
    public FishingPole ConnectedPole;
    public bool EnableInteractions = true;

    public Rigidbody Rbody { get; private set; }

    private void Awake()
    {
        Rbody = GetComponent<Rigidbody>();
        ConnectedPole = UnityUtilities.TryResolveDependency(ConnectedPole, this.gameObject);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (EnableInteractions)
        {
            var fish = collider.gameObject.GetComponent<Fish>();
            if (fish != null && fish.BiteLures)
                OnFishCollision(fish);
        }
    }

    public void OnFishCollision(Fish fish)
    {
        ConnectedPole.AttachFish(fish);
        fish.AttachHook(ConnectedPole);
    }

}
