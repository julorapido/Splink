using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTurret : MonoBehaviour
{
    public static List<string> turret_types = new List<string>(new string[] {"Normal","Double","Gattling", "Catapult", "Heavy"});
    private bool plyr_n_sight = false;
    private GameObject plyr_gm;
    private enum turret_Type{
        Normal,
        Double,
        Gattling,
        Catapult,
        Heavy
    };
    [SerializeField]
    turret_Type t_type = new turret_Type();
    
    [Header ("Attached Turret Lists")]
    int i = 0, j = 0, k = 0;
    private List<GameObject> tr_barrels = new List<GameObject>(new GameObject[] {null, null, null});
    private List<GameObject> tr_sht_points = new List<GameObject>(new GameObject[] {null, null, null});
    private List<Transform> tr_body = new List<Transform>(new Transform[] {null, null, null});

    [Header ("Turret FireRate")]
    private float timer;
    private float shootCoolDown;

    [Header ("Turret Rotations")]
    private float randomRot_a;
    private float tr_aimSpeed = 60.2f;

    [Header ("Turret Orientation")]
    public bool is_horizontal;
    private void Start()
    {
        InvokeRepeating("target_check", 0, 0.25f);
        InvokeRepeating("shoot_prjcle", 0, 0.50f);
        randomRot_a = Random.Range(30, 60);

        // ASSIGNEMENT OF BARRELS AND SHOOTS POINTS
        Transform[] tr_trsnfrms = GetComponentsInChildren<Transform>();
        //Debug.Log(tr_trsnfrms.Length);
        foreach(Transform chld_ in tr_trsnfrms){
            GameObject chld_g = chld_.gameObject;
            if(chld_g?.tag == "tr_Barrel"){
                tr_barrels[i] = chld_.gameObject; i++;
            }  
            if(chld_g?.tag == "tr_ShootP"){
                tr_sht_points[j] = chld_g; j++;
            }
            if(is_horizontal ? chld_g?.tag == "tr_BarrelHz" : chld_g?.tag == "tr_Body"){
                tr_body[k] = chld_; k++; 
            }
        }
    }

    // FixedUpdate is called for physics (around 3x time per frame)
    private void FixedUpdate()
    {
        if(plyr_n_sight){
            follow_target(false);
        }else{
            follow_target(true);
        }

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
            float angl_add = 0.5f;
            if (tr_body[k - 1].rotation.y < randomRot_a &&  tr_body[k - 1].rotation.y > -randomRot_a){
                tr_body[k - 1].Rotate(is_horizontal ? 0.5f : 0f, !is_horizontal ? 0.5f : 0, 0, Space.Self);
                //rotation = Quaternion.RotateTowards(tr_body[k - 1].rotation, Quaternion.Euler(randomRot), tr_aimSpeed * Time.deltaTime);
            }
            else{
                angl_add = -angl_add;
                tr_body[k - 1].Rotate(is_horizontal ? (angl_add*3f) : 0f, !is_horizontal ? (angl_add*3f) : 0f, 0, Space.Self);
            }
            return;
        }
        // TURRET AUTO-AIM
        Vector3 target_Dir =  Quaternion.LookRotation(plyr_gm.transform.position - tr_body[k - 1].position).eulerAngles;
        target_Dir.z = 0;
        if(is_horizontal){target_Dir.y = 0;}else{
            target_Dir.x = 0;
        }
        Debug.Log(target_Dir);
        //turreyHead.forward = targetDir;
        tr_body[k - 1].rotation = Quaternion.RotateTowards(tr_body[k - 1].rotation, Quaternion.Euler(target_Dir), tr_aimSpeed * Time.deltaTime);
            // if (t_type == turret_Type.Normal){
        //     tr_body[k - 1].forward = target_Dir;
        // }
        // else{
        //     tr_body[k - 1].rotation = Quaternion.RotateTowards(tr_body[k].rotation, Quaternion.LookRotation(target_Dir), tr_aimSpeed * Time.deltaTime);
        // }
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
