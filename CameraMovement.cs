using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
//using System.Collections.Generic;

public class CameraMovement : MonoBehaviour
{
    [Header ("Camera Smooth Auto-Aim divisor")]
    private const float divisor = 60f;

    [Header ("Camera Main Rotation Ratio")]
    private const float x_ratio = -0.0620f;


    [Header ("Player Animator")]
    public GameObject p_gm;
    private Animator p_anim;

    [Header ("Player Inspector Values")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody player_rb;
    [SerializeField] private Vector3 player_velocity;

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
    private float end_mathfRef_grpl = 0.0f;


    [Header ("Camera FixedUpdate & Lateupdate var")]
    private Vector3 desired_;
    private Quaternion desired_rt;
    private float xRot;
    private float lst_offst_x;
    private float x_offst = 0.0f;
    private Vector3 currentVelocity;

    [Header ("Player Actions Informations")]
    private float tyro_z = 0f;
    private bool tyro_on = false;
    private bool slideRail_on = false;
    private bool grappl_on = false;
    private bool sliding_on = false;
    private bool rotate_back = false;

    [Header ("Grapple Point Position")]
    [HideInInspector] public Vector3 _grplPoint_;
    private bool end_trans_called = false;


    [Header ("Aimed Target")]
    private Transform aimed_target;
    public Transform set_aimedTarget  
    {
        get { return null; } 
        set {
            if(value == null) {
                aimed_target = null;
                return;
            }
            if( (value.GetType() == typeof(Transform))) aimed_target = value; }  // set method
    }
    public void rst_aimedTarget()
    { aimed_target = null; }
    [SerializeField] private float aim_x, aim_y;

    private Camera c_;

    private PlayerMovement plyr_mv;

    [Header ("Hang Transition")]
    [HideInInspector] public Transform hang_point;
    [HideInInspector] public Transform stored_picoParent;
    private Vector3 hangVelocity;

    [Header ("SlowTime")]
    private bool slow_time = false;

    [Header ("Momentm")]
    private float plyr_Momentum;

    [Header ("Recoil")]
    private float cam_recoil = 0f;

    [Header ("Gravity Y-Effect")]
    private float x_gravity_ = 0f;

    [Header ("Camera Bob")]
    [HideInInspector] public bool can_headBob = false;
    private float[] x_y = new float[2]{ 0.05f, 0.10f };
    private int bobEffect = 225;
    private const double t_Pi = 6.28319;
    private const double Pi = 3.14159;
    private double bob_rad = 0;
    private double pi_toRad { get {return 0; }
        set { if((value).GetType() == typeof(double)) 
            {coef = (( (Math.PI) / 180f) *(value)); }
        }
    }
    [SerializeField] private double coef = 0;

    private void Start()
    {
        plyr_mv = FindObjectOfType<PlayerMovement>();

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

        // field of view
        if(c_.fieldOfView != (new_fov + plyr_Momentum))
        {
            c_.fieldOfView = Mathf.Lerp(c_.fieldOfView, (new_fov + plyr_Momentum), 0.85f);
        }


        if(smthDmp_grpl) 
            rot_dc["wallR_rot_x_offst"] = Mathf.SmoothDamp(rot_dc["wallR_rot_x_offst"], 0.12f, ref mathfRef_grpl, 0.295f);
            
        if(smthDmp_grpl_end)
            rot_dc["wallR_rot_x_offst"] = Mathf.SmoothDamp(rot_dc["wallR_rot_x_offst"], -0.325f, ref mathfRef_grpl, 0.650f);



        // recoil
        if(cam_recoil > 0 && cam_recoil  < (0.03f) * Time.deltaTime)
            cam_recoil -= (0.00002f) * Time.deltaTime;
            
        else if ( (cam_recoil < 0 ) && (cam_recoil > (-1 *  (0.03f) * Time.deltaTime)) ) 
            cam_recoil -= (0.0002f) * Time.deltaTime;



        // aim effect
        Vector3 player_target_relativePos = (
            (aimed_target != null) ? 
            (aimed_target.position - player.position) : (Vector3.zero)
        );
        Quaternion aiming_rotation = (
            (aimed_target != null) ? 
            (Quaternion.LookRotation(player_target_relativePos)) : (Quaternion.identity)
        ); 
        float euler_y_fixed = aiming_rotation.eulerAngles.y > 180 ?  (aiming_rotation.eulerAngles.y - 360f) : (aiming_rotation.eulerAngles.y);
        float euler_x_fixed = aiming_rotation.eulerAngles.x > 180f ?  (aiming_rotation.eulerAngles.x - 360f) : (aiming_rotation.eulerAngles.x);
        aim_y = Mathf.Lerp(aim_y, euler_y_fixed, Time.deltaTime * 1.05f);
        aim_x = Mathf.Lerp(aim_x, euler_x_fixed, Time.deltaTime * 1.05f);
            

        if(true == false)
        {
            // bob effect
            // +π
            bob_rad += bobEffect * Time.deltaTime;
            // 2π reset
            if( (coef > 1f && coef > 0f) || (coef < -1f && coef < 0f) )
            {
                bobEffect = -1 * bobEffect;
                bob_rad += (5 * bobEffect) * Time.deltaTime;
            }
            coef = (( (Math.PI) / 180f) * (bob_rad));
        }
    }


    

    private void FixedUpdate()
    {
        player_velocity = player_rb.velocity;
        plyr_Momentum = plyr_mv.get_Momentum;
        
        tyro_on = plyr_mv.plyr_tyro;
        sliding_on =  plyr_mv.plyr_sliding;

        if(tyro_on != plyr_mv.plyr_tyro)
        {
            int zz = UnityEngine.Random.Range(1, 3);
            if(zz == 1) tyro_z = 0.1f; 
            else tyro_z = -0.1f; 
        }


        // Smooth damp function
        if(trns_fnc)
        {
            int it_ = 0;
            for (int i = 0; i < values_flt.Count; i++)
            {
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
                            if(Math.Abs(rot_dc[values_ref[i]]) < 0.0040f ){
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
                    reset_smoothDmpfnc();
                    break;
                }
            }
        }




        // x_offst definition
        x_offst = (player_rb.rotation.eulerAngles.y > 298.0f ? -1 *   (60 - (player_rb.rotation.eulerAngles.y - 300.0f)) : player_rb.rotation.eulerAngles.y);

        // Lerp Position
        if(((x_ratio * x_offst)) != lst_offst_x)
        {
            desired_  = (player.position + offset);
            desired_.x = desired_.x +  (
                (x_ratio * (tyro_on ?
                    x_offst * 2 : 
                    (x_offst/2.5f)
                ))
            );
            desired_.z = desired_.z +  (Math.Abs(x_offst)) / 80f;
            lst_offst_x = ((x_ratio * x_offst));
        }


        // end grapl transition detection
        if(_grplPoint_ != new Vector3(0,0,0))
        {
            if( !end_trans_called && (player.position.z > _grplPoint_.z + 2.0f) )
            {
                end_grpl_Cm();
            }
        }
        else { end_trans_called = false; }


        // time
        if(slow_time)
        {
            Time.timeScale = 0.75f;
        }
        else { Time.timeScale = 1f; }

    }



    // cam smoothdamps & lerps
    private void LateUpdate()
    {
        if (!game_Over_)
        {
            if(!plyr_mv.plyr_hanging)
            {
                float xy_ratio = 0.0590f;
                
                // Dampen towards the target rotation
                bool aim_off =  (aimed_target == null || tyro_on ) ? true : false;

                Quaternion desired_rt  = new Quaternion( 
                    xRot + supl_xRot + rot_dc["wallR_rot_x_offst"] 
                     + ( (aim_off) ? 0 : (aim_x * xy_ratio) / (divisor * 0.35f))
                      + ( (player_velocity.y < -1) ? ( player_velocity.y/240 ) : 0f)
                    + cam_recoil
                    ,
                    (x_offst / (sliding_on ? 120f : 160.0f) ) + rot_dc["wallR_rot_y_offst"] 
                     + ( (aim_off) ? 0 : (aim_y * xy_ratio) / divisor)
                    , 
                    (-1 * (x_offst / 360.0f)) + rot_dc["wallR_rot_z_offst"] + (tyro_on ? (tyro_z) : 0f)
                     + cam_recoil
                    ,
                    1f
                );

                transform.localRotation = Quaternion.Slerp(gameObject.transform.rotation, desired_rt,  (tyro_on || grappl_on) ? (grappl_on ? 0.070f : 0.020f) : 0.12f );
                
                // Smooth Damp
                Vector3 smoothFollow = Vector3.SmoothDamp(
                    transform.position,
                    (
                        desired_ + 
                            (tyro_on ? new Vector3(0f, 0.5f, 0.5f) : new Vector3(0f,0f,0f)) 
                        + 
                        new Vector3(pos_dc["wallR_x_offst"], pos_dc["wallR_y_offst"] + supl_yOff, pos_dc["wallR_z_offst"])
                        + new Vector3(0f, ( (player_velocity.y < -1) ? (player_velocity.y/14) : 0f), 0f) // y compensation
                        + new Vector3( (float)(x_y[0] * coef),(float) Math.Abs(x_y[1] * coef), 0f) // bob
                        + new Vector3(0, 0, (player_rb.velocity.z > 8 ) ? (player_rb.velocity.z - 8) * 0.05f : 0f ) // z speed compensation
                    ),
                    ref currentVelocity,
                    ( grappl_on ? 
                        0.0710f 
                    :
                        (tyro_on) ? 0.080f : 0.065f
                    )
                ); 

                transform.position = smoothFollow;

                if(tyro_on) 
                    transform.LookAt( player.position + new Vector3(0, 1.25f, 0) );

            }else
            {
                // Smooth hang
                Vector3 smoothHang = Vector3.SmoothDamp(
                    transform.position, 
                    hang_point.position + ( new Vector3(0f, 2f, 1.5f)), 
                    ref hangVelocity,
                    0.2f
                ); 
                transform.position = smoothHang;

                // Dampen towards relative pico target rotation
                Vector3 dir = (stored_picoParent.position + new Vector3(0f, 1f, 0f) ) - transform.position;
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, dir + new Vector3(0f, -2f, 0f), Time.deltaTime * 2f, 0.0f);
                Quaternion l_R = Quaternion.LookRotation(newDirection);

                transform.localRotation = Quaternion.Slerp(transform.rotation, l_R, 0.12f); 
                
            }
        }else{
            // game over cam
            transform.LookAt(player);
        }

    }





