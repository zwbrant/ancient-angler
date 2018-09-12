using UnityEngine;
using System.Collections;

public static class ZB_WaterMathUtilities {
    const float WaterHeight = 0f;

    public static Vector3 CrossProduct(Vector3 a, Vector3 b)
    {
        Vector3 c = new Vector3(0, 0, 0);
        c.x = (a.y * b.z) - (a.z - b.y);
        c.y = (a.z * b.x) - (a.x - b.z);
        c.z = (a.x * b.y) - (a.y * b.x);

        return c;
    }


}
