using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class FishingPole : MonoBehaviour {
    [Header("Component Dependencies")]
    public FishingLine Line;
    public FishingPoleAudio PoleAudio;
    public FishingPole_InteractObj InteractableObj;
    [Header("Parameters")]
    public float ForceForMaxHaptic = 200;
    public float ForceForMaxHapticWithFish = 100;
    public float ForceForMaxStretchVolume = 100;

    public Fish AttachedFish { get; private set; }

    private void Awake()
    {
        Line = UnityUtilities.TryResolveDependency(Line, gameObject);
        PoleAudio = UnityUtilities.TryResolveDependency(PoleAudio, gameObject);
        InteractableObj = UnityUtilities.TryResolveDependency(InteractableObj, gameObject);
    }

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        float currentPullForce = Line.CurrentEndpointAttachmentForce.magnitude;

        if (AttachedFish != null)
        {
            // line stretch audio
            PoleAudio.ModulateStretchSound(currentPullForce / ForceForMaxHapticWithFish);
        }

        // line haptic feedback
        if (InteractableObj.IsGrabbed())
            InteractableObj.HapticPulse((currentPullForce - 18f) / ForceForMaxHaptic);
    }

    public void AttachFish(Fish fish)
    {
        Line.AttachEndpoint(fish, fish.LineAttachmentPoint);
        PoleAudio.EnableLineStretchNoise();

        AttachedFish = fish;
    }

    public void DettachFish(bool snapSound)
    {
        if (snapSound)
            PoleAudio.PlaySnapSound();

        Line.DetachEndpoint();
        Line.EnableLure();
        PoleAudio.DisableLineStretchNoise();

        AttachedFish = null;
    }

}
