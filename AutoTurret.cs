using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTurret : MonoBehaviour
{
    [Header ("Turret Type")]
    public static List<string> turret_types = new List<string>(new string[] {"Normal","Double",
    "Catapult", "Heavy", "Sniper", "Gattling", "LightGun", "Military"});
    private bool plyr_n_sight = false;
    public GameObject plyr_gm;

    private enum turret_Type{
        Normal,
        Double,
        Catapult,
        Heavy,
        Sniper,
        Gattling,
        LightGun,
        Military
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
    public float shootCoolDown;
    private float timer;

    [Header ("Turret Rotations")]
    private List<float> randomRot_a = new List<float>(new float[] {0.0f, 0.0f, 0.0f});
    private bool hz_sens = false;

    private List<float> randomRot_vrt = new List<float>(new float[] {0.0f, 0.0f, 0.0f});
    private bool vrt_sens = false;

    private float tr_aimSpeed = 60.2f;
    private List<Vector3> strt_rt = new List<Vector3>(new Vector3[] {new Vector3(0,0,0),new Vector3(0,0,0), new Vector3(0,0,0)} );

    [Header ("Turret Projectile")]
    public GameObject turret_bullet;

    [Header ("Turret Orientation")]
    public bool is_horizontal;
    private bool reset_bl = false;


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
            if(chld_g?.tag == "tr_Shootp"){
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
        float randm_flt_a =  UnityEngine.Random.Range(40f, 70f);
        float randm_flt_vrt =  UnityEngine.Random.Range(23f, 42f);
        for(int b = 0; b < k; b ++){
            randomRot_a[b] = is_horizontal ? randm_flt_a : (tr_body[k-1].rotation.y + randm_flt_a);
            randomRot_vrt[b] = tr_body[b].rotation.eulerAngles.x + randm_flt_vrt;

            strt_rt[b] = tr_body[b].eulerAngles;
        }
        // Reset X-Axis 0deg rotation [Horizontal]
        if(is_horizontal && (t_type == turret_Type.Normal || t_type == turret_Type.Double) ) tr_body[k - 1].localRotation = Quaternion.Euler(0, tr_body[k - 1].localRotation.y, 0);
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
                reset_bl = false;
                shoot_prjcle();
            }
        }

        if(!plyr_n_sight && ( (t_type == turret_Type.Normal || t_type == turret_Type.Double) )){
            if(!reset_bl){
                for(int c = 0; c < k; c ++) tr_body[c].rotation = Quaternion.Euler(strt_rt[c].x , -180, strt_rt[c].z);
                reset_bl = true;
            }
        }
    }

    private void follow_target(bool is_idle) //todo : smooth rotate
    {
        for(int a = 0; a < k; a ++){
            // IDLE TURRET ROTATION
            if(is_idle){    

                // Y-AXIS ROTATION for Normal--Double
                // AND FOR VERTICAL => [Sniper, Heavy, Military]
                if( (t_type == turret_Type.Normal || t_type == turret_Type.Double) || (!is_horizontal) )
                {
                    float y_rot = !is_horizontal ? ((tr_body[a].localRotation.eulerAngles.y < 180f) ? 
                        (tr_body[a].localRotation.eulerAngles.y) : (-1 * (360 - tr_body[a].localRotation.eulerAngles.y)) )
                            : 
                        (tr_body[a].localRotation.eulerAngles.x > 270f) ? 
                            (-1* (360 - tr_body[a].localRotation.eulerAngles.x)) : (tr_body[a].localRotation.eulerAngles.x )
                    ;
                            
                    if(!hz_sens && (y_rot > randomRot_a[a]) ) hz_sens = true;
                    if(hz_sens && (y_rot < (is_horizontal ? -1* (randomRot_a[a] + 25) : -randomRot_a[a]) ) ) hz_sens = false; // HORIZONTAL GOT 35 DEGRES MORE ON HZ_SENS TRUE

                    float h_agl_add = hz_sens ? -0.45f : 0.45f; // HORIZONTAL

                    tr_body[a].Rotate(0f, h_agl_add, 0f, Space.World);

                    // rt_stand [NON-HORIZONTAL]
                    if(!is_horizontal && l > 0 && (t_type == turret_Type.Normal || t_type == turret_Type.Double) ) tr_stand[l - 1].Rotate(0f, h_agl_add, 0f, Space.World);
                }


                // VERT-AXIS ROTATION for NON Normal--Double
                // ONLY FOR HORIZONTAL => [Sniper, Heavy, Military]
                if(is_horizontal){
                    if(t_type != turret_Type.Normal && t_type != turret_Type.Double ){
                        
                        float rt_v = ( tr_body[a].rotation.eulerAngles.x < 270f ? 
                            (tr_body[a].rotation.eulerAngles.x + 360f) : tr_body[a].rotation.eulerAngles.x
                        );

                        float rot_rest = Math.Abs(strt_rt[a].x - (randomRot_vrt[a]) );
                        if(a == 0){
                            //  Debug.Log(rt_v );
                            //  Debug.Log("vs " + (randomRot_vrt[a]) );
                        }
                        if(!vrt_sens && (rt_v < randomRot_vrt[a] - 2 * rot_rest) ) { vrt_sens = true; }
                        if(vrt_sens && (rt_v > randomRot_vrt[a]) ) { vrt_sens = false; };

                        float v_agl_add = vrt_sens ? -0.45f : 0.45f; // VERT
                        tr_body[a].Rotate(v_agl_add, 0f, 0f, Space.World);
                    }
                }
                // break;
                // return;

            }else{  
                ////////////////////////////////////////////

                // TURRET AUTO-AIM
                // ONLY FOR 1ST TR_BARRELHZ !!
                if(a == 0){
                    Vector3 target_Dir = new Vector3(0,0,0);
                    if(is_horizontal){
                        target_Dir = new Vector3(plyr_gm.transform.rotation.x, 
                                                plyr_gm.transform.rotation.y,
                                            0f) ;
                    }
                    Vector3 trgt = is_horizontal ? target_Dir :  plyr_gm.transform.position;
                    if(is_horizontal){
                        tr_body[a].LookAt(plyr_gm.transform, Vector3.right);
                    }else{
                        tr_body[a].LookAt(plyr_gm.transform);
                    }
                }
                // just [VERT or HORIZONTAL] rotation for Stand

            }
        }
        
    }

    private void shoot_prjcle(){
        if(plyr_n_sight){
            try{
                Vector3 msl_scale = turret_bullet.transform.localScale;

                switch(t_type){
                    case turret_Type.Normal:
                        //Instantiate()
                        LeanTween.scale(tr_barrels[0], tr_barrels[0].transform.localScale * 1.2f, shootCoolDown - 0.2f).setEasePunch();
                        GameObject missle_Go = Instantiate(turret_bullet, tr_sht_points[0].transform.position, tr_sht_points[0].transform.rotation);
                        //typeof(GameObject) as GameObject;
                        missle_Go.transform.localScale = new Vector3(msl_scale.x * (4*(gameObject.transform.localScale.x / 5)), msl_scale.y * (4*(gameObject.transform.localScale.y / 5)),
                            msl_scale.z * (4*(gameObject.transform.localScale.z / 5))
                        );
                        if(!missle_Go) return;

                        A_T_Projectile projectile_scrpt = missle_Go.GetComponent<A_T_Projectile>();
                        projectile_scrpt.plyr_target = plyr_gm.transform;
                        projectile_scrpt.blt_type = A_T_Projectile.turret_Type.Normal;
                        break;
                    case turret_Type.Double:
                        for(int i = 0; i < k; i ++){
                            LeanTween.scale(tr_barrels[i], tr_barrels[i].transform.localScale * 1.2f, shootCoolDown - 0.2f).setEasePunch();

                            GameObject m_Go = Instantiate(turret_bullet, tr_sht_points[i].transform.position, tr_sht_points[i].transform.rotation);
                            m_Go.transform.localScale = new Vector3(msl_scale.x * (4*(gameObject.transform.localScale.x / 5)), msl_scale.y * (4*(gameObject.transform.localScale.y / 5)),
                                msl_scale.z * (4*(gameObject.transform.localScale.z / 5))
                            );

                            A_T_Projectile pj_scrpt = m_Go.GetComponent<A_T_Projectile>();
                            pj_scrpt.plyr_target = plyr_gm.transform;
                            pj_scrpt.blt_type = A_T_Projectile.turret_Type.Double;
                        }
                        break;
                    case turret_Type.Heavy:
                        LeanTween.scale(tr_barrels[0], tr_barrels[0].transform.localScale * 1.35f, shootCoolDown - 0.05f).setEasePunch();

                        GameObject missle_Go_h = Instantiate(turret_bullet, tr_sht_points[0].transform.position, tr_sht_points[0].transform.rotation);
                        // missle_Go.transform.localScale = new Vector3(msl_scale.x * (4*(gameObject.transform.localScale.x / 5)), msl_scale.y * (4*(gameObject.transform.localScale.y / 5)),
                        //     msl_scale.z * (4*(gameObject.transform.localScale.z / 5))
                        // );
                        if(!missle_Go_h) return;

                        A_T_Projectile proj_scrpt_h = missle_Go_h.GetComponent<A_T_Projectile>();
                        proj_scrpt_h.plyr_target = plyr_gm.transform;
                        proj_scrpt_h.blt_type = A_T_Projectile.turret_Type.Heavy;
                        break;
                    case turret_Type.Sniper:
                        break;
                    case turret_Type.Gattling:
                        
                        break;
                    case turret_Type.Catapult:

                        //Vector3 throw_dst = CalculateCatapult(plyr_gm.transform.position, gameObject.transform.position, 1f);
                        break;
                    case turret_Type.LightGun:
                        
                        break;
                    case turret_Type.Military:
                    
                        break;
                }
            }catch(Exception err){
                Debug.Log(err);
                //Debug.Log("no turret type");
            }
        }
    }

    private void target_check()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, 2f);
        //float distAway = Mathf.Infinity;
        bool s = false;
        for (int i = 0; i < colls.Length; i++)
        {
            if (colls[i].tag == "player_hitbx")
            {
                plyr_gm = colls[i].gameObject;
                plyr_n_sight = true; s = true;
                break;
            }
        }
        if (!s){plyr_n_sight = false;}
    }

    
}
