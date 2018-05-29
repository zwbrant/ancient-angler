using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingReel : MonoBehaviour {
    public float LineReleaseThreshold;

    [Header("Component Dependencies")]
    public FishingPole Pole;

    public bool IsBailOpen { get; private set; }

    private void Awake()
    {
        Pole = UnityUtilities.TryResolveDependency(Pole, transform.parent.gameObject);

        IsBailOpen = false;
    }

    private void FixedUpdate()
    {
        
    }

    public void ReelIn(float meters)
    {
        Pole.Line.BaseLine.RemoveLine(meters);
    }

    public void ReelOut(float meters)
    {
        Pole.Line.BaseLine.AddLine(meters);
    }

    public void OpenBail()
    {
        IsBailOpen = true;
        Pole.Line.BaseLine.SetLinePullSpawn(true, LineReleaseThreshold);
    }

    public void CloseBail()
    {
        IsBailOpen = false;
        Pole.Line.BaseLine.SetLinePullSpawn(false);
    }
}
