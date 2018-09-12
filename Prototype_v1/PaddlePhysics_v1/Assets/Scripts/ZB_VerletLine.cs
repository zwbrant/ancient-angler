using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRTK;
using System;

public class ZB_VerletLine : MonoBehaviour {
    VerletLineInputEvents controllerInputEvents;
    public Transform lineStart;
    public Transform lineEnd;
    public Rigidbody Attachment;
    internal Rigidbody lineStartRBody;
    internal Rigidbody lineEndRBody;
    
    internal ZB_Reel reel;
    internal ZB_FishResistance_v1 fishResistance;

    public int lineSegments;
    internal float segmentLength;
    internal float totalLength;
    internal bool hooked;
    internal float hookStretchLength = -1f;

    public delegate void LineBreakHandler(object sender, EventArgs e);
    public event LineBreakHandler LineBreak;

    // main particles that compose line
    List<ZB_VerletParticle> lineParticles = new List<ZB_VerletParticle>();
    LineRenderer lineRenderer;

    public ZB_LineAudio lineAudio;

    [Range(0, 1)]
    public float drag;
    [Range(-.1f, .1f)]
    public float gravity;
    [Range(-.1f, .1f)]
    public float underwaterGravity;
    [Range(0, 4000)]
    public float elasticity = 300f;
    [Range(1, 50)]
    public float castMultiplier = 3f;
    [Range(1, 50)]
    public int physicsPerFrame = 3;
    [Range(0, .5f)]
    public float lineReleaseResistance = .05f;
    [Range(0, 5f)]
    public float stretchLimit = 1f;

    private bool releaseLine;
    public bool ReleaseLine { get { return releaseLine;  }
        set {
            //avoidHookReactions = value;
            releaseLine = value;
        } }


    void Start () {
        hooked = (Attachment == null) ? false : true;

        controllerInputEvents = new VerletLineInputEvents(this);
        StartCoroutine(controllerInputEvents.InitEvents());
        
        ReleaseLine = false;

        Vector3 differenceVect =  lineEnd.position - lineStart.position;
        Vector3 segmentVect = differenceVect / lineSegments;
        segmentLength = segmentVect.magnitude;

        // start --> x-o-o-o-o-o-x <-- end
        Vector3 segmentSum = segmentVect;
        for (int i = 1; i < lineSegments; i++)
        {

            lineParticles.Add(new ZB_VerletParticle(lineStart.position + segmentSum, false));

            segmentSum += segmentVect;
        }
        lineStartRBody = lineStart.GetComponent<Rigidbody>();
        lineEndRBody = lineEnd.GetComponent<Rigidbody>();
        InitLineRenderer();
	}

