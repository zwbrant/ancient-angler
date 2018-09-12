using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRTK;

public class ZB_VerletLine : MonoBehaviour {
    VerletLineInputEvents controllerInputEvents;
    public Transform lineStart;
    public Transform lineEnd;
    internal Rigidbody lineStartRBody;
    internal Rigidbody lineEndRBody;
    

    internal ZB_Reel reel;

    public int lineSegments;
    internal float segmentLength;
    internal float totalLength;
    internal bool hooked;
    internal float hookStretchLength = -1f;

    // main particles that compose line
    List<ZB_VerletParticle> lineParticles = new List<ZB_VerletParticle>();
    LineRenderer lineRenderer;

    public float stretchLimit;
    [Range(0, 1)]
    public float drag;
    [Range(-.1f, .1f)]
    public float gravity;
    [Range(-.1f, .1f)]
    public float underwaterGravity;
    [Range(0, 4000)]
    public float elasticity = 300f;
    [Range(1, 50)]
    public int physicsPerFrame = 3;
    [Range(0, .5f)]
    public float lineReleaseResistance = .05f;

    private bool releaseLine;
    public bool ReleaseLine { get { return releaseLine;  }
        set {

            //avoidHookReactions = value;
            releaseLine = value;
        } }


    void Start () {
        hooked = false;
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

        //foreach (ZB_VerletParticle particle in lineParticles)
        //{
        //    particle.WeirdUpdate(Time.deltaTime);
        //}

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
        }

        if (removeSegment)
        {
            RemoveSegment();
        }

        //if (ReleaseLine)
        //    AddSegment(lineEnd.position);

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
    private void ConstrainTransformAndParticle(Transform transform, ZB_VerletParticle particle)
    {
        //float stretchedLength = segmentLength + stretchLimit;

        Vector3 vectorDelta = transform.position - particle.Position;
        float deltaLength = vectorDelta.magnitude;
        hookStretchLength = deltaLength - segmentLength;

        if (deltaLength > segmentLength)
        {

            float diff = (deltaLength - segmentLength) / deltaLength;

            particle.Position += vectorDelta * diff * 0.5f;
            if (!ReleaseLine && !avoidHookReactions)
            {
                // elasticity = 35
                // mass = 3
                // drag = 1.34
                // angular drag = 2.5
                // forceMode = velocityChange

                hookDelta = vectorDelta * diff * 0.5f;
                transform.position -= hookDelta;


                //if (!hooked)
                    transform.gameObject.GetComponent<Rigidbody>().AddForce(-vectorDelta * diff * 0.5f * elasticity, ForceMode.VelocityChange);
            } else if (ReleaseLine && totalLength < Vector3.Distance(lineStart.position, lineEnd.position))
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
                    else {
                        reel.releaseSpeed = reel.initialReleaseSpeed;
                    }
                }
            } else if (avoidHookReactions && !hooked)
            {
                transform.gameObject.GetComponent<Rigidbody>().AddForce(-vectorDelta * diff * 0.01f * elasticity, ForceMode.Force);
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