    // game OVR
    public void cam_GamerOver_()
    {
        Invoke("gm_over_call", 1.5f);
        special_jmp();
    } private void gm_over_call(){ game_Over_ = true;}


  
  
  
    private void reset_smoothDmpfnc()
    {
        smoothTime_prc = 0.0f;
        // new_fov = start_fov;
        mathfRef_x = mathfRef_y = mathfRef_z = mathfRef0_x = mathfRef0_y = mathfRef0_z = mathfRef_pos_x = mathfRef_pos_y = mathfRef_pos_z = mathfRef0_pos_x = mathfRef0_pos_y = mathfRef0_pos_z = 0.0f;
        mathRef_arr = new List<float>(new float[6] {
            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f
        }); 
        trns_fnc = false;
        for(int b = 0; b < 6; b ++)
        {
            bool s_ =  nms_.Contains(values_ref[b]);
            if(!s_) rot_dc[values_ref[b]] = 0.0f;
            else pos_dc[values_ref[b]] = 0.0f;
        }
        iterator_ = 0;
    }




    // Wall run
    public void wal_rn_offset(bool is_ext, Transform gm_, float y_bonuss = 0.0f)
    {
        if(is_ext)
        {
            supl_xRot = 0.0f;
            new_fov = start_fov;
            pos_dc["wallR_x_offst"] = 0.0f; pos_dc["wallR_y_offst"] = 0.0f; pos_dc["wallR_z_offst"] = 0.0f;

            rot_dc["wallR_rot_x_offst"] = 0.0f; rot_dc["wallR_rot_z_offst"] = 0.0f; rot_dc["wallR_rot_y_offst"] = 0.0f;
        }else
        {
            // // //
            reset_smoothDmpfnc();
            // // //

            float sns =  Math.Abs(player.position.x) - Math.Abs(gm_.position.x);

            //FovTrans(start_fov, 0.5f);
            new_fov = 93f;
            pos_dc["wallR_y_offst"] = sns > 0 ? -1f : -0.75f;

            // SPACE UP || CLOSE UP Z OFFSET
            pos_dc["wallR_z_offst"] = (sns < 0) ? 0.8f :  0.4f;

            // ROTATE TOP
            rot_dc["wallR_rot_x_offst"] = -0.135f;

            rot_dc["wallR_rot_z_offst"] = sns > 0 ? 0.13f : -0.13f;

            if(sns > 0 )
            {
                // right wall hit
                pos_dc["wallR_x_offst"] = 0.2f + (y_bonuss / 22); 
                rot_dc["wallR_rot_y_offst"] = -0.210f + (-1 * (y_bonuss / 92));

            }else
            {
                // left wall hit
                pos_dc["wallR_x_offst"] = 2f + ( -1 * (y_bonuss / 22) );
                rot_dc["wallR_rot_y_offst"] = -0.35f + 1 * (y_bonuss / 92); 
               
            }
        }
    }






