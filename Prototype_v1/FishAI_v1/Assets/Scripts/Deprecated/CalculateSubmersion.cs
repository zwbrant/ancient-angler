using UnityEngine;
using System;
using System.Collections;
using System.Linq;

public class CalculateSubmersion : MonoBehaviour {
    public GameObject a, b, c, d;
    LineRenderer lineRenderer;

    private Vector3[] cornerVectors = new Vector3[4];
    public Vector3[] CornerVectors
    {
        get
        {
            cornerVectors[0] = a.transform.position;
            cornerVectors[1] = b.transform.position;
            cornerVectors[2] = c.transform.position;
            cornerVectors[3] = d.transform.position;
            return cornerVectors;
        }
    }

    public Vector3[] CornersByHeight
    {
        get
        {
            return CornerVectors.OrderBy(v => v.y).ToArray<Vector3>();
        }
    }

    [Range(1, 20)]
    public int velocityFrameBuffer = 4;
    Vector3 previousPos;
    Vector3 cumulativeVelocity = new Vector3();
    private Vector3 velocityVector = new Vector3();
    public float Velocity
    {
        get { return velocityVector.magnitude; }
    }

    private Vector3 direction;
    public Vector3 Direction
    {
        get { return velocityVector.normalized; }
    }

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Use this for initialization
    void Start () {
        previousPos = transform.position;

    }

    int frameCount = 0;
    void Update () {
        if (frameCount < velocityFrameBuffer)
        {
            cumulativeVelocity += (transform.position - previousPos) / Time.deltaTime;
            frameCount++;
        } else
        {
            velocityVector = cumulativeVelocity / velocityFrameBuffer;
            frameCount = 0;
            cumulativeVelocity = new Vector3();
        }

        previousPos = transform.position;

        //Debug.DrawLine(CornersByHeight[0], CornersByHeight[1], Color.magenta);
    }

    void FixedUpdate()
    {
        direction = (transform.position - previousPos).normalized;

        //if (previousPos != null)
        //    velocity = (Vector3.Magnitude(transform.position - previousPos) * 5);
    }

    public float GetSubmergArea()
    {
        int cornersSubmerged = CornerVectors.Count(v => v.y < 0);

        float submergArea = 0f;
        switch (cornersSubmerged)
        {
            case 1:
                submergArea = GetAreaOneSubmergCorner();
                break;
            case 2:
                submergArea = GetAreaTwoSubmergCorner();
                break;
            case 3:
                submergArea = GetAreaThreeSubmergCorner();
                break;
            case 4:
                submergArea = GetAreaFourSubmergCorners();
                break;
            default:
                submergArea = 0f;
                break;
        }

        return submergArea;
    }

    public float[] GetCoordTuple(GameObject go)
    {
        float[] result = new float[2];

        result[0] = go.transform.position.x;
        result[1] = go.transform.position.y;

        return result;
    }

    public float GetAreaOneSubmergCorner()
    {
        Vector3 lowestCorner = CornersByHeight[0];

        RaycastHit shortWaterHit;
        RaycastHit tallWaterHit;
        bool shortHit = Physics.Linecast(CornersByHeight[1], lowestCorner, out shortWaterHit);
        bool tallHit = Physics.Linecast(CornersByHeight[2], lowestCorner, out tallWaterHit);

        Vector3 shortWaterVect = shortWaterHit.point;
        Vector3 tallWaterVect = tallWaterHit.point;

        float submergBase = Math.Abs(Vector3.Distance(lowestCorner, tallWaterVect));
        float submergHeight = Math.Abs(Vector3.Distance(lowestCorner, shortWaterVect));
        float submergArea = (submergBase * submergHeight) / 2f;

        //Debug.DrawLine(lowestCorner, shortWaterVect, Color.red);
        //Debug.DrawLine(lowestCorner, tallWaterVect, Color.red);

        lineRenderer.SetVertexCount(4);
        lineRenderer.SetPositions(new Vector3[] { lowestCorner, shortWaterVect, tallWaterVect, lowestCorner });

        return submergArea;
    }

