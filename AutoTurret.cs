using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTurret : MonoBehaviour
{
    [Header ("Turret Type")]
    public static List<string> turret_types = new List<string>(new string[] {"Normal","Double","Catapult", "Heavy", "Sniper","Gattling"});
    private bool plyr_n_sight = false;
    private GameObject plyr_gm;
    private enum turret_Type{
        Normal,
        Double,
        Catapult,
        Heavy,
        Sniper,
        Gattling
    };
    [SerializeField]
    turret_Type t_type = new turret_Type();
    
    [Header ("Attached Turret Lists")]
    int i = 0, j = 0, k = 0, l = 0;
    private List<GameObject> tr_barrels = new List<GameObject>(new GameObject[] {null, null, null});
    private List<GameObject> tr_sht_points = new List<GameObject>(new GameObject[] {null, null, null});
    private List<Transform> tr_body = new List<Transform>(new Transform[] {null, null, null});
    private List<Transform> tr_stand = new List<Transform>(new Transform[] {null, null, null});

    [Header ("Turret FireRate")]
    private float timer;
    private float shootCoolDown;

    [Header ("Turret Rotations")]
    private float randomRot_a;
    private bool hz_sens = false;

    private float randomRot_vrt;
    private bool vrt_sens = false;

    private float tr_aimSpeed = 60.2f;
    
    [Header ("Turret Projectile")]
    private Quaternion strt_rt;
    public GameObject turret_projectiles;

    [Header ("Turret Orientation")]
    public bool is_horizontal;
    private void Start()
    {
        InvokeRepeating("target_check", 0, 0.25f);
        //InvokeRepeating("shoot_prjcle", 0, 0.50f);
        // ASSIGNEMENT OF BARRELS AND SHOOTS POINTS
        Transform[] tr_trsnfrms = GetComponentsInChildren<Transform>();
        //Debug.Log(tr_trsnfrms.Length);
        foreach(Transform chld_ in tr_trsnfrms){
            GameObject chld_g = chld_.gameObject;
            if(chld_g?.tag == "tr_Barrel"){
                tr_barrels[i] = chld_.gameObject;
                 i++;
            }  
            if(chld_g?.tag == "tr_ShootP"){
                tr_sht_points[j] = chld_g; 
                 j++;
            }
            if(t_type != turret_Type.Catapult){
                if(is_horizontal ? chld_g?.tag == "tr_BarrelHz" : chld_g?.tag == "tr_BarrelHz"){
                    tr_body[k] = chld_;
                    k++; 
                }
            }else{
                if(chld_g?.tag == "tr_Body"){
                    tr_body[k] = chld_;
                    k++; 
                } 
            }
            if(chld_g?.tag == "tr_Stand"){
                tr_stand[l] = chld_;
                l ++;
            }
        }
        // HORIZONTAL START X-AXIS DEGREE GAP
        randomRot_a = is_horizontal ?  (Random.Range(25f, 50f)): Random.Range(tr_body[k-1].rotation.y + 35, tr_body[k-1].rotation.y + 70);
        randomRot_vrt = Random.Range(tr_body[k-1].rotation.x + 15, tr_body[k-1].rotation.x + 32);

        // Reset X-Axis 20deg rotation [Horizontal]
        if(is_horizontal) tr_body[k - 1].localRotation = Quaternion.Euler(0, tr_body[k - 1].localRotation.y, 0);

        strt_rt = tr_body[k-1].rotation;
    }

    // FixedUpdate is called for physics (around 3x time per frame)
    private void Update()
    {
        if(plyr_n_sight){
            follow_target(false);
        }else{
            follow_target(true);
        }

        // BLOCK Z-AXIS
        //tr_body[k - 1].rotation = Quaternion.Euler(tr_body[k - 1].rotation .x, tr_body[k - 1].rotation.y, 0);

        timer += Time.deltaTime;
        if (timer >= shootCoolDown)
        {
            if (plyr_n_sight)
            {
                timer = 0;
                shoot_prjcle();
            }
        }
    }

    private void follow_target(bool is_idle) //todo : smooth rotate
    {
        // IDLE TURRET ROTATION
        if(is_idle){

            // Y-AXIS ROTATION
            float y_rot = !is_horizontal ? ((tr_body[k - 1].localRotation.eulerAngles.y < 180f) ? 
                (tr_body[k - 1].localRotation.eulerAngles.y) : (-1 * (360 - tr_body[k - 1].localRotation.eulerAngles.y)) )
                    : 
                (tr_body[k - 1].localRotation.eulerAngles.x > 270f) ? 
                    (-1* (360 - tr_body[k - 1].localRotation.eulerAngles.x)) : (tr_body[k - 1].localRotation.eulerAngles.x )
            ;
    
            if(!hz_sens && (y_rot > randomRot_a) ) hz_sens = true;
            if(hz_sens && (y_rot < (is_horizontal ? -randomRot_a-50 : -randomRot_a) ) ) hz_sens = false; // HORIZONTAL GOT 50 DEGRES MORE ON HZ_SENS TRUE

            float h_agl_add = hz_sens ? -0.45f : 0.45f; // HORIZONTAL

            tr_body[k - 1].Rotate(0f, h_agl_add, 0f, Space.World);
            // rt stand
            if(!is_horizontal) tr_stand[l - 1].Rotate(0f, h_agl_add, 0f, Space.World);
    

            // VERT-AXIS ROTATION
            float rt_v = (tr_body[k - 1].localRotation.eulerAngles.x < 0f) ? 
                -1 * (360 - tr_body[k - 1].localRotation.eulerAngles.x) : tr_body[k - 1].localRotation.eulerAngles.x
            ;
            if(!vrt_sens && (rt_v < -randomRot_vrt) ) vrt_sens = true;
            if(vrt_sens && (rt_v > randomRot_vrt) ) vrt_sens = false;

            float v_agl_add = vrt_sens ? -0.50f : 0.50f; // VERT

            //tr_body[k - 1].Rotate(v_agl_add, 0f, 0f, Space.World);

            
            return;
        }
        
        // TURRET AUTO-AIM
        Vector3 target_Dir = new Vector3(0,0,0);
        if(is_horizontal){
            target_Dir = new Vector3(plyr_gm.transform.rotation.x, 
                                    plyr_gm.transform.rotation.y,
                                0f) ;
        }
        Vector3 trgt = is_horizontal ? target_Dir :  plyr_gm.transform.position;
        if(is_horizontal){
            tr_body[k - 1].LookAt(plyr_gm.transform, Vector3.right);
        }else{
            tr_body[k - 1].LookAt(plyr_gm.transform);
        }
    }

    private void shoot_prjcle(){
        if(plyr_n_sight){
            try{
                switch(t_type){
                    case turret_Type.Normal:
                        break;
                    case turret_Type.Double:
                        break;
                    case turret_Type.Gattling:
                        break;
                    case turret_Type.Catapult:
                        Vector3 throw_dst = CalculateCatapult(plyr_gm.transform.position, gameObject.transform.position, 1f);
                        break;
                }
            }catch{
                Debug.Log("no turret type");
            }
        }
    }

    private void target_check()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, 30f);
        //float distAway = Mathf.Infinity;
        bool s = false;
        for (int i = 0; i < colls.Length; i++)
        {
            if (colls[i].tag == "Player")
            {
                plyr_gm = colls[i].gameObject;
                plyr_n_sight = true; s = true;
                break;
            }
        }
        if (!s){plyr_n_sight = false;}
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