    // Grappling
    public void grpl_offset(bool is_ext, Transform gm_ = null)
    {
        if(is_ext)
        {
            supl_xRot = 0.0f;
            new_fov = start_fov;
            grappl_on = false;

            pos_dc["wallR_x_offst"] = 0.0f; Invoke("delay_yOf_grpl", 0.55f); pos_dc["wallR_z_offst"] = 0.0f;
            rot_dc["wallR_rot_z_offst"] = 0.0f; rot_dc["wallR_rot_y_offst"] = 0.0f; rot_dc["wallR_rot_x_offst"] = 0.0f;
        }else
        {
            if(gm_)
            {
                // // //
                reset_smoothDmpfnc();
                // // //
                grappl_on = true;
                new_fov = 95f;

                // CLOSE UP Z OFFSET
                pos_dc["wallR_z_offst"] = 1.45f;
                pos_dc["wallR_y_offst"] = -0.40f;

                // SMOOTH DAMP FOR X ROTATION boolean
                smthDmp_grpl = true;

                float sns =  player.position.x - gm_.position.x;
                if(sns < 0 )rot_dc["wallR_rot_z_offst"] = -0.04f;
                else rot_dc["wallR_rot_z_offst"] = 0.04f; 
                

            }
        }
    }
    private void delay_yOf_grpl()
    {
        _grplPoint_ = new Vector3(0,0,0);
        smthDmp_grpl_end = false; smthDmp_grpl = false;
        rot_dc["wallR_y_offst"] = 0f;
    }
    // End grapple
    private void end_grpl_Cm()
    {
        FindObjectOfType<Grappling>().waveHeight *= 4;
        FindObjectOfType<Grappling>().soft_Grapple();
        end_trans_called = true;
        // SPACE UP Z OFFSET
        pos_dc["wallR_z_offst"] = 1.0f;

        pos_dc["wallR_x_offst"] = rot_dc["wallR_rot_y_offst"] = 0f;
        pos_dc["wallR_y_offst"] = -1.90f;
        // CANCEL smthDmp_grpl PREVIOUS SMOOTH DAMP X ROTATION
        smthDmp_grpl = false; smthDmp_grpl_end = true;
    }






