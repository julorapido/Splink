using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_T_Projectile : MonoBehaviour
{
    
    /// <summary>
    /// An impassable tile is one which does not allow the player to move through
    /// it at all. It is completely solid.
    /// </summary>

    [Header ("Player_")]
    [HideInInspector] public Transform plyr_trnsfrm;

    [HideInInspector] public enum turret_Type{
        Normal,
        Double,
        Catapult,
        Heavy,
        Sniper,
        Gattling
    };
    [SerializeField]
    public turret_Type blt_type;

    [Header ("Attached Objects")]
    [HideInInspector] public Transform plyr_target;
    //[HideInInspector] public Transform prnt_turret;
    private MeshRenderer bullet_msh;

    [Header ("Projectile_Speeds")]
    private float speed = 15f;
    private float turnSpeed = 12f;
    private bool plyr_passed = false;

    private int z_ = 0;

    // Start is called before the first frame update
    private void Start()
    {
        bullet_msh  = gameObject.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        //gameObject.transform.LookAt(plyr_trnsfrm);
        float dst_ = Vector3.Distance(transform.position, plyr_target.position);
        if( (dst_ < 8) && !plyr_passed) {plyr_passed = true; speed *= 1.3f;}

        try{
            Vector3 dir = plyr_target.position - transform.position;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, dir, Time.deltaTime * turnSpeed, 0.0f);
            switch(blt_type){
                case turret_Type.Normal:
                case turret_Type.Double:
                    //Debug.DrawRay(transform.position, newDirection, Color.red);

                    transform.Translate(Vector3.forward * Time.deltaTime * speed);
                    transform.rotation = Quaternion.LookRotation(newDirection);
                    transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, z_);
                    z_+= 2;
                    break;
                case turret_Type.Heavy:
                    transform.Translate(Vector3.forward * Time.deltaTime * speed);

                    break;
                case turret_Type.Sniper:

                    break;
                case turret_Type.Gattling:

                    break;
                case turret_Type.Catapult:

                    break;
            }
        }catch{
            Debug.Log("No turret bullet type");
        }
    }

    private void FixedUpdate(){
        transform.Rotate(0,0,10f, Space.Self);
    }


    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "plaer_hitbx"){
            Debug.Log("zer");
            bullet_explode();
        } 
        if(other.gameObject.tag == "ground") bullet_explode();
    }

    private void bullet_explode(){
        bullet_msh.enabled = false;

    }

    private Vector3 CalculateCatapult(Vector3 target, Vector3 origen, float time)
    {
        Vector3 distance = target - origen;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0;

        float Sy = distance.y;
        float Sxz = distanceXZ.magnitude;

        float Vxz = Sxz / time;
        float Vy = Sy / time + 0.5f * Mathf.Abs(Physics.gravity.y) * time;

        Vector3 result = distanceXZ.normalized;
        result *= Vxz;
        result.y = Vy;

        return result;
    }
}