    void InitLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetVertexCount(lineSegments + 1);
    }

    public bool removeSegment = false;
	void Update () {
        for (int z = 0; z < physicsPerFrame; z++)
        {

            foreach (ZB_VerletParticle particle in lineParticles)
            {
                Verlet(particle);
            }

            ConstrainTransformAndParticle(lineStart, lineParticles[0]);
            for (int i = 0; i < lineParticles.Count - 1; i++)
            {
                ConstrainParticles(lineParticles[i], lineParticles[i + 1]);
            }
            ConstrainParticleAndTransform(lineParticles[lineParticles.Count - 1], lineEnd, ReleaseLine);
            //ConstrainParticles(lineParticles[0], lineParticles[1], segmentLength);

            if (lineAudio != null)
            {
                if (hooked)
                {
                    if (!lineAudio.passiveSrc.isPlaying)
                        lineAudio.passiveSrc.Play();
                    ConstrainAttachmentAndTransform(Attachment, lineStart);
                }
                else
                {
                    lineAudio.passiveSrc.Stop();
                }
            }
        }

        if (removeSegment)
        {
            RemoveSegment();
        }

        lineRenderer.SetPositions(GetArrayOfVectors());
	}


    private void Verlet(ZB_VerletParticle particle)
    {
        float deltaTime = Time.deltaTime;
        float deltaTimeSq = deltaTime * deltaTime;
       
        Vector3 tempPos = particle.Position;
        Vector3 positionDelta = particle.Position - particle.OldPosistion;

        if (particle.Position.y > 0f)
        {
            particle.Position += ((positionDelta * (1f - drag * deltaTime) + new Vector3(0f, -gravity * deltaTime, 0f)) + ((particle.Acceleration) * deltaTimeSq));
        }
        else {
            //positionDelta.y = 0f;
            particle.Position += ((positionDelta * (1f - 1f * deltaTime) + new Vector3(0f, -underwaterGravity * deltaTime, 0f)) + ((particle.Acceleration) * deltaTimeSq));
        }
        particle.OldPosistion = tempPos;
    }


    public Vector3[] GetArrayOfVectors()
    {
        Vector3[] result = new Vector3[lineSegments + 1];

        result[0] = lineStart.position;
        // line particles is always lineSegments - 1, giving us room for the start and end
        for (int i = 0; i < lineParticles.Count; i++)
        {
            result[i + 1] = lineParticles[i].Position;
        }
        result[result.Length - 1] = lineEnd.position;

        return result;
    }


    private void ConstrainParticles(ZB_VerletParticle particleOne, ZB_VerletParticle particleTwo)
    {
        float restraintLength = segmentLength;

        Vector3 vectorDelta = particleOne.Position - particleTwo.Position;
        float deltaLength = vectorDelta.magnitude;
        if (deltaLength > restraintLength)
        {
            float diff = (deltaLength - restraintLength) / deltaLength;

            Vector3 change = vectorDelta * diff * 0.5f;

            particleOne.Position -= change;
            particleTwo.Position += change;
        }
    }

    internal Vector3 hookDelta = Vector3.zero;
    bool avoidHookReactions = false;
    // for the hook 
    private void ConstrainTransformAndParticle(Transform givenTransform, ZB_VerletParticle particle)
    {
        //float stretchedLength = segmentLength + stretchLimit;
        Vector3 vectorDelta = givenTransform.position - particle.Position;
        float deltaLength = vectorDelta.magnitude;
        hookStretchLength = deltaLength - segmentLength;

        if (deltaLength > segmentLength)
        {

            float stretchDeltaRatio = (deltaLength - segmentLength) / deltaLength;

            //particle.Position += vectorDelta * stretchDeltaRatio * 0.5f;
            particle.Position = Vector3.MoveTowards(particle.Position, givenTransform.position, hookStretchLength / 2f);
            if (!ReleaseLine && !avoidHookReactions)
            {
                // elasticity = 35
                // mass = 3
                // drag = 1.34
                // angular drag = 2.5
                // forceMode = velocityChange
                
                hookDelta = vectorDelta * stretchDeltaRatio * 0.5f;
                //transform.position -= hookDelta;
                givenTransform.position = Vector3.MoveTowards(givenTransform.position, particle.Position, hookStretchLength /  2f);
                //ZB_SceneSingletons.debugText1.text = "Hook stretch: " + Math.Round((Decimal)hookStretchLength, 5, MidpointRounding.AwayFromZero);

                //if (!hooked)
                if (!lineStartRBody.isKinematic)
                    givenTransform.gameObject.GetComponent<Rigidbody>().AddForce(-vectorDelta * stretchDeltaRatio * 0.5f * elasticity, ForceMode.VelocityChange);

            }
            else if (ReleaseLine && totalLength < Vector3.Distance(lineStart.position, lineEnd.position))
            {
                //transform.position -= vectorDelta * diff * 0.5f;
                //print(totalLength);
                //transform.gameObject.GetComponent<Rigidbody>().AddForce(-vectorDelta * diff * 0.2f * elasticity, ForceMode.Force);
                if (!hooked)
                {
                    float hookVelocity = lineStartRBody.velocity.magnitude;
                    if (hookVelocity > reel.initialReleaseSpeed * 2 + 1f)
                    { 
                        //print("Adding " + (hookVelocity - reel.initialReleaseSpeed) + " to release speed");
                        reel.releaseSpeed += hookVelocity - reel.initialReleaseSpeed;
                    }
                    else
                    {
                        reel.releaseSpeed = reel.initialReleaseSpeed;
                    }
                }
            } else if (avoidHookReactions && !hooked)
            {
                givenTransform.gameObject.GetComponent<Rigidbody>().AddForce(-vectorDelta * stretchDeltaRatio * castMultiplier * elasticity * Time.deltaTime, ForceMode.Force);
            }
        }
    }

    bool everyOther = true;
    private void ConstrainParticleAndTransform(ZB_VerletParticle particle, Transform transform, bool releaseLine)
    {
        float restraintLength = segmentLength;
        Vector3 vectorDelta = transform.position - particle.Position;
        float deltaLength = vectorDelta.magnitude;
        if (deltaLength > restraintLength)
        {
            float diff = (deltaLength - restraintLength) / deltaLength;
            particle.Position += vectorDelta * diff * .2f;

            float tipPullForce = (-vectorDelta * diff * 10f).magnitude;
            //transform.gameObject.GetComponent<Rigidbody>().AddForce(-vectorDelta * diff * 0.5f * elasticity);
        }
    }

    [Range(0, 100)]
    public float AttachStrength = 1f;
    private void ConstrainAttachmentAndTransform(Rigidbody attachment, Transform transform) 
    {
        Vector3 vectorDelta = attachment.position - transform.position;
        float stretchLength = vectorDelta.magnitude;

        if (stretchLength >= stretchLimit)
        {
            BreakLine();
        }

        ZB_SceneSingletons.debugText1.text = "Force: " + (-vectorDelta * AttachStrength).magnitude;

        attachment.AddForce(-vectorDelta * AttachStrength, ForceMode.Force);
        lineStartRBody.AddForce(vectorDelta * AttachStrength, ForceMode.Force);

        if (ZB_InteractableFishingRod.rodController != null)
        {
            float hapticStrength = (stretchLength) * (1500f / stretchLimit);
            ZB_InteractableFishingRod.rodController.TriggerHapticPulse((ushort)hapticStrength);
        }

        lineAudio.ModulateStretchSound(stretchLength, stretchLimit);
    }

    internal void BreakLine()
    {
        lineStartRBody.AddForce(-lineStartRBody.velocity * 1000f, ForceMode.Acceleration);
        if (ZB_InteractableFishingRod.rodController != null)
            ZB_InteractableFishingRod.rodController.TriggerHapticPulse(4000, .5f, .01f);
        lineAudio.PlaySnapSound();
        Attachment = null;
        hooked = false;
        if (LineBreak != null)
            LineBreak(this, EventArgs.Empty);
        return;
    }

    internal void StartHookReactionDelay() { StartCoroutine(DelayHookReactions()); }

    internal IEnumerator DelayHookReactions()
    {
        yield return new WaitForSeconds(.75f);
        avoidHookReactions = false;
    }

    internal void AddSegment(Vector3 spawnPoint)
    {
        lineSegments++;
        lineRenderer.SetVertexCount(lineSegments + 1);
        lineParticles.Add(new ZB_VerletParticle(spawnPoint, false));
        //lineParticles.Insert(lineParticles.Count - 1, new ZB_VerletParticle(spawnPoint, false));
        UpdateTotalLength();
    }

    internal void AddSegment()
    {
        Vector3 spawnPoint = lineParticles[lineParticles.Count - 1].Position + ((lineEnd.position - lineParticles[lineParticles.Count - 1].Position) * .5f);
        AddSegment(spawnPoint);
    }

    public void RemoveSegment()
    {
        if (lineParticles.Count > 1)
        {
            lineSegments--;
            lineRenderer.SetVertexCount(lineSegments + 1);
            lineParticles.RemoveAt(lineParticles.Count - 1);
            UpdateTotalLength();
        } 
    }

    internal void UpdateTotalLength()
    {
        totalLength = lineSegments * segmentLength;
    }

    void OnCollisionEnter(Collision collision)
    {
        avoidHookReactions = false;
    }

    void OnTriggerEnter(Collider other)
    {
        avoidHookReactions = false;
    }

    public class VerletLineInputEvents : ZB_ControllerInputEvents
    {
        ZB_VerletLine verletLine;

        public VerletLineInputEvents(ZB_VerletLine verletLine)
        {
            this.verletLine = verletLine;
        }

        public override void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
        {
            base.DoTriggerPressed(sender, e);
            verletLine.ReleaseLine = true;
            verletLine.avoidHookReactions = true;
        }

        public override void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
        {
            base.DoTriggerReleased(sender, e);
            verletLine.ReleaseLine = false;
            verletLine.StartHookReactionDelay();
        }

        public override void DoTouchPadPressed(object sender, ControllerInteractionEventArgs e)
        {
            base.DoTouchPadPressed(sender, e);
            verletLine.removeSegment = true;
        }

        public override void DoTouchpadReleased(object sender, ControllerInteractionEventArgs e)
        {
            base.DoTouchpadReleased(sender, e);
            verletLine.removeSegment = false;
        }

        public override void DoGripPressed(object sender, ControllerInteractionEventArgs e)
        {
            base.DoGripPressed(sender, e);
        }
    }

}