    // Sliding
    public void sld_offset(bool is_ext)
    {
        if(is_ext)
        {
            supl_xRot = 0.0f;
            new_fov = start_fov;
            pos_dc["wallR_y_offst"] = 0.0f; pos_dc["wallR_z_offst"] = 0.0f;
            rot_dc["wallR_rot_x_offst"] = 0.0f; rot_dc["wallR_rot_z_offst"] = 0.0f;       

        }else
        {
            reset_smoothDmpfnc();

            new_fov = 90f;
            
            float s_ = UnityEngine.Random.Range(1, 3);

            // CLOSE UP Z OFFSET
            pos_dc["wallR_z_offst"] = s_ == 1 ? 1.5f : 1.9f;
            pos_dc["wallR_y_offst"] = s_ == 1 ? 0.4f : 0.6f;

            // SMOOTH DAMP FOR X ROTATION
            rot_dc["wallR_rot_x_offst"] = s_ == 1 ? 0.04f : 0.12f;    
            rot_dc["wallR_rot_z_offst"] = -0.110f;       
  
        }
    }


    // Rail Slide
    public void railSlide_offset(bool is_ext)
    {
        if(is_ext)
        {
            //FovTrans(85f, 0.5f);
            supl_xRot = 0.0f;
            new_fov = start_fov;
            pos_dc["wallR_y_offst"] = 0.0f; pos_dc["wallR_z_offst"] = 0.0f; rot_dc["wallR_rot_x_offst"] = 0.0f; rot_dc["wallR_rot_z_offst"] = 0.0f;
            pos_dc["wallR_x_offst"] = 0.0f;
       
        }else
        {
            reset_smoothDmpfnc();
            new_fov = 82f;
            // SPACE + OFFSET
            pos_dc["wallR_x_offst"] = 0.5f;
            pos_dc["wallR_y_offst"] = -0.35f;
            pos_dc["wallR_z_offst"] = 0.5f;

            rot_dc["wallR_rot_z_offst"] = -0.120f;       
            rot_dc["wallR_rot_x_offst"] = -0.050f;       

        }
    }