    public float GetAreaTwoSubmergCorner()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        RaycastHit shortWaterHit;
        RaycastHit tallWaterHit;
        bool shortHit = Physics.Linecast(CornersByHeight[2], CornersByHeight[0], out shortWaterHit);
        bool tallHit = Physics.Linecast(CornersByHeight[3], CornersByHeight[1], out tallWaterHit);

        Vector3 shortWaterVect = shortWaterHit.point;
        Vector3 tallWaterVect = tallWaterHit.point;

        Vector3[] corns = CornersByHeight;

        float rightTriHeight = Vector3.Distance(CornersByHeight[1], tallWaterVect);
        float rightTriBase = Vector3.Distance(CornersByHeight[1], CornersByHeight[0]);
        float rightTriArea = (rightTriBase * rightTriHeight) / 2f;

        //Debug.DrawLine(CornersByHeight[1], tallWaterVect, Color.green, 1f, true);
        //Debug.DrawLine(CornersByHeight[1], CornersByHeight[0], Color.green, 1f, true);


        float acuteTriBase = Vector3.Distance(shortWaterVect, tallWaterVect);
        float acuteTriHeight = Math.Abs(0 - CornersByHeight[0].y);
        float acuteTriArea = (acuteTriBase * acuteTriHeight) / 2f;

        lineRenderer.SetVertexCount(6);
        lineRenderer.SetPositions(new Vector3[] { CornersByHeight[0], CornersByHeight[1], tallWaterVect, CornersByHeight[0], shortWaterVect, tallWaterVect });

        //Debug.DrawLine(shortWaterVect, tallWaterVect, Color.blue, 1f, true);
        //Debug.DrawLine(CornersByHeight[1], CornersByHeight[0], Color.blue, 1f, true);

        return rightTriArea + acuteTriArea;
    }

    public float GetAreaThreeSubmergCorner()
    {
        RaycastHit deepWaterHit;
        RaycastHit shallowWaterHit;
        Physics.Linecast(CornersByHeight[3], CornersByHeight[2], out shallowWaterHit);
        Physics.Linecast(CornersByHeight[3], CornersByHeight[1], out deepWaterHit);
        Vector3 deepWaterVect = deepWaterHit.point;
        Vector3 shallowWaterVect = shallowWaterHit.point;

        float shallowRightTriHeight = Vector3.Distance(CornersByHeight[2], shallowWaterVect);
        float shallowRightTriBase = Vector3.Distance(CornersByHeight[0], CornersByHeight[2]);
        float shallowRightTriArea = (shallowRightTriHeight * shallowRightTriBase) / 2f;

        float deepRightTriHeight = Vector3.Distance(CornersByHeight[1], deepWaterVect);
        float deepRightTriBase = Vector3.Distance(CornersByHeight[0], CornersByHeight[1]);
        float deepRightTriArea = (deepRightTriHeight * deepRightTriBase) / 2f;

        float[] acuteTriSides = new float[] {
            Vector3.Distance(deepWaterVect, shallowWaterVect),
            Vector3.Distance(CornersByHeight[0], deepWaterVect),
            Vector3.Distance(CornersByHeight[0], shallowWaterVect)};

        lineRenderer.SetVertexCount(9);
        lineRenderer.SetPositions(new Vector3[] { CornersByHeight[0], CornersByHeight[2], shallowWaterVect, CornersByHeight[0], CornersByHeight[1], deepWaterVect,
                                                    CornersByHeight[0], shallowWaterVect, deepWaterVect});

        float halfPerim = acuteTriSides.Sum() / 2f;

        float acuteTriArea = (float)Math.Sqrt(halfPerim * (halfPerim - acuteTriSides[0]) * (halfPerim - acuteTriSides[1]) * (halfPerim - acuteTriSides[2]));

        return acuteTriArea + deepRightTriArea + shallowRightTriArea;
    }

    // cache fully submerged paddle area, since it will never change
    private float paddleArea = -1f;
    public float GetAreaFourSubmergCorners()
    {
        if (paddleArea == -1f)
            paddleArea = Vector3.Distance(CornerVectors[0], CornerVectors[1]) * Vector3.Distance(CornerVectors[0], CornerVectors[2]);

        return paddleArea;
    }


}
