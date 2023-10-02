using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
//using System.Collections.Generic;

public class CameraMovement : MonoBehaviour
{

    [Header ("Player Animator")]
    public GameObject p_gm;
    private Animator p_anim;

    [Header ("Player Inspector Values")]
    public Transform player;
    public Rigidbody player_rb;
    public Vector3 player_velocity;

    [Header ("Camera Offset Value")]
    public Vector3 offset;

    
    [Header ("Game State Value")]
    private bool game_Over_ = false;


    [Header ("Immediate Camera Transitions Values")]
    [HideInInspector] public float supl_xRot = 0.0f;
    [HideInInspector] public float supl_yOff = 0.0f;


    [Header ("SmoothDamp Values")]  
    private static IDictionary<string, float> rot_dc = new Dictionary<string, float>(){
        {"wallR_rot_x_offst", 0.0f  },
        {"wallR_rot_y_offst", 0.0f },  {"wallR_rot_z_offst",  0.0f }
    };
    private static IDictionary<string, float> pos_dc = new Dictionary<string, float>(){
        { "wallR_x_offst", 0.0f },
        { "wallR_y_offst", 0.0f },  {"wallR_z_offst", 0.0f}
    };




    [Header ("SmoothDamp Functions")]
    private int iterator_;
    private bool trns_fnc = false;
    private bool trns_back = false;
    private float trns_vlue;
    private List<float> values_flt = new List<float>(new float[6] {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f});
    private List<string> values_ref = new List<string>(new string[6]{ "wallR_x_offst", "wallR_y_offst", "wallR_z_offst", "wallR_rot_x_offst", "wallR_rot_y_offst", "wallR_rot_z_offst"});    
    private List<bool?> trns_back_arr = new List<bool?>(new bool?[6] {
        false, false, false ,false, false, false
    });    
    private List<string> nms_ = new List<string>(new string[3] {"wallR_x_offst", "wallR_y_offst", "wallR_z_offst"});
    private List<float> mathRef_arr = new List<float>(new float[6] {
        0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f
    }); 
    // ROTATIONS REFS
    private float mathfRef_x = 0.0f, mathfRef_y = 0.0f, mathfRef_z = 0.0f;
    private float mathfRef0_x = 0.0f, mathfRef0_y = 0.0f, mathfRef0_z = 0.0f;
    // POSITIONS REFS
    private float mathfRef_pos_x = 0.0f, mathfRef_pos_y = 0.0f, mathfRef_pos_z = 0.0f;
    private float mathfRef0_pos_x = 0.0f, mathfRef0_pos_y = 0.0f, mathfRef0_pos_z = 0.0f;
    // Percentage % 
    private float smoothTime_prc = 0f;


    [Header ("Fov Floats")]
    private float start_fov;
    private float new_fov;

    [Header ("Grapple SmoothDamp")]
    private bool smthDmp_grpl = false;
    private bool smthDmp_grpl_end = false;
    private float mathfRef_grpl = 0.0f;


    [Header ("Camera FixedUpdate & Lateupdate var")]
    private Vector3 desired_;
    private Quaternion desired_rt;
    private float xRot;
    private float lst_offst_x;
    private float x_offst = 0.0f;
    private Vector3 currentVelocity;

    [Header ("Player Actions Informations")]
    private bool tyro_on = false;
    private bool sliding_on = false;
    private bool rotate_back = false;

    [Header ("Grapple Point Position")]
    [HideInInspector] public Vector3 _grplPoint_;
    private bool end_trans_called = false;


    private Camera c_;

    private void Start(){

        p_anim = p_gm.GetComponentInChildren<Animator>();

        xRot = gameObject.transform.rotation.x;
        gameObject.transform.position = player.position + offset;
        
        desired_  = (player.position + offset);

        c_ = gameObject.GetComponent<Camera>();
        start_fov = gameObject.GetComponent<Camera>().fieldOfView;
        new_fov = start_fov;
    }






