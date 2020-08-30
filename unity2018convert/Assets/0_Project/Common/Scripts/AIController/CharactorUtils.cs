using UnityEngine;
using System.Collections;

public static class CharactorUtils {
    static public float deltaYAngle (this Vector3 sourceForward, Vector3 targetDirection){
//		float angle = Quaternion.Angle(source.rotation, target.rotation);
		Vector3 forwardA = sourceForward.normalized;
        Vector3 forwardB = targetDirection.normalized;
        float angleA = Mathf.Atan2(forwardA.x, forwardA.z) * Mathf.Rad2Deg;
        float angleB = Mathf.Atan2(forwardB.x, forwardB.z) * Mathf.Rad2Deg;
        return Mathf.DeltaAngle(angleA, angleB);
    }
    static public float deltaXAngle(this Vector3 sourceForward, Vector3 targetDirection)
    {
        //		float angle = Quaternion.Angle(source.rotation, target.rotation);
        Vector3 forwardA = sourceForward.normalized;
        Vector3 forwardB = targetDirection.normalized;
        float angleA = Mathf.Atan2(forwardA.y, Mathf.Sqrt(forwardA.z * forwardA.z + forwardA.x * forwardA.x)) * Mathf.Rad2Deg;
        float angleB = Mathf.Atan2(forwardB.y, Mathf.Sqrt(forwardB.z * forwardB.z + forwardB.x * forwardB.x)) * Mathf.Rad2Deg;
        return Mathf.DeltaAngle(angleA, angleB);
    }
    static public float toNegativeAngle(this float angle)
    {
        return (angle < 0) ? angle : -360f + angle;
    }
    static public float toPositiveAngle(this float angle)
    {
        return (angle < 0) ? 360f + angle : angle;
    }
}
