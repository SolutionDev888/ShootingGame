using UnityEngine;
using System.Collections;

public class DamageController : MonoBehaviour {
    [System.Serializable]
    public enum HitPos
    {
        Head = 5,
        Body = 2,
        Arm = 1,
        Leg = 1,
    }
    public HitPos hitPos = HitPos.Body;
    AI_Move root;
    public void Damage(float damage)
    {
        if(root == null)
        {
            root = GetComponentInParent<AI_Move>();
        }
		root.Damage(damage*((float)hitPos)*0.5f);
    }
    public void Die()
    {
        Component.Destroy(this.GetComponent<vp_SurfaceIdentifier>());
    }
}