    private void Update()
    {
        if (!Input.GetKeyDown("q") && !Input.GetKeyDown("d") )
        {
            rotate_back = true;
        }else
        {
            rotate_back = false;
        }

        if(c_.fieldOfView != new_fov)
        {
            c_.fieldOfView = Mathf.Lerp(c_.fieldOfView, new_fov, 0.85f);
        }

        if(smthDmp_grpl) rot_dc["wallR_rot_x_offst"] = Mathf.SmoothDamp(rot_dc["wallR_rot_x_offst"], -0.08f, ref mathfRef_grpl, 0.295f);
        if(smthDmp_grpl_end) rot_dc["wallR_rot_x_offst"] = Mathf.SmoothDamp(rot_dc["wallR_rot_x_offst"], -0.74f, ref mathfRef_grpl, 0.295f);


    }





    private void FixedUpdate()
    {
        player_velocity = player_rb.velocity;

        tyro_on = FindObjectOfType<PlayerMovement>().plyr_tyro;
        sliding_on =  FindObjectOfType<PlayerMovement>().plyr_sliding;


        // Smooth damp function
        if(trns_fnc)
        {
            int it_ = 0;
            for (int i = 0; i < values_flt.Count; i++){
                if( values_flt[i] == 0.0f) break;
                
                bool s_ =  nms_.Contains(values_ref[i]);
                if(!s_){
                    // Debug.Log( ( !s_ ? rot_dc[values_ref[i]] : pos_dc[values_ref[i]] ) +
                    //             " vs " + values_ref[i] );
                }

                if(trns_back_arr[i] == null)
                {
                    it_ ++;
                }else 
                {

                    if(!s_) // ROTATIONS MOVEMENTS
                    {
                        float srched_ref =( trns_back_arr[i] == true ?  
                         (values_ref[i] == "wallR_rot_x_offst"  ? (mathfRef_x) : (values_ref[i] == "wallR_rot_y_offst" ? mathfRef_y : mathfRef_z) )
                            :
                          (values_ref[i] == "wallR_rot_x_offst"  ? (mathfRef0_x) : (values_ref[i] == "wallR_rot_y_offst" ? mathfRef0_y : mathfRef0_z) )
                        );


                        if(trns_back_arr[i] == false)
                        {
                            rot_dc[values_ref[i]] = Mathf.SmoothDamp( rot_dc[values_ref[i]], values_flt[i], ref srched_ref, 0.0635f * (1.0f + (smoothTime_prc/100) ) ); 

                            if( Math.Abs(rot_dc[values_ref[i]]) >= Math.Abs(values_flt[i]) - 0.0045f) {
                                trns_back_arr[i] = true;
                            }
                        }
                        else if (trns_back_arr[i] == true)
                        { 
                            rot_dc[values_ref[i]] = Mathf.SmoothDamp(rot_dc[values_ref[i]], 0.00f, ref srched_ref, 0.0460f * (1.0f + (smoothTime_prc/100) ) ); 
                            //if( rot_dc[values_ref[i]] == 0.0f) {
                            if(Math.Abs(rot_dc[values_ref[i]]) < 0.0035f ){
                                it_++;
                                trns_back_arr[i] = null;
                                //Debug.Log("rotation done");
                            }
                        }  
                    }
                    else // POSITIONS MOVEMENTS 
                    {
                        float srched_ref =( trns_back_arr[i] == true ?  
                         (values_ref[i] == "wallR_x_offst"  ? (mathfRef_pos_x) : (values_ref[i] == "wallR_y_offst" ? mathfRef_pos_y : mathfRef_pos_z) )
                            :
                          (values_ref[i] == "wallR_x_offst"  ? (mathfRef0_pos_x) : (values_ref[i] == "wallR_y_offst" ? mathfRef0_pos_y : mathfRef0_pos_z) )
                        );



                        if(trns_back_arr[i] == false)
                        {
                            pos_dc[values_ref[i]] = Mathf.SmoothDamp( pos_dc[values_ref[i]], values_flt[i], ref srched_ref, 0.060f * (0.8f + (smoothTime_prc/100) ) );
                            if( Math.Abs(pos_dc[values_ref[i]]) >= Math.Abs(values_flt[i]) - 0.003f ) { 
                                trns_back_arr[i] = true; 
                            }
                        }
                        else if (trns_back_arr[i] == true)
                        { 
                            pos_dc[values_ref[i]] = Mathf.SmoothDamp(pos_dc[values_ref[i]], 0.00f, ref srched_ref, 0.040f * (0.8f + (smoothTime_prc/100) ) ); 
                            if(Math.Abs(pos_dc[values_ref[i]]) < 0.0070f ){
                            //if( pos_dc[values_ref[i]] == 0.0f){
                                it_++;
                                trns_back_arr[i] = null;
                                //Debug.Log("position done");
                            }
                        }  
                    }    

                }
      
                if (it_ == iterator_) trns_fnc = false; 
                if(!trns_fnc)
                {
                    //Debug.Log("WHOLE RESET !");
                    reset_smoothDmpfnc();
                    break;
                }
            }
        }





        x_offst = (player_rb.rotation.eulerAngles.y > 298.0f ? -1 *   (60 - (player_rb.rotation.eulerAngles.y - 300.0f)) : player_rb.rotation.eulerAngles.y);

        // Lerp Position
        if(((-0.043f * x_offst)) != lst_offst_x)
        {
            desired_  = (player.position + offset);
            desired_.x = desired_.x +  ((-0.043f * x_offst));
            desired_.z = desired_.z +  (Math.Abs(x_offst)) / 80f;
            lst_offst_x = ((-0.43f * x_offst));
        }

        if(_grplPoint_ != new Vector3(0,0,0))
        {
            if( !end_trans_called && (player.position.z > _grplPoint_.z + 2.0f) )
            {
                end_grpl_Cm();
            }
        } else { end_trans_called = false; }


    }




