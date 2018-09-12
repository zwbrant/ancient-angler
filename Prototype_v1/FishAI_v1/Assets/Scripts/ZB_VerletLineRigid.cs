using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRTK;

public class ZB_VerletLineRigid : MonoBehaviour {
    public Transform lineStart;
    public Transform lineEnd;
    internal Rigidbody lineStartRBody;

    internal ZB_Reel reel;

    public int lineSegments;
    internal float segmentLength;
    internal float totalLength;

    // main particles that compose line
    List<ZB_VerletParticle> lineParticles = new List<ZB_VerletParticle>();
    LineRenderer lineRenderer;

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
    public bool ReleaseLine
    {
        get { return releaseLine; }
        set
        {

            //avoidHookReactions = value;
            releaseLine = value;
        }
    }


    void Start()
    {
        Vector3 differenceVect = lineEnd.position - lineStart.position;
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
        InitLineRenderer();
    }

    void InitLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetVertexCount(lineSegments + 1);
    }

    void Update()
    {

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

            ConstrainTransformAndParticle(lineStart, lineParticles[0], segmentLength);
            for (int i = 0; i < lineParticles.Count - 1; i++)
            {
                ConstrainParticles(lineParticles[i], lineParticles[i + 1], segmentLength);
            }
            ConstrainParticleAndTransform(lineParticles[lineParticles.Count - 1], lineEnd, segmentLength, ReleaseLine);
            //ConstrainParticles(lineParticles[0], lineParticles[1], segmentLength);
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


    private void ConstrainParticles(ZB_VerletParticle particleOne, ZB_VerletParticle particleTwo, float restraintLength)
    {
        Vector3 vectorDelta = particleOne.Position - particleTwo.Position;
        float deltaLength = vectorDelta.magnitude;

            float diff = (deltaLength - restraintLength) / deltaLength;

            Vector3 change = vectorDelta * diff * 0.5f;

            particleOne.Position -= change;
            particleTwo.Position += change;

    }

    bool avoidHookReactions = false;
    private void ConstrainTransformAndParticle(Transform transform, ZB_VerletParticle particle, float restraintLength)
    {
        Vector3 vectorDelta = transform.position - particle.Position;
        float deltaLength = vectorDelta.magnitude;


            float diff = (deltaLength - restraintLength) / deltaLength;

            particle.Position += vectorDelta * diff * 0.5f;


            transform.position -= vectorDelta * diff * 0.5f;

            transform.gameObject.GetComponent<Rigidbody>().AddForce(-vectorDelta * diff * 0.5f, ForceMode.VelocityChange);
    }

    bool everyOther = true;
    private void ConstrainParticleAndTransform(ZB_VerletParticle particle, Transform transform, float restraintLength, bool releaseLine)
    {
        Vector3 vectorDelta = transform.position - particle.Position;
        float deltaLength = vectorDelta.magnitude;

            float diff = (deltaLength - restraintLength) / deltaLength;
            particle.Position += vectorDelta * diff * .2f;

            float tipPullForce = (-vectorDelta * diff * 10f).magnitude;
            //transform.gameObject.GetComponent<Rigidbody>().AddForce(-vectorDelta * diff * 0.5f * elasticity);


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
    }

    void OnTriggerEnter(Collider other)
    {
    }




}
