using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour {
    public Vector3 LineAttachmentPoint = Vector3.zero;
    public RbodyFishResistance RbodyResistance;
    public bool BiteLures = true;
    public bool EnableHookBreaking = true;
    public float HookBreakForceThreshold = 50f;

    public FishingPole AttachedPole { get; private set; }

    private void Awake()
    {
        if (RbodyResistance == null) { RbodyResistance = GetComponent<RbodyFishResistance>(); }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (AttachedPole != null) {
            float cumulativeForce = AttachedPole.Line.CurrentEndpointAttachmentForce.magnitude + RbodyResistance.CurrentForce.magnitude;
            //print(Mathf.Round(cumulativeForce));

            // hook breaks
            if (EnableHookBreaking && cumulativeForce > HookBreakForceThreshold)
            {
                DetachHook();
            }
        }
	}

    public void AttachHook(FishingPole pole)
    {
        AttachedPole = pole;
        RbodyResistance.FishPositionTarget = pole.transform.forward * 30f;
        RbodyResistance.FishPositionTarget.y = transform.position.y;
        RbodyResistance.EnableAutoForce = true;

        // BITE HAPTIC
        pole.InteractableObj.HapticPulse(1f, 1f);
    }

    public void DetachHook()
    {
        RbodyResistance.EnableAutoForce = false;

        AttachedPole.DettachFish(true);

        AttachedPole = null;
        StartCoroutine(RejectLures(.5f));
    }

    public IEnumerator RejectLures(float forSeconds)
    {
        BiteLures = false;
        yield return new WaitForSecondsRealtime(forSeconds);
        BiteLures = true;
    }
}