    private void LateUpdate()
    {
        if (!game_Over_)
        {
            // Dampen towards the target rotation
            Quaternion desired_rt  = new Quaternion(xRot + supl_xRot + rot_dc["wallR_rot_x_offst"],
                 (x_offst / 105.0f) + rot_dc["wallR_rot_y_offst"], 
                (x_offst / 10000.0f) + rot_dc["wallR_rot_z_offst"],
                 1
            );

            transform.localRotation = Quaternion.Slerp(gameObject.transform.rotation, desired_rt,  tyro_on ? 0.07f : 0.12f);

            // Smooth Damp
            Vector3 smoothFollow = Vector3.SmoothDamp(
                transform.position,
                desired_ + (tyro_on ? new Vector3(0f, 0.5f, 3.0f) : new Vector3(0f,0f,0f)) + new Vector3(pos_dc["wallR_x_offst"], pos_dc["wallR_y_offst"] + supl_yOff, pos_dc["wallR_z_offst"]),
                ref currentVelocity,
                tyro_on ? 0.15f : 0.055f
            ); 

            transform.position = smoothFollow;
        }else{
            transform.LookAt(player);
        }
    }




    public void cam_GamerOver_()
    {
        game_Over_ = true;
    }


    private void reset_smoothDmpfnc()
    {
        smoothTime_prc = 0.0f;
        mathfRef_x = mathfRef_y = mathfRef_z = mathfRef0_x = mathfRef0_y = mathfRef0_z = mathfRef_pos_x = mathfRef_pos_y = mathfRef_pos_z = mathfRef0_pos_x = mathfRef0_pos_y = mathfRef0_pos_z = 0.0f;
        mathRef_arr = new List<float>(new float[6] {
            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f
        }); 
        trns_fnc = false;
        for(int b = 0; b < iterator_; b ++)
        {
            bool s_ =  nms_.Contains(values_ref[b]);
            if(s_) rot_dc[values_ref[b]] = 0.0f;
            else pos_dc[values_ref[b]] = 0.0f;
        }
    }