    // Ladder
    public void ladderClimb_offst(bool is_ext)
    {
        if(is_ext)
        {
            supl_xRot = 0.0f;
            new_fov = start_fov;
            pos_dc["wallR_y_offst"] = 0.0f; pos_dc["wallR_z_offst"] = 0.0f; rot_dc["wallR_rot_x_offst"] = 0.0f; rot_dc["wallR_rot_z_offst"] = 0.0f;
            pos_dc["wallR_x_offst"] = 0.0f;
       
        }else
        {
            reset_smoothDmpfnc();

            new_fov = 82f;
            // SPACE Z+
            pos_dc["wallR_z_offst"] = 0.5f;

            pos_dc["wallR_y_offst"] = 0.5f;

            // SMOOTH DAMP FOR X ROTATION
            rot_dc["wallR_rot_z_offst"] = -0.120f;       
            rot_dc["wallR_rot_x_offst"] = 0.050f;       
        }
    }



    // Obstcl Jump
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
        pos_dc["wallR_y_offst"] = 0.0f;rot_dc["wallR_rot_x_offst"] = 0.0f;
    }


    // tyro
    public void tyro_offset(bool is_ext)
    {
        if(is_ext)
        {
            supl_xRot = 0.0f;
            new_fov = start_fov;
            pos_dc["wallR_z_offst"] = 0.0f;

        }else
        {
            reset_smoothDmpfnc();

            new_fov = 88f;
            // SPACE UP Z OFFSET
            pos_dc["wallR_z_offst"] = -0.77f;

        }
    }



    // Ramp sliding
    public void rmp_slid_offst(bool is_ext, Transform gm_)
    {
        if(is_ext)
        {
            //FovTrans(85f, 0.5f);
            supl_xRot = 0.0f;
            new_fov = start_fov;
            pos_dc["wallR_y_offst"] = 0.0f; pos_dc["wallR_z_offst"] = 0.0f;
            rot_dc["wallR_rot_x_offst"] = 0.0f; rot_dc["wallR_rot_z_offst"] = 0.0f;
        }else
        {
            // // //
            reset_smoothDmpfnc();
            // // //

            //FovTrans(start_fov, 0.5f);
            new_fov = 84f;
            pos_dc["wallR_y_offst"] = 0.8f;

            // SPACE UP || CLOSE UP Z OFFSET
            pos_dc["wallR_z_offst"] = 1f;

            // ROTATE X
            rot_dc["wallR_rot_x_offst"] = 0.070f;
            rot_dc["wallR_rot_z_offst"] = 0.04f;

        }
    }



   // Fall Down
    public void fall_Down(bool is_ext)
    {
        if(is_ext)
        {
            //FovTrans(85f, 0.5f);
            supl_xRot = 0.0f;
            new_fov = start_fov;
            pos_dc["wallR_y_offst"] = 0.0f; pos_dc["wallR_z_offst"] = 0.0f; rot_dc["wallR_rot_x_offst"] = 0.0f; rot_dc["wallR_rot_z_offst"] = 0.0f;
            pos_dc["wallR_x_offst"] = 0.0f;
       
        }else
        {
            reset_smoothDmpfnc();
            new_fov = 86f;
            // SPACE + OFFSET
            pos_dc["wallR_y_offst"] = 0.9f;
            pos_dc["wallR_z_offst"] = 1.2f;

            rot_dc["wallR_rot_x_offst"] = 0.20f;       

        }
    }







    // Jump & DoubleJump
    public void jmp(bool is_dblJmp)
    {
        reset_smoothDmpfnc();  

        // -30% smoothTime !!
        smoothTime_prc = 0f;

        iterator_ = 4;
        List<float> v_flt = new List<float>(new float[6] {
            is_dblJmp ? -0.14f : -0.25f, is_dblJmp ? -0.9f : -1.7f, is_dblJmp ?  -0.12f : 0.12f, 
            0.0f, 0.0f, 0.0f
        } ); 
        List<string> s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_y_offst", "wallR_rot_z_offst", "", "",""} ); 
 
        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] {
            false, false, false ,false, false, false
        }); 

        trns_fnc = true;
        trns_back = false;
        StartCoroutine(time_coroutine());
    }
    

    // Special Jump [LAUNCHER, BUMPER, TAP TAP] 
    public void special_jmp()
    { 
        reset_smoothDmpfnc();  

        // +30% smoothTime !!
        smoothTime_prc = 30f;

        iterator_ = 3;
        List<float> v_flt = new List<float>(new float[6] {0.170f, 1.40f, 0.040f, 0.0f, 0.0f, 0.0f} ); 
        List<string> s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_y_offst", "wallR_rot_z_offst", "", "",""} ); 
 
        values_ref = s_arr; values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] {
            false, false, false ,false, false, false
        }); 

        trns_fnc = true; trns_back = false;
      
    }


    // Kill effect
    public void kill_am()
    {
        reset_smoothDmpfnc();  

        iterator_ = 3;
        List<float> v_flt; List<string> s_arr;
        
        float s_ = UnityEngine.Random.Range(1, 3);

        // +40% smoothTime !!
        smoothTime_prc = 30f;

        // +5 fov  !!
        new_fov = 80f;

        if(s_ == 1)
        {
            v_flt = new List<float>(new float[6] {-0.18f, -0.2f, -0.20f, -1.7f, 0f, 0f} ); 
            s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_rot_z_offst", "wallR_z_offst", "wallR_y_offst", "",""} ); 
        }else
        {
            v_flt = new List<float>(new float[6] {-0.18f, 0.2f, -0.20f, -1.7f, 0f, 0f} ); 
            
            s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_rot_z_offst", "wallR_z_offst", "wallR_y_offst", "",""} ); 
        }
 
        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] {
            false, false, false ,false, false, false
        }); 

        trns_fnc = true; trns_back = false;
      
    }



    // hang jumpOut
    public void hang_jumpOut()
    {

        reset_smoothDmpfnc();  

        iterator_ = 3;
        List<float> v_flt = new List<float>(new float[6] {-0.210f, -2.60f, 0.090f, 0.0f, 0.0f, 0.0f} ); 
        List<string> s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_y_offst", "wallR_rot_z_offst", "", "",""} ); 
 
         // +20% smoothTime !!
        smoothTime_prc = 20f;


        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false }); 

        trns_fnc = true; trns_back = false; 
    }


    // fallBox
    public void fall_Box()
    {

        reset_smoothDmpfnc();  

        // +50% smoothTime !!
        smoothTime_prc = 50f;

        iterator_ = 2;
        List<float> v_flt = new List<float>(new float[6] {-0.2f, -1.5f, 0f, 0.0f, 0.0f, 0.0f} ); 
        List<string> s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_y_offst", "", "", "",""} ); 
 
        values_ref = s_arr; values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false }); 

        trns_fnc = true; trns_back = false; 
    }

    // climb & ladder
    public void climbUp()
    {

        pos_dc["wallR_x_offst"] = 0.0f; pos_dc["wallR_y_offst"] = 0.0f; pos_dc["wallR_z_offst"] = 0.0f;
        rot_dc["wallR_rot_x_offst"] = 0.0f; rot_dc["wallR_rot_z_offst"] = 0.0f; rot_dc["wallR_rot_y_offst"] = 0.0f;
        reset_smoothDmpfnc();  
        
        // unique currentVelocity reset
        // currentVelocity = Vector3.zero;

        // +30% smoothTime !!
        smoothTime_prc = 30f;

        iterator_ = 2;
        List<float> v_flt = new List<float>(new float[6] {-0.1f, -0.5f, 0f, 0.0f, 0.0f, 0.0f} ); 
        List<string> s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_y_offst", "", "", "",""} ); 
 
        values_ref = s_arr; values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false }); 

        trns_fnc = true; trns_back = false; 
    }


    // side hang
    public void sideHg(bool leftSide)
    {

        pos_dc["wallR_x_offst"] = 0.0f; pos_dc["wallR_y_offst"] = 0.0f; pos_dc["wallR_z_offst"] = 0.0f;
        rot_dc["wallR_rot_x_offst"] = 0.0f; rot_dc["wallR_rot_z_offst"] = 0.0f; rot_dc["wallR_rot_y_offst"] = 0.0f;
        reset_smoothDmpfnc();  
        

        // +20% smoothTime !!
        smoothTime_prc = 30f;

        iterator_ = 6;
        List<float> v_flt = new List<float>(new float[6] {0.5f, leftSide ?  0.5f : -0.5f, 
        0.1f, leftSide ? -0.1f : 0.1f, leftSide ? -3f : 3f, 0.5f} ); 

        List<string> s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_rot_y_offst", 
        "wallR_rot_x_offst", "wallR_rot_z_offst", "wallR_x_offst","wallR_z_offst"} ); 
 
        values_ref = s_arr; values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false }); 

        trns_fnc = true; trns_back = false; 
    }


    // bump
    public void bump(float y_rotation)
    {
        reset_smoothDmpfnc();  

        // +25% 
        smoothTime_prc = 10f;


        // float x__ = -1 * (y_rotation / 35);
        iterator_ = 4;
        List<float> v_flt = new List<float>(new float[6] {0.130f, 0.7f,  0.120f, 0.65f, 
        0f, 0f } ); 
        List<string> s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_y_offst", "wallR_rot_z_offst", "wallR_z_offst", 
        "wallR_x_offst",  "wallR_rot_y_offst"} ); 
 
        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] {
            false, false, false ,false, false, false
        }); 

        trns_fnc = true; trns_back = false;
    }

    // under
    public void under()
    {
        reset_smoothDmpfnc(); 

        // +15% 
        smoothTime_prc = 15f;

        iterator_ = 6;
        List<float> v_flt = new List<float>(new float[6] {-0.70f, 0.6f,  0.120f, -0.2f, 0.30f, -0.10f} ); 
        List<string> s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_z_offst", "wallR_rot_z_offst", "wallR_rot_x_offst", "wallR_x_offst",  "wallR_rot_y_offst"} ); 
 
        values_ref = s_arr; values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false }); 

        trns_fnc = true; trns_back = false;
    }

    // taptap
    public void tapTapJmp(bool tap_tapExit){
        reset_smoothDmpfnc();  

        // +10% || +30%
        smoothTime_prc = tap_tapExit ? 10f : 30f;

        // +5 fov  !!
        new_fov = 80f;

        iterator_ = 4;
        List<float> v_flt = new List<float>(new float[6] {
            tap_tapExit ? -1.5f : -0.9f, tap_tapExit ? 0f : 0.4f, 
            tap_tapExit ? -0.13f : 0.100f, tap_tapExit ? -0.110f : 0.040f, 0f, 0f
        } ); 
        List<string> s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_z_offst", "wallR_rot_z_offst", "wallR_rot_x_offst", "",  ""} ); 
 
        values_ref = s_arr; values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false }); 

        trns_fnc = true; trns_back = false; 
    }


    // bareer
    public void bareerJump(bool first_jumps)
    {
        reset_smoothDmpfnc();  

        // -15% || +30%
        smoothTime_prc = first_jumps ? -15f : 30f;

        iterator_ = first_jumps ? 4 : 3;
        List<float> v_flt = new List<float>(new float[6] {
            first_jumps ? 0.90f : -1.30f, first_jumps ? 0.1f : -0.1f, 
            first_jumps ? 0.120f : -0.160f, first_jumps ? 0.35f : 0f, 0f, 0f
        } ); 
        List<string> s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_rot_z_offst", "wallR_rot_x_offst", first_jumps ? "wallR_z_offst" : "",  "", ""} ); 
 
        values_ref = s_arr; values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false }); 

        trns_fnc = true; trns_back = false;
    }


    private IEnumerator time_coroutine()
    {
        yield return new WaitForSeconds(0.2f);
        slow_time = true;
        yield return new WaitForSeconds(0.15f);
        slow_time = false;
    }

    // public recoil method
    public void shoot_recoil(bool r_side)
    {
        cam_recoil = r_side ? -0.008f : 0.008f;
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
