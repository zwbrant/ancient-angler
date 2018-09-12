using UnityEngine;
using System.Collections;

public class ZB_VerletParticle : MonoBehaviour {
    public Vector3 Position { get; set; }
    public Vector3 OldPosistion { get; set; }
    public Vector3 Acceleration { get; set; }

    public ZB_VerletParticle(Vector3 startPosition, bool yes)
    {
        Position = startPosition;
        OldPosistion = Position;
        //if (yes)
        //    Acceleration = new Vector3(0f, .1f, 0f);
        //else
        //    Acceleration = new Vector3(0f, -.1f, 0f);

    }

    Vector3 velocityVector = Vector3.zero;
    Vector3 oldVelocityVector = Vector3.zero;

    internal void WeirdUpdate(float deltaTime)
    {
        velocityVector = (Position - OldPosistion);

        Vector3 velocityDelta = velocityVector - oldVelocityVector;

        //if (velocityVector.magnitude > .1f)
        //{
        //    print(velocityVector.magnitude);
        //}

        Acceleration = velocityDelta / deltaTime;
        //Acceleration = Acceleration + new ;

        oldVelocityVector = velocityVector;
    }


}