    // PUBLIC CAM VALUES TRANSITIONS FOR WALL RUN
    public void wal_rn_offset(bool is_ext, Transform gm_){
        if(is_ext){
            //FovTrans(85f, 0.5f);
            supl_xRot = 0.0f;
            new_fov = start_fov;
            pos_dc["wallR_x_offst"] = 0.0f; pos_dc["wallR_y_offst"] = 0.0f; pos_dc["wallR_z_offst"] = 0.0f;

            rot_dc["wallR_rot_z_offst"] = 0.0f; rot_dc["wallR_rot_y_offst"] = 0.0f; rot_dc["wallR_rot_x_offst"] = 0.0f;
        }else{
            // // //
            reset_smoothDmpfnc();
            // // //

            //FovTrans(start_fov, 0.5f);
            new_fov = 90f;
            pos_dc["wallR_y_offst"] = -0.55f;

            // CLOSE UP Z OFFSET
            pos_dc["wallR_z_offst"] = 1.90f;

            float sns =  player.position.x - gm_.position.x;
            if(sns < 0 ){
                pos_dc["wallR_x_offst"] = -1.35f; 
                rot_dc["wallR_rot_y_offst"] = 0.250f; rot_dc["wallR_rot_z_offst"] = 0.14f;

            }else{
                pos_dc["wallR_x_offst"] = 1.35f;
                rot_dc["wallR_rot_y_offst"] = -0.250f; rot_dc["wallR_rot_z_offst"] = -0.14f;
            }
        }
    }




    // PUBLIC CAM VALUES TRANS FOR GRAPPLING
    public void grpl_offset(bool is_ext, Transform gm_ = null)
    {
        if(is_ext)
        {
            supl_xRot = 0.0f;
            new_fov = start_fov;
          
            pos_dc["wallR_x_offst"] = 0.0f; Invoke("delay_yOf_grpl", 0.55f); pos_dc["wallR_z_offst"] = 0.0f;
            rot_dc["wallR_rot_z_offst"] = 0.0f; rot_dc["wallR_rot_y_offst"] = 0.0f; rot_dc["wallR_rot_x_offst"] = 0.0f;
        }else
        {
            if(gm_)
            {
                // // //
                reset_smoothDmpfnc();
                // // //

                new_fov = 95f;

                // CLOSE UP Z OFFSET
                pos_dc["wallR_z_offst"] = 1.45f;
                pos_dc["wallR_y_offst"] = -0.65f;

                rot_dc["wallR_rot_x_offst"] = -0.28f;

                // SMOOTH DAMP FOR X ROTATION boolean
                smthDmp_grpl = true;

                float sns =  player.position.x - gm_.position.x;
                if(sns < 0 ) 
                { 
                    pos_dc["wallR_x_offst"] = -0.6f; 
                    rot_dc["wallR_rot_y_offst"] = 0.15f;
                    rot_dc["wallR_rot_z_offst"] = -0.085f;
                }
                else
                {
                    pos_dc["wallR_x_offst"] = 0.6f; 
                    rot_dc["wallR_rot_y_offst"] = -0.15f;
                    rot_dc["wallR_rot_z_offst"] = 0.085f; 
                }

            }
        }
    }
    private void delay_yOf_grpl()
    {
        _grplPoint_ = new Vector3(0,0,0);
        smthDmp_grpl_end = false; smthDmp_grpl = false;
        rot_dc["wallR_y_offst"] = 0f;
    }




    // END GRAPPLING CAMERA MOVEMENT TRANSITION
    private void end_grpl_Cm()
    {
        FindObjectOfType<Grappling>().soft_Grapple();
        end_trans_called = true;
        // SPACE UP Z OFFSET
        rot_dc["wallR_z_offst"] = 0.0f;

        rot_dc["wallR_y_offst"] = -1.70f;
        // CANCEL smthDmp_grpl PREVIOUS SMOOTH DAMP X ROTATION
        smthDmp_grpl = false;
        smthDmp_grpl_end = true;
    }




