using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraMovement : MonoBehaviour
{

    [Header ("Player Animator")]
    public GameObject p_gm;
    private Animator p_anim;

    [Header ("Player Inspector Values")]
    public Transform player;
    public Rigidbody player_rb;

    [Header ("Camera Offset Value")]
    public Vector3 offset;
    
    private float smoothTrans_  = 2.5f;
    private bool game_Over_ = false;

    // private float updtd_plyr_offset;
    // private float prev_plyr_offset;

    private const float vert_Trns = 2.00f; // VERT CAMERA CLAMP TIME
    private float vert_y_pos = 0f;
    private float vert_y_rot = 0f;
    private float xRot;
    private Vector3 def_offset;

    [Header ("Camera Jump/Fly/Slide offset values")]
    public float supl_xRot = 0.0f;
    public float supl_yOff = 0.0f;
    private float mathfRef = 0.0f;
    private string dy_cm_inf = "";

    [Header ("Camera WallRun Values")]
    private float wallR_x_offst = 0.0f;
    private float wallR_y_offst = 0.0f;
    private float wallR_z_offst = 0.0f;

    public float wallR_rot_x_offst = 0.0f;
    private float wallR_rot_y_offst = 0.0f;
    private float wallR_rot_z_offst = 0.0f;

    [Header ("Fov Floats")]
    private float start_fov;
    private float new_fov;

    [Header ("Grappl SmoothDamp Boolean")]
    private bool smthDmp_grpl = false;

    private Camera c_;

    private void Start(){
        p_anim = p_gm.GetComponentInChildren<Animator>();

        def_offset = offset;
        //Application.targetFrameRate = 60;
        xRot = gameObject.transform.rotation.x;
        gameObject.transform.position = player.position + offset;
        
        desired_  = (player.position + offset);

        c_ = gameObject.GetComponent<Camera>();
        start_fov = gameObject.GetComponent<Camera>().fieldOfView;
        new_fov = start_fov;
    }
    public void cam_GamerOver_(){
        game_Over_ = true;
    }

    public void fly_dynm(bool is_leave){
        StopCoroutine(vert_rt(true)); StopCoroutine(vert_rt(false)); 
        if(is_leave){
            StartCoroutine(vert_rt(true));
            return;
        }
        StartCoroutine(vert_rt(false));
    }

    [Header ("Camera FixedUpdate & Lateupdate var")]
    // FXED UPDATE for Values
    private Vector3 desired_;
    private float lst_offst_x;
    private Quaternion desired_rt;
    private float x_offst = 0.0f;
    //private float smoothTime = 2f ;
    private Vector3 currentVelocity;

    private bool tyro_on = false;
    private bool sliding_on = false;
    private bool rotate_back = false;

    private void Update(){
        if (!Input.GetKeyDown("q") && !Input.GetKeyDown("d") ){rotate_back = true;}else{
            rotate_back = false;
        }

        if(c_.fieldOfView != new_fov){
            c_.fieldOfView = Mathf.Lerp(c_.fieldOfView, new_fov, 0.85f);
        }

        if(smthDmp_grpl){
            wallR_rot_x_offst = Mathf.SmoothDamp(wallR_rot_x_offst, -0.08f, ref mathfRef, 0.295f);
        }
    }

    private void FixedUpdate(){
        x_offst = (player_rb.rotation.eulerAngles.y > 298.0f ? -1 *   (60 - (player_rb.rotation.eulerAngles.y - 300.0f)) : player_rb.rotation.eulerAngles.y);
        // Lerp Position
        if(((-0.050f * x_offst)) != lst_offst_x){
            desired_  = (player.position + offset);
            desired_.x = desired_.x +  ((-0.050f * x_offst));
            desired_.z = desired_.z +  (Math.Abs(x_offst)) / 60;
            lst_offst_x = ((-0.050f * x_offst));
        }

        tyro_on = FindObjectOfType<PlayerMovement>().plyr_tyro;
        sliding_on =  FindObjectOfType<PlayerMovement>().plyr_sliding;

        if(p_anim != null){
            if(p_anim.GetBool("Jump") || p_anim.GetBool("DoubleJump")){
                //supl_xRot = Mathf.SmoothDamp(supl_xRot, -0.07f, ref mathfRef, 0.52f);
                //supl_yOff = Mathf.SmoothDamp(supl_yOff, -2f, ref mathfRef, 0.52f);
            }else if(p_anim.GetBool("launcherJump") || p_anim.GetBool("bumperJump") || p_anim.GetBool("obstacleJump") || p_anim.GetBool("slide")){
                // supl_xRot = Mathf.SmoothDamp(supl_xRot, 0.04f, ref mathfRef, 0.4f);
                // supl_yOff = Mathf.SmoothDamp(supl_yOff, 0.25f, ref mathfRef, 0.4f);
            }else{
                supl_xRot = Mathf.SmoothDamp(supl_xRot, 0f, ref mathfRef, 0.52f);
                supl_yOff = Mathf.SmoothDamp(supl_yOff, 0f, ref mathfRef, 0.52f);
            }
        }
    }

    private void LateUpdate() {
       // transform.rotation.x = xRot
       
        if (!game_Over_){
            // Dampen towards the target rotation
            //Quaternion initial_rt  = new Quaternion(15, gameObject.transform.rotation.y, 0, 1);  
            Quaternion desired_rt  = new Quaternion(xRot + supl_xRot + wallR_rot_x_offst, (x_offst / 130.0f) + wallR_rot_y_offst, (x_offst / 10000.0f) + wallR_rot_z_offst, 1);
            transform.localRotation = Quaternion.Slerp(gameObject.transform.rotation, desired_rt,  tyro_on ? 0.07f : 0.15f);

            // Smooth Damp
            Vector3 smoothFollow = Vector3.SmoothDamp(
                transform.position,
                desired_ + (tyro_on ? new Vector3(0f, 0.5f, 3.0f) : new Vector3(0f,0f,0f)) + new Vector3(wallR_x_offst, wallR_y_offst + supl_yOff, wallR_z_offst),
                ref currentVelocity,
                tyro_on ? 0.15f : 0.07f
            ); 
            // Vector3 smoothFollow = Vector3.SmoothDamp(transform.position, desired_, ref currentVelocity, smoothTime *   Time.fixedDeltaTime); 

            transform.position = smoothFollow;
            //transform.position.x +=  ((-0.025f * x_offst))
        }else{
            transform.LookAt(player);
        }
    }


    private IEnumerator vert_trns_(bool loop_sns){
        float time = 1.25f;
        float from = 0.0f;
        Hashtable options = new Hashtable();
        if(loop_sns){
            float to = 1.0f;
            //LeanTween.value(gameObject, from, to, time, options);
        }else{
            float to = 1.0f;
            //LeanTween.value(gameObject, from, to, time, options);
        }
        yield return new WaitForSeconds(1f);
    }
    private static void updateOffst_Y(float hi){Debug.Log(hi);}   
 

    private IEnumerator vert_rt(bool loop_sns){
        float t_tick = vert_Trns / 120;
        float y_posOff_tick = 0.5f / 120;
        float y_posRot_tick = (1.25f / 120) * 0.5f;

        for (int i = 0; i < 120; i ++){
            if(loop_sns){
                if(vert_y_pos < 1.5f){vert_y_pos += y_posOff_tick;}
                //if(vert_y_rot < (1.25f * 0.00001f) ){vert_y_rot += y_posRot_tick;}
            }else{
                if(vert_y_pos > 0.02f){ vert_y_pos -= y_posOff_tick;}
                //if (vert_y_rot > 0.02f){ vert_y_rot -= y_posRot_tick;}
            }
            
            yield return new WaitForSeconds(t_tick);
            //offset.y = def_offset.y - vert_y_pos;
        }
        if(loop_sns){vert_y_rot -= y_posRot_tick/4;}else{
            vert_y_rot = 0f;
        }     
        //Debug.Log("pos " + vert_y_pos + " rot " + vert_y_rot);
    }



    // private void FovTrans(float fov_value, float t_){
    //     Camera c_ = gameObject.GetComponent<Camera>();
    //     if(c_){
    //         c_.fieldOfView = Mathf.Lerp(c_.fieldOfView, fov_value, t_);
    //     }
    // }

    // PUBLIC CAM VALUES TRANSITIONS FOR WALL RUN
    public void wal_rn_offset(bool is_ext, Transform gm_){
        if(is_ext){
            //FovTrans(85f, 0.5f);
            supl_xRot = 0.0f;
            new_fov = start_fov;
            wallR_x_offst = 0.0f; wallR_y_offst = 0.0f; wallR_z_offst = 0.0f;

            wallR_rot_z_offst = 0.0f; wallR_rot_y_offst = 0.0f; wallR_rot_x_offst = 0.0f;
        }else{
            //FovTrans(start_fov, 0.5f);
            new_fov = 90f;
            wallR_y_offst = -0.55f;
            // CLOSE UP Z OFFSET
            wallR_z_offst = 2.20f;
            float sns =  player.position.x - gm_.position.x;
            if(sns < 0 ){
                wallR_x_offst = -1.35f; 
                wallR_rot_y_offst = 0.150f; wallR_rot_z_offst = 0.14f;

            }else{
                wallR_x_offst = 1.35f;
                wallR_rot_y_offst = -0.150f; wallR_rot_z_offst = -0.14f;
            }
        }
    }


    // PUBLIC CAM VALUES TRANS FOR GRAPPLING
    public void grpl_offset(bool is_ext, Transform gm_ = null){
        if(is_ext){
            //FovTrans(85f, 0.5f);
            supl_xRot = 0.0f;
            new_fov = start_fov;
            wallR_x_offst = 0.0f; Invoke("delay_yOf_grpl", 0.55f); wallR_z_offst = 0.0f; wallR_rot_z_offst = 0.0f; wallR_rot_y_offst = 0.0f;wallR_rot_x_offst = 0.0f;
        }else{
            if(gm_){
                new_fov = 95f;
                float sns =  player.position.x - gm_.position.x;
                // CLOSE UP Z OFFSET
                wallR_z_offst = 1.50f;
                wallR_y_offst = -0.5f;

                wallR_rot_x_offst = 0.25f;

                // SMOOTH DAMP FOR X ROTATION boolean
                //wallR_rot_x_offst = Mathf.SmoothDamp(wallR_rot_x_offst, -0.10f, ref mathfRef, 3f);
                smthDmp_grpl = true;

                if(sns < 0 ) {wallR_x_offst = 0.3f; wallR_rot_z_offst = -0.07f;} else{
                    wallR_rot_z_offst = 0.07f;
                    wallR_x_offst = -0.3f; 
                }
            }
        }
    }
    private void delay_yOf_grpl(){smthDmp_grpl = false;wallR_y_offst = 0.0f;}

    // PUBLIC CAM VALUES TRANS FOR SLIDING
    public void sld_offset(bool is_ext){
        if(is_ext){
            //FovTrans(85f, 0.5f);
            supl_xRot = 0.0f;
            new_fov = start_fov;
            wallR_x_offst = 0.0f; wallR_y_offst = 0.0f; wallR_z_offst = 0.0f; wallR_rot_z_offst = 0.0f; wallR_rot_y_offst = 0.0f;wallR_rot_x_offst = 0.0f;
        }else{
            new_fov = 86f;
            // CLOSE UP Z OFFSET
            wallR_z_offst = 1.00f;
            wallR_y_offst = -0.35f;

            // SMOOTH DAMP FOR X ROTATION
            wallR_rot_x_offst = -0.090f;       
        }
    }

    // PUBLIC CAM VALUES TRANS FOR OBSTCL JMP
    public void obs_offset(){
        // SMOOTH DAMP FOR X ROTATION
        wallR_y_offst = 0.35f;
        wallR_rot_x_offst = 0.14f;    
        Invoke("obst_rst", 0.75f);   
    }
    private void obst_rst(){
        supl_xRot = 0.0f; wallR_x_offst = 0.0f; wallR_y_offst = 0.0f; wallR_z_offst = 0.0f; wallR_rot_z_offst = 0.0f; wallR_rot_y_offst = 0.0f;wallR_rot_x_offst = 0.0f;
    }
}