    // PUBLIC CAM VALUES TRANS FOR SLIDING
    public void sld_offset(bool is_ext)
    {
        if(is_ext)
        {
            //FovTrans(85f, 0.5f);
            supl_xRot = 0.0f;
            new_fov = start_fov;
            pos_dc["wallR_x_offst"] = 0.0f; pos_dc["wallR_y_offst"] = 0.0f; pos_dc["wallR_z_offst"] = 0.0f;
            rot_dc["wallR_rot_z_offst"] = 0.0f; rot_dc["wallR_rot_y_offst"] = 0.0f; rot_dc["wallR_rot_x_offst"] = 0.0f;

        }else
        {
            reset_smoothDmpfnc();

            new_fov = 86f;
            // CLOSE UP Z OFFSET
            pos_dc["wallR_z_offst"] = 0.77f;
            pos_dc["wallR_y_offst"] = -0.35f;

            // SMOOTH DAMP FOR X ROTATION
            rot_dc["wallR_rot_x_offst"] = -0.090f;       
        }
    }




    // PUBLIC CAM VALUES TRANS FOR OBSTCL JMP
    public void obs_offset()
    {
        // SMOOTH DAMP FOR X ROTATION
        pos_dc["wallR_y_offst"] = 0.35f;
        rot_dc["wallR_rot_x_offst"] = 0.14f;  
        reset_smoothDmpfnc();  
        Invoke("obst_rst", 0.75f);   
    }
    private void obst_rst()
    {
        supl_xRot = 0.0f;
        pos_dc["wallR_x_offst"] = 0.0f; pos_dc["wallR_y_offst"] = 0.0f; pos_dc["wallR_z_offst"] = 0.0f;
        rot_dc["wallR_rot_z_offst"] = 0.0f; rot_dc["wallR_rot_y_offst"] = 0.0f; rot_dc["wallR_rot_x_offst"] = 0.0f;
    }




    // PUBLIC FUNC FOR JUMP SUPER SMOOTH TRANS
    public void jmp(bool is_dblJmp)
    {
        // wallR_rot_x_offst = -0.052f; 
        // wallR_y_offst = -0.20f;    
        reset_smoothDmpfnc();  

        iterator_ = 3;
        List<float> v_flt = new List<float>(new float[6] {-0.170f, -1.10f, is_dblJmp ?  -0.0550f : 0.0550f, 0.0f, 0.0f, 0.0f} ); 
        List<string> s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_y_offst", "wallR_rot_z_offst", "", "",""} ); 
 
        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] {
            false, false, false ,false, false, false
        }); 

        trns_fnc = true;
        trns_back = false;
      
    }
    
    // PUBLIC FUNC FOR [LAUNCHER, BUMPER, TAP TAP] 
    public void special_jmp()
    { 
        reset_smoothDmpfnc();  

        // +20% smoothTime !!
        smoothTime_prc = 30f;

        iterator_ = 3;
        List<float> v_flt = new List<float>(new float[6] {0.170f, 1.40f, 0.040f, 0.0f, 0.0f, 0.0f} ); 
        List<string> s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_y_offst", "wallR_rot_z_offst", "", "",""} ); 
 
        values_ref = s_arr; values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] {
            false, false, false ,false, false, false
        }); 

        trns_fnc = true;
        trns_back = false;
      
    }


    // private void array_pnt_Constructor(ref List<float> p,int index, ref float p){
    //     List<float> v_flt = new List<float>(new float[6] {-0.052f, -0.20f, 0.0f, 0.0f, 0.0f, 0.0f} ); 

    //     values_ref[index] = p;
    //     switch(v_ref_names[z]){
    //             case "x_off_rot": 
    //                 array_pnt_Constructor(z, ref wallR_rot_x_offst);
    //                 break;
    //             case "y_off_rot":
    //                 array_pnt_Constructor(z, ref wallR_rot_y_offst);
    //                 break;
    //             case "z_off_rot":
    //                 array_pnt_Constructor(z, ref wallR_rot_z_offst);
    //                 break;

    //             case "z_off":
    //                 array_pnt_Constructor(z, ref wallR_x_offst);
    //                 break;
    //             case "y_off":
    //                 array_pnt_Constructor(z, ref wallR_y_offst);
    //                 break;
    //             case "x_off":
    //                 array_pnt_Constructor(z, ref wallR_z_offst);
    //                 break;

    //         }
    // }
}
