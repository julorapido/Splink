using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
//using System.Collections.Generic;

public class CameraMovement : MonoBehaviour
{
    [Header ("Auto-Aim Coefficient")]
    private const float divisor = 55f;

    [Header ("Camera Main Rotation Ratio")]
    private const float x_ratio = -0.086f;

    [Header ("Camera Dash/Movements settings")]
    private const float Camera_Movement_Ratio = 1.25f;

    [Header ("Player Animator")]
    public GameObject p_gm;
    private Animator p_anim;

    [Header ("Player Inspector Values")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody player_rb;
    [SerializeField] private Vector3 player_velocity;

    [Header ("Camera Offset Value")]
    public Vector3 offset;


    [Header ("GameOver")]
    private bool game_Over_ = false;
    private int cam_gameOver_mode = 0;
    [HideInInspector] public int set_cam_gameOver_mode
    {
        set { if(value.GetType() == typeof(int) ) cam_gameOver_mode = value;}
        get { return -1; }
    }
    [HideInInspector] public bool set_gameOver_cam
    {
        set { if(value.GetType() == typeof(bool) ) game_Over_ = value;}
        get { return false; }
    }
    private Vector3  death_CAM_pos, death_V_pos, saved_deathPosition = Vector3.zero;
    private string death_mode;

    [Header ("Immediate Camera Transitions Values")]
    [HideInInspector] public float supl_xRot = 0.0f;
    [HideInInspector] public float supl_yOff = 0.0f;



    [Header ("NotSmooth Values")]
    private int iterator_;
    private bool trns_back = false, trns_fnc = false;
    private float trns_vlue;
    private List<float> values_flt = new List<float>(new float[6] {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f});
    private List<float> mathRef_arr = new List<float>(new float[6] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f });
    private List<string> values_ref = new List<string>(new string[6]{ "wallR_x_offst", "wallR_y_offst", "wallR_z_offst", "wallR_rot_x_offst", "wallR_rot_y_offst", "wallR_rot_z_offst"});
    private List<bool?> trns_back_arr = new List<bool?>(new bool?[6] {false, false, false ,false, false, false });
    private List<string> nms_ = new List<string>(new string[3] {"wallR_x_offst", "wallR_y_offst", "wallR_z_offst"});
    // ROTATIONS REFS
    private float mathfRef_x = 0.0f, mathfRef_y = 0.0f, mathfRef_z = 0.0f;
    private float mathfRef0_x = 0.0f, mathfRef0_y = 0.0f, mathfRef0_z = 0.0f;
    // POSITIONS REFS
    private float mathfRef_pos_x = 0.0f, mathfRef_pos_y = 0.0f, mathfRef_pos_z = 0.0f;
    private float mathfRef0_pos_x = 0.0f, mathfRef0_pos_y = 0.0f, mathfRef0_pos_z = 0.0f;
    // Percentage %
    private float smoothTime_prc = 0f;
    private static IDictionary<string, float> rot_dc = new Dictionary<string, float>(){
        {"wallR_rot_x_offst", 0.0f  },
        {"wallR_rot_y_offst", 0.0f },  {"wallR_rot_z_offst",  0.0f }
    };
    private static IDictionary<string, float> pos_dc = new Dictionary<string, float>(){
        { "wallR_x_offst", 0.0f },
        { "wallR_y_offst", 0.0f },  {"wallR_z_offst", 0.0f}
    };



    [Header ("Smooth Values")]
    private float lerpTime = 0.5f;
    private float currentLerpTime = 0f;
    private bool use_specialSmooth = false;
    private Vector3 lerp_v_pos, lerp_v_rot;



    [Header ("Fov Floats")]
    private float start_fov;
    private float new_fov;
    
    [Header ("Offet Y Compensation")]
    private float saved_y_offset = 0f;

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
    private Vector3 currentVelocity_pos, currentVelocity_rot;

    [Header ("Player Actions Informations")]
    private float tyro_z = 0f;
    private bool tyro_on = false;
    private bool slideRail_on = false;
    private bool sliding_on = false;
    private bool hanging = false;

    [Header ("Grapple Point Position")]
    [HideInInspector] public Vector3 _grplPoint_;
    private bool end_trans_called = false;


    [Header ("Aimed Target")]
    private Transform aimed_target;
    private Vector3 aim_v;
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
    private float aim_x, aim_y = 0f;


    [Header ("Scripts")]
    private Camera c_;
    private PlayerMovement plyr_mv;

    [Header ("SlowTime")]
    private bool slow_time = false;

    [Header ("Momentm")]
    private float plyr_Momentum;

    [Header ("Recoil")]
    private float cam_recoil = 0f;

    [Header ("Ragdoll")]
    [SerializeField] private Transform player_rag;

    [Header ("IS BUILD")]
    [HideInInspector] public bool is_build = false;


    // [Header ("Camera Bob")]
    // [HideInInspector] public bool can_headBob = false;
    // private float[] x_y = new float[2]{ 0.05f, 0.10f };
    // private int bobEffect = 225;
    // private const double t_Pi = 6.28319;
    // private const double Pi = 3.14159;
    // private double bob_rad = 0;
    // private double pi_toRad { get {return 0; }
    //     set { if((value).GetType() == typeof(double))
    //         {coef = (( (Math.PI) / 180f) *(value)); }
    //     }
    // }
    // private double coef = 0;

    private void Awake()
    {
        plyr_mv = FindObjectOfType<PlayerMovement>();

        saved_y_offset = offset.y;
    }

    private void Start()
    {
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

        // field of view
        if(c_.fieldOfView != (new_fov + plyr_Momentum))
        {
            c_.fieldOfView = Mathf.Lerp(c_.fieldOfView, (new_fov + plyr_Momentum), 0.10f);
        }

        /*
        if(smthDmp_grpl)
            rot_dc["wallR_rot_x_offst"] = Mathf.SmoothDamp(rot_dc["wallR_rot_x_offst"], 0.12f, ref mathfRef_grpl, 0.295f);

        if(smthDmp_grpl_end)
            rot_dc["wallR_rot_x_offst"] = Mathf.SmoothDamp(rot_dc["wallR_rot_x_offst"], -0.325f, ref mathfRef_grpl, 0.650f);
        */


        // recoil
        if(cam_recoil > 0 && cam_recoil  < (0.03f) * Time.deltaTime)
            cam_recoil -= (0.00002f) * Time.deltaTime;

        else if ( (cam_recoil < 0 ) && (cam_recoil > (-1 *  (0.03f) * Time.deltaTime)) )
            cam_recoil -= (0.0002f) * Time.deltaTime;





        // aim effect
        Vector3 player_target_relativePos =  (
            aimed_target != null ? ((aimed_target.position - player.position)) : (Vector3.zero)
        );
        Quaternion aiming_rotation = (
            aimed_target != null ? (Quaternion.LookRotation(player_target_relativePos)) : (Quaternion.identity)
        );


        aim_v = (aimed_target != null) ? (Vector3.Lerp(
            aim_v,
            new Vector3(
                aiming_rotation.eulerAngles.y > 180 ?  (aiming_rotation.eulerAngles.y - 360f) : (aiming_rotation.eulerAngles.y),
                aiming_rotation.eulerAngles.x > 180f ?  (aiming_rotation.eulerAngles.x - 360f) : (aiming_rotation.eulerAngles.x),
                0f
            ) * (0.2f),
            4f * Time.deltaTime
        )) : (Vector3.zero);



        // lerp
        if(!use_specialSmooth)
        {
            if(
                lerp_v_pos != new Vector3(pos_dc["wallR_x_offst"], pos_dc["wallR_y_offst"], pos_dc["wallR_z_offst"]) &&
                lerp_v_rot != new Vector3(rot_dc["wallR_rot_x_offst"], rot_dc["wallR_rot_y_offst"], rot_dc["wallR_rot_z_offst"])
            )
                currentLerpTime += 0.02f * (Time.deltaTime);

            if (currentLerpTime > lerpTime)
                currentLerpTime = lerpTime;

            //lerp!
            float perc = currentLerpTime / lerpTime;

            lerp_v_pos = Vector3.Lerp(
                lerp_v_pos,
                new Vector3(pos_dc["wallR_x_offst"], pos_dc["wallR_y_offst"], pos_dc["wallR_z_offst"]),
            perc);

            lerp_v_rot = Vector3.Lerp(
                lerp_v_rot,
                new Vector3(rot_dc["wallR_rot_x_offst"], rot_dc["wallR_rot_y_offst"], rot_dc["wallR_rot_z_offst"]),
            perc);
        }

        
        
        // x_offst Assignation
        // x_offst = (player_rb.rotation.eulerAngles.y > 298.0f ?
        //         -1 *  (60 - (player_rb.rotation.eulerAngles.y - 300.0f))
        //     :  player_rb.rotation.eulerAngles.y
        // );

        /*
        x_offst = (player.rotation.eulerAngles.y > 298.0f ?
                -1 *  (60 - (player.rotation.eulerAngles.y - 300.0f))
            :  player.rotation.eulerAngles.y
        );
        */
        /*

        // Lerp [Position] and [Rotation]
        if( ((x_ratio * x_offst) != lst_offst_x)
            || (x_ratio * x_offst == 0))
        {
            desired_  = (player.position + offset);
           
            desired_.x = desired_.x +  (
                (x_ratio * ( (tyro_on) ?
                    (x_offst * 2)
                        :
                    (x_offst / 2.6f)
                ))
            );
            
            desired_.z = desired_.z +  (Math.Abs(x_offst)) / 80f;

            lst_offst_x = ((x_ratio * x_offst));
        }

        */

    }




    private void FixedUpdate()
    {
        // values attribution
        player_velocity = player_rb.velocity;
        plyr_Momentum = plyr_mv.get_Momentum;
        tyro_on = plyr_mv.plyr_tyro;
        sliding_on =  plyr_mv.plyr_sliding;



        if(tyro_on != plyr_mv.plyr_tyro)
        {
            int zz = UnityEngine.Random.Range(1, 3);
            if(zz == 1) 
                tyro_z = 0.1f;
            else 
                tyro_z = -0.1f;
        }



        // Smooth damp function
        if(trns_fnc)
        {
            int it_ = 0;
            for (int i = 0; i < values_flt.Count; i++)
            {
                if( values_flt[i] == 0.0f) break;

                bool s_ =  nms_.Contains(values_ref[i]);

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
                            rot_dc[values_ref[i]] = Mathf.SmoothDamp( rot_dc[values_ref[i]], values_flt[i] /* * (Camera_Movement_Ratio) */, 
                                        ref srched_ref, 0.060f * (1.0f + (smoothTime_prc/100) ) );

                            if( Math.Abs(rot_dc[values_ref[i]]) >= Math.Abs(values_flt[i]) - 0.0045f)
                            {
                                trns_back_arr[i] = true;
                            }
                        }
                        // trans => to 0f
                        else if (trns_back_arr[i] == true)
                        {
                            rot_dc[values_ref[i]] = Mathf.SmoothDamp(rot_dc[values_ref[i]], 0.00f, ref srched_ref, 0.055f * (1.0f + (smoothTime_prc/100) ) );
                            if(Math.Abs(rot_dc[values_ref[i]]) < 0.0040f )
                            {
                                it_++;
                                trns_back_arr[i] = null;
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
                            pos_dc[values_ref[i]] = Mathf.SmoothDamp( pos_dc[values_ref[i]], values_flt[i] /* * (Camera_Movement_Ratio) */, 
                                        ref srched_ref, 0.060f * (0.8f + (smoothTime_prc/100) ) );
                            if( Math.Abs(pos_dc[values_ref[i]]) >= Math.Abs(values_flt[i]) - 0.003f )
                            {
                                trns_back_arr[i] = true;
                            }
                        }
                        // trans => to 0f
                        else if (trns_back_arr[i] == true)
                        {
                            pos_dc[values_ref[i]] = Mathf.SmoothDamp(pos_dc[values_ref[i]], 0.00f, ref srched_ref, 0.050f * (0.8f + (smoothTime_prc/100) ) );
                            if(Math.Abs(pos_dc[values_ref[i]]) < 0.0070f )
                            {
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



        /*
        // Lerp Position
        if( ((x_ratio * x_offst) != lst_offst_x)
            || (x_ratio * x_offst == 0))
        {
            desired_  = (player.position + offset);
           
            desired_.x = desired_.x +  (
                (x_ratio * ( (tyro_on) ?
                    (x_offst * 2)
                        :
                    (x_offst / 2.6f)
                ))
            );
            
            desired_.z = desired_.z +  (Math.Abs(x_offst)) / 80f;

            lst_offst_x = ((x_ratio * x_offst));
        }

        */

        if(hanging)
            desired_ = (player.position + offset);


        // time
        if(slow_time)
        {
            Time.timeScale = 0.85f;
        }
        else
        { 
            Time.timeScale = 1f; 
        }

    }


    // cam smoothdamps & lerps
    private void LateUpdate()
    {
        
        if (!game_Over_)
        {
            // float xy_ratio = 0.0590f;

            bool aim_off =  (aimed_target == null || tyro_on ) ? true : false;

            // --- [ROTATION] ---
            // Smooth Damp (rotation)
            // Dampen towards target rotation
            x_offst = (player.rotation.eulerAngles.y > 298.0f ?
                    -1 *  (60 - (player.rotation.eulerAngles.y - 300.0f))
                :  player.rotation.eulerAngles.y
            );
        
            Quaternion desired_rt  = new Quaternion(
                xRot + supl_xRot
                + (!use_specialSmooth ? lerp_v_rot.x : rot_dc["wallR_rot_x_offst"])
                + (aim_v.y * 0.002f)
                ,
                (!use_specialSmooth ? lerp_v_rot.y : rot_dc["wallR_rot_y_offst"])
                +  (hanging ? 0f : (x_offst / (sliding_on ? 120f : 160.0f) ))
                 + (aim_v.x * 0.002f)
                ,
                (!use_specialSmooth ? lerp_v_rot.z : rot_dc["wallR_rot_z_offst"])
                + (hanging ? 0f : (-1 * (x_offst / 360.0f)))
                + (tyro_on ? (tyro_z) : 0f)
                ,
                1f
            );

            
            transform.localRotation = Quaternion.Slerp(
                gameObject.transform.rotation, desired_rt,
                (!(is_build)) ? 
                    ((tyro_on) ? 0.020f : 0.08f) // PC = a constant (0.08f)
                    :
                    (20f * Time.deltaTime )  // Mobile = fixed deltaTime            
            );
            // --------------------------------------------------------------------



            // --- [POSITION] ---
            // Smooth Damp (position)
            // Dampen towards offset position
            desired_  = (player.position + offset); // <----------------------
            desired_.x = desired_.x +  (// <----------------------
                (x_ratio * ( (tyro_on) ?// <----------------------
                    (x_offst * 2)// <----------------------
                        :// <----------------------
                    (x_offst / 2.6f)// <----------------------
                ))// <----------------------
            );// <----------------------
            desired_.z = desired_.z +  (Math.Abs(x_offst)) / 75f;// <----------------------

            if( !(is_build) ) // PC = SmoothDamp
            {
                Vector3 smoothFollow = Vector3.SmoothDamp(
                    transform.position,
                    (
                        desired_ +
                            (tyro_on ? new Vector3(0f, 0.5f, 0.5f) : new Vector3(0f,0f,0f))
                        +
                        (!use_specialSmooth ?
                            (lerp_v_pos) : (new Vector3(pos_dc["wallR_x_offst"], pos_dc["wallR_y_offst"] + supl_yOff, pos_dc["wallR_z_offst"]))
                        )
                        + new Vector3(0f, ( (player_velocity.y < -4) ? ((player_velocity.y * Time.fixedDeltaTime) * 1.6f) : 0f), 0f) // y neg velocity compensation
                        // + new Vector3(0, 0, (player_rb.velocity.z > 8 ) ? (player_rb.velocity.z - 8) * 0.03f : 0f ) // z speed compensation
                        + (aim_v * 0.005f) // aiming vector
                    ),
                    ref currentVelocity_pos,
                    (
                        (tyro_on) ? 0.080f : 0.04f
                    )
                );
                transform.position = smoothFollow;
            }
            else // MOBILE = Lerp
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    (
                        desired_ +
                            (tyro_on ? new Vector3(0f, 0.5f, 0.5f) : new Vector3(0f,0f,0f))
                        +
                        (!use_specialSmooth ?
                            (lerp_v_pos) : (new Vector3(pos_dc["wallR_x_offst"], pos_dc["wallR_y_offst"] + supl_yOff, pos_dc["wallR_z_offst"]))
                        )
                        + new Vector3(0f, ( (player_velocity.y < -4) ? ((player_velocity.y * Time.fixedDeltaTime) * 1.6f) : 0f), 0f) // y neg velocity compensation
                    ),
                    (
                        40f * Time.deltaTime 
                    )
                );
            }
            // --------------------------------------------------------------------


            if(tyro_on)
                transform.LookAt( player.position + new Vector3(0, 1.25f, 0) );
        }
        else
        {
            // ---------------------
            //   GameOver Behavior
            // ---------------------
            // 1 -> [MoveUp Y-AXIS] && [LookAt Player]
            if(cam_gameOver_mode == 1)
            {
                transform.LookAt(player_rag.position);
                death_V_pos = player_rag.position;

           
                transform.position = transform.position + new Vector3(0f, 0.003f, -0.004f);
            }
            // 2 -> [Return_DeathPos] && [LooktAt Player-Death]
            else if(cam_gameOver_mode == 2)
            {
                transform.LookAt(death_V_pos);
                transform.position = death_CAM_pos;
                // if(transform.position.y < 4f)
                // {
                //     transform.position += new Vector3(0f, 0.0175f, 0.001f);
                // }
            }
        }

    }





    // -----------------------------------------------
    //           RESET LERPS() AND SMOOTHDAMPS()
    // -----------------------------------------------
    private void reset_smoothDmpfnc()
    {
        new_fov = start_fov;
        smoothTime_prc = 0.0f;
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

        currentLerpTime = 0f;
        lerp_v_rot = lerp_v_pos = Vector3.zero;
        use_specialSmooth = false;
    }
    private void reset_notSmooth()
    {
        currentLerpTime = 0f;
        lerp_v_rot = lerp_v_pos = Vector3.zero;
        use_specialSmooth = false;

        supl_xRot = 0.0f;
        new_fov = start_fov;
        pos_dc["wallR_x_offst"] = 0.0f; pos_dc["wallR_y_offst"] = 0.0f; pos_dc["wallR_z_offst"] = 0.0f;
        rot_dc["wallR_rot_x_offst"] = 0.0f; rot_dc["wallR_rot_z_offst"] = 0.0f; rot_dc["wallR_rot_y_offst"] = 0.0f;
    }



    // -----------------------------------------------
    //           RESET LERPS() AND SMOOTHDAMPS()
    // -----------------------------------------------
    private void apply_movementRatio(bool is_for_smoothDamps)
    {
        // return; 
        if(is_for_smoothDamps)
        {
            List<float> temp_flt = values_flt;
            temp_flt.Select(
                    (el, index) => new {index, el = el * (Camera_Movement_Ratio)}
                ).ToList();
            // values_flt = temp_flt;
            for(int i = 0; i < 6; i++)
            {
                values_flt[i] *= (Camera_Movement_Ratio);
                //Debug.Log("AFTER = " + values_flt[i]);
            }
        }else
        {
            foreach(string key in pos_dc.Keys.ToList())
            {
                pos_dc[key] = pos_dc[key] * (Camera_Movement_Ratio);
            }
            foreach(string key in rot_dc.Keys.ToList())
            {
                rot_dc[key] = rot_dc[key] * (Camera_Movement_Ratio);
            }
            // foreach(KeyValuePair<string, float> entry in pos_dc)
            // {
            //     // do something with entry.Value or entry.Key
            //     entry.Value =  entry.Value * (Camera_Movement_Ratio);
            // }
            // foreach(KeyValuePair<string, float> entry in rot_dc)
            // {
            //     // do something with entry.Value or entry.Key
            //     entry.Value = ( entry.Value * Camera_Movement_Ratio);
            // }
        }
    }


    // [Wall run]
    public void wal_rn_offset(bool is_ext, Transform gm_, float y_bonuss = 0.0f)
    {
        if(is_ext)
        {
            reset_notSmooth();

        }else
        {
            // // //
            reset_smoothDmpfnc();
            // // //

            float sns =  Math.Abs(player.position.x) - Math.Abs(gm_.position.x);

            new_fov = 87f;
            pos_dc["wallR_y_offst"] = -1.3f;

            // CLOSE UP Z OFFSET
            pos_dc["wallR_z_offst"] = 0.75f;

            // ROTATE TOP
            rot_dc["wallR_rot_x_offst"] = -0.110f;

            rot_dc["wallR_rot_z_offst"] = sns > 0 ? 0.17f : -0.17f;

            if(sns > 0 )
            {
                // right wall hit
                // pos_dc["wallR_x_offst"] = -1.5f + (y_bonuss / 22);
                // rot_dc["wallR_rot_y_offst"] = 0.2f + (-1 * (y_bonuss / 92));
                pos_dc["wallR_x_offst"] = -1.1f;
                rot_dc["wallR_rot_y_offst"] = 0.085f;
            }else
            {
                // left wall hit
                // pos_dc["wallR_x_offst"] = 1.2f + ( -1 * (y_bonuss / 22) );
                // rot_dc["wallR_rot_y_offst"] = -0.15f + 1 * (y_bonuss / 92);
                pos_dc["wallR_x_offst"] = 1.1f;
                rot_dc["wallR_rot_y_offst"] = -0.085f;
            }
        
            apply_movementRatio(false);
        }
    }

    // [Wall Slide]
    public void wal_slide_offset(bool is_ext, Transform gm_)
    {
        if(is_ext)
        {
            reset_notSmooth();
        }else
        {
            // reset_smoothDmpfnc();

            float sns =  Math.Abs(player.position.x) - Math.Abs(gm_.position.x);

            new_fov = 92f;
            pos_dc["wallR_y_offst"] = -1.2f;

            // CLOSE UP Z OFFSET
            pos_dc["wallR_z_offst"] = 0.75f;

            // ROTATE TOP
            rot_dc["wallR_rot_x_offst"] = -0.015f;

            rot_dc["wallR_rot_z_offst"] = sns > 0 ? 0.35f : -0.35f;

            if(sns > 0 )
            {
                // right wall hit
                pos_dc["wallR_x_offst"] = -1f;
                rot_dc["wallR_rot_y_offst"] = 0.10f;
            }else
            {
                // left wall hit

                pos_dc["wallR_x_offst"] = 1f;
                rot_dc["wallR_rot_y_offst"] = -0.10f;
            }
            
            apply_movementRatio(false);
        }
    }

    // [Sidehang]
    public void side_hang(bool is_ext, bool l_side = false)
    {
        if(is_ext)
        {
            reset_notSmooth();
        }else
        {
            // // //
            reset_smoothDmpfnc();
            // // //
            apply_movementRatio(false);
  
            //FovTrans(start_fov, 0.5f);
            new_fov = 88f;
    
             // CLOSE UP Z OFFSET
            pos_dc["wallR_x_offst"] = l_side ? 1f : -1f;
            pos_dc["wallR_y_offst"] = 0.4f;

            // SMOOTH DAMP FOR X ROTATION
            rot_dc["wallR_rot_x_offst"] = 0.19f;
            rot_dc["wallR_rot_y_offst"] = l_side ? -0.115f : 0.115f;
            rot_dc["wallR_rot_z_offst"] =  l_side ? -0.10f : 0.10f;

            apply_movementRatio(false);
        }
    }

    // Hang
    // public void hang(bool is_ext)
    // {
    //     if(is_ext)
    //     {
    //         reset_notSmooth();
    //     }else
    //     {
    //         // // //
    //         reset_smoothDmpfnc();
    //         // // //
            
    //         hanging = true;
    //         new_fov = 90f;
    
    //          // CLOSE UP Z OFFSET
    //         pos_dc["wallR_z_offst"] = -1f;
    //         pos_dc["wallR_y_offst"] = 0.5f;

    //         rot_dc["wallR_rot_x_offst"] = 0.15f;
    //     }
    // }



    // [Sliding]
    public void sld_offset(bool is_ext)
    {
        if(is_ext)
        {
              reset_notSmooth();
        }else
        {
            reset_smoothDmpfnc();

            new_fov = 95f;

            float s_ = UnityEngine.Random.Range(1, 3);

            // CLOSE UP Z OFFSET
            pos_dc["wallR_z_offst"] = 1f;
            pos_dc["wallR_y_offst"] = -1.2f;

            // SMOOTH DAMP FOR X ROTATION
            rot_dc["wallR_rot_x_offst"] = -0.06f;
            rot_dc["wallR_rot_z_offst"] =  s_ == 1 ? -0.10f : 0.10f;

            apply_movementRatio(false);
        }
    }


    // [Rail Slide]
    public void railSlide_offset(bool is_ext, bool side_)
    {
        if(is_ext)
        {
            reset_notSmooth();
        }
        else
        {
            reset_smoothDmpfnc();


            new_fov = 90f;
            // SPACE + OFFSET
            pos_dc["wallR_x_offst"] = !side_ ? -0.35f : 0.35f;
            pos_dc["wallR_y_offst"] = -0.5f;
            pos_dc["wallR_z_offst"] = 0.7f;

            rot_dc["wallR_rot_z_offst"] = !side_ ? 0.150f : -0.150f;
            rot_dc["wallR_rot_x_offst"] = 0.075f;

            apply_movementRatio(false);
        }
    }

    // [Ladder]
    public void ladderClimb_offst(bool is_ext)
    {
        if(is_ext)
        {
            reset_notSmooth();
        }else
        {
            reset_smoothDmpfnc();

            new_fov = 86f;
            // SPACE Z+
            pos_dc["wallR_z_offst"] = 0.1f;

            pos_dc["wallR_y_offst"] = 1.5f;

            // SMOOTH DAMP FOR X ROTATION
            rot_dc["wallR_rot_x_offst"] = 0.150f;

            apply_movementRatio(false);
        }
    }



    // [Obstcl Jump]
    // public void obs_offset()
    // {
    //     // SMOOTH DAMP FOR X ROTATION
    //     pos_dc["wallR_y_offst"] = 0.35f;
    //     rot_dc["wallR_rot_x_offst"] = 0.14f;
    //     reset_smoothDmpfnc();
    //     Invoke("obst_rst", 0.75f);
    // }
    // private void obst_rst()
    // {
    //     supl_xRot = 0.0f;
    //     pos_dc["wallR_y_offst"] = 0.0f;rot_dc["wallR_rot_x_offst"] = 0.0f;
    // }


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
            reset_notSmooth();
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
            apply_movementRatio(false);
        }
    }








    // Jump & DoubleJump
    public void jmp(bool is_dblJmp)
    {
        reset_smoothDmpfnc();

        // +10% smoothTime !!
        smoothTime_prc = 10f;

        iterator_ = 3;
        List<float> v_flt = new List<float>(new float[6] {
            is_dblJmp ? -0.26f : -0.22f, is_dblJmp ? -2.2f : -1.8f, is_dblJmp ?  -0.155f : 0.155f,
            0.0f, 0.0f, 0.0f
        } );
        List<string> s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_y_offst", "wallR_rot_z_offst", "", "",""} );

        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] {
            false, false, false ,false, false, false
        });

        apply_movementRatio(true);

        trns_fnc = use_specialSmooth = true;
        trns_back = false;


        StartCoroutine(time_coroutine());
    }



    // Special Jump [LAUNCHER, TAP TAP]
    public void special_jmp()
    {
        reset_smoothDmpfnc();

        // +30% smoothTime !!
        smoothTime_prc = 15f;

        iterator_ = 4;
        List<float> v_flt = new List<float>(new float[6] {0.120f, 0.85f, 0.040f, 
            0.6f, 0.0f, 0.0f} );
        List<string> s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_y_offst", "wallR_rot_z_offst", 
            "wallR_z_offst", "",""} );

        values_ref = s_arr; values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] {
            false, false, false ,false, false, false
        });

        apply_movementRatio(true);
        trns_fnc = use_specialSmooth = true; trns_back = false;

    }


    // Kill
    public void kill_am()
    {
        reset_smoothDmpfnc();

        iterator_ = 3;
        List<float> v_flt; List<string> s_arr;

        smoothTime_prc = 25f;

        // +5 fov  !!
        new_fov = 80f;

        v_flt = new List<float>(new float[6] {-0.15f, -0.12f, -1.3f, 0f, 0f, 0f} );
        s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_rot_z_offst", "wallR_y_offst", "", "",""} );

        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] {
            false, false, false ,false, false, false
        });

        trns_fnc = use_specialSmooth = true; trns_back = false;

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

        trns_fnc = use_specialSmooth = true; trns_back = false;
    }

    // climb & ladder
    public void climbUp()
    {

        pos_dc["wallR_x_offst"] = 0.0f; pos_dc["wallR_y_offst"] = 0.0f; pos_dc["wallR_z_offst"] = 0.0f;
        rot_dc["wallR_rot_x_offst"] = 0.0f; rot_dc["wallR_rot_z_offst"] = 0.0f; rot_dc["wallR_rot_y_offst"] = 0.0f;
        reset_smoothDmpfnc();

        // unique currentVelocity_pos reset
        // currentVelocity_pos = Vector3.zero;

        // +30% smoothTime !!
        smoothTime_prc = 30f;

        iterator_ = 2;
        List<float> v_flt = new List<float>(new float[6] {-0.1f, -0.5f, 0f, 0.0f, 0.0f, 0.0f} );
        List<string> s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_y_offst", "", "", "",""} );

        values_ref = s_arr; values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false });

        trns_fnc = use_specialSmooth = true; trns_back = false;
    }




    // bump
    public void bump(float y_rotation)
    {
        reset_smoothDmpfnc();

        // +25%
        smoothTime_prc = 14f;


        // float x__ = -1 * (y_rotation / 35);
        iterator_ = 3;
        List<float> v_flt = new List<float>(new float[6] {
            0.130f, 1f,  0.120f, 0f, 0f, 0f
        } );
        List<string> s_arr = new List<string>(new string[6] {
            "wallR_rot_x_offst", "wallR_y_offst", "wallR_rot_z_offst", "", "",  ""
        } );

        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] {
            false, false, false ,false, false, false
        });

        trns_fnc = use_specialSmooth = true; trns_back = false;
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

        trns_fnc = use_specialSmooth = true; trns_back = false;
    }
 
    // taptap
    public void tapTapJmp(bool tap_tapExit){
        reset_smoothDmpfnc();

        // if(tap_tapExit)
        //     return;
        // +10% || +30%
        smoothTime_prc = tap_tapExit ? 22f : 25f;

        // +5 fov  !!
        new_fov = 80f;

        iterator_ = 4;
        List<float> v_flt = new List<float>(new float[6] {
            tap_tapExit ? -0.65f : -0.2f, tap_tapExit ? 0.42f : 0.55f,
            tap_tapExit ? 0.13f : -0.050f, tap_tapExit ? -0.001f : 0.09f, 0f, 0f
        } );
        List<string> s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_z_offst", 
            "wallR_rot_z_offst", "wallR_rot_x_offst", "",  ""} );

        values_ref = s_arr; values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false });

        apply_movementRatio(true);

        trns_fnc = use_specialSmooth = true; trns_back = false;
    }


    // ground hit
    public void ground_roll()
    {
        reset_smoothDmpfnc();

        // +10%
        smoothTime_prc = 10f;

        iterator_ = 4;
        List<float> v_flt = new List<float>(new float[6] {0.19f, 0.45f,  -0.125f, 0.5f, 0f, 0f } );
        List<string> s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_y_offst", "wallR_rot_z_offst", "wallR_z_offst", "",  ""} );

        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] {false, false, false ,false, false, false});

        apply_movementRatio(true);

        trns_fnc = use_specialSmooth = true; trns_back = false;
    }



    // leave ground
    public void leave_ground(float y_rot)
    {
        reset_smoothDmpfnc();

        // +17%
        smoothTime_prc = 15f;

        iterator_ = 3;
        List<float> v_flt = new List<float>(new float[6] {-1.15f, y_rot > 0 ? -0.15f : 0.15f, -0.085f,
        0f, 0f, 0f } );
        List<string> s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_rot_z_offst", "wallR_rot_x_offst",
        "", "",  ""} );

        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false});

        apply_movementRatio(true);
 
        trns_fnc = use_specialSmooth = true; trns_back = false;
    }

    // JumpOut [Rail && Slide]
    public void jumpOut(float y_rotation)
    {
        reset_smoothDmpfnc();

        // +12%
        smoothTime_prc = 13f;

        iterator_ = 4;
        List<float> v_flt = new List<float>(new float[6] {-0.30f, y_rotation < 0 ? 0.2f : -0.2f, 0.1f, 
        0.5f, 0f, 0f } );
        List<string> s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_rot_z_offst", "wallR_rot_x_offst",
        "wallR_z_offst", "",  ""} );

        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false});

        apply_movementRatio(true);

        trns_fnc = use_specialSmooth = true; trns_back = false;
    }
    
    // fallbox
    public void fall_box(bool y)
    {
        reset_smoothDmpfnc();

  
        // +12%
        smoothTime_prc = 15f;

        // +5 fov  !!
        new_fov = 90f;

        iterator_ = 4;
        List<float> v_flt = new List<float>(new float[6] {-1.2f, -0.11f,
        0.75f, y ? -0.15f : 0.15f, 0f, 0f } );

        List<string> s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_rot_x_offst",
        "wallR_z_offst", "wallR_rot_z_offst", "",  ""} );

        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false});
        trns_fnc = use_specialSmooth = true; trns_back = false;
        
    }

    // dash_down
    public void dash_down()
    {
        reset_smoothDmpfnc();

        
        // +12%
        smoothTime_prc = 15f;

        // +7 fov  !!
        new_fov = 85f;

        iterator_ = 4;
        List<float> v_flt = new List<float>(new float[6] {0.14f, 0.070f,
        0.30f, -0.70f, 0f, 0f} );

        List<string> s_arr = new List<string>(new string[6] {"wallR_rot_z_offst", "wallR_rot_x_offst",
        "wallR_z_offst", "wallR_y_offst", "",  ""} );
    
        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false});

        apply_movementRatio(true);

        trns_fnc = use_specialSmooth = true; trns_back = false;
    }


    // bareer_
    public void bareer_jmp(bool horizontal, bool lft_rgt = false, bool lat = false)
    {
        reset_smoothDmpfnc();

        // +10 fov  !!
        new_fov = 82f;
        List<float> v_flt;
        List<string> s_arr;

        if(!horizontal)
        {
            smoothTime_prc = 10f;
            iterator_ = 3;
            s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_rot_z_offst", "wallR_rot_x_offst", "", "",  ""} );
            if(lat)
                v_flt = new List<float>(new float[6] {-0.35f, !lft_rgt ? 0.2f : -0.2f, 0.07f, 0f, 0f, 0f} );
            else
                v_flt = new List<float>(new float[6] {-2.3f, 0.1f, -0.2f, 0f, 0f, 0f} );
        }else
        {
            smoothTime_prc = 0f;
            iterator_ = 4;
            v_flt = new List<float>(new float[6] {-2.7f, -0.2f, 
                lft_rgt ? (0.05f) : (-0.05f), lft_rgt ? (-0.25f) : (0.25f), 0f, 0f} );
            s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_rot_x_offst", 
                    "wallR_rot_y_offst", "wallR_rot_z_offst", "",  ""} );
        }
 
   
        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false});

        apply_movementRatio(true);

        trns_fnc = use_specialSmooth = true; trns_back = false;
    }


    // hang
    public void hang(bool is_jumpOut)
    {
        if(!is_jumpOut)
            hanging = true;
        reset_smoothDmpfnc();

        // +20%
        smoothTime_prc = is_jumpOut ? 17f : 32f;

        // +10 fov  !!
        new_fov = 90f;

        iterator_ = 2; 
        List<float> v_flt  = new List<float>(new float[6] {-1.6f, -0.25f, 0f,
             0f, 0f, 0f} );
        List<string> s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_rot_x_offst", "",
        "", "",  ""} );

        if(is_jumpOut)
        {
            hanging = false;
            iterator_ = 4; 
            v_flt  = new List<float>(new float[6] {-2f, -0.2f, 0.15f, 1f, 0f, 0f} );
            s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_rot_x_offst", "wallR_rot_z_offst", "wallR_z_offst", "",  ""} );
        }

        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false});
        trns_fnc = use_specialSmooth = true; trns_back = false;
     
    }


    // wall_out
    public void wall_outttt(bool sns)
    {
        reset_smoothDmpfnc();

        
        // -5%
        smoothTime_prc = -5f;

        // +7 fov  !!
        new_fov = 85f;

        iterator_ = 2; 
        List<float> v_flt = new List<float>(new float[6] {sns ? 0.26f : -0.26f, -0.4f,
        0f, 0f, 0f, 0f} );

        List<string> s_arr = new List<string>(new string[6] {"wallR_rot_z_offst", "wallR_y_offst", 
        "", "", "",  ""} );

        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false});


        apply_movementRatio(true);

        trns_fnc = use_specialSmooth = true; trns_back = false;
    }


    // hang
    public void obstacle_jump(int jump_mode, bool q = false)
    {
        reset_smoothDmpfnc();

        // +20%
        smoothTime_prc = 15f;

        // +10 fov  !!
        new_fov = 80f;

        List<float> v_flt = null;
        List<string> s_arr = null;

        if(jump_mode == 1)
        {
            iterator_ = 3;
            v_flt = new List<float>(new float[6] {-1.7f, -0.1f, 
                0.35f, 0f, 0f, 0f} );
            s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_rot_x_offst", 
                "wallR_rot_z_offst", "", "",  ""} );
        }
        if(jump_mode == 2)
        {
            iterator_ = 4;
            v_flt = new List<float>(new float[6] {-1.3f, 0.1f, 
                ((q) ? 0.25f : -0.25f), 0.6f, 0f, 0f} );
            s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_rot_x_offst", 
            "wallR_rot_z_offst", "wallR_z_offst", "",  ""} );
        }
        if(jump_mode == 3)
        {
            iterator_ = 4;
            v_flt = new List<float>(new float[6] {-0.4f, 0.1f, 
                (0.15f), 1f, 0f, 0f} );
            s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_rot_x_offst", 
            "wallR_rot_z_offst", "wallR_z_offst", "",  ""} );  
        }
        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false});
        trns_fnc = use_specialSmooth = true; trns_back = false;
     
    }


    // start_jump
    public void start_jump(int m_)
    {
        reset_smoothDmpfnc();

        
        // +25%
        smoothTime_prc = m_ == 1 ? 35f : 20f;

        // +12 fov  !!
        new_fov = 90f;

        iterator_ = 4; 

        List<float> v_flt;
        
        if(m_ == 1)
            v_flt = new List<float>(new float[6] {0.5f, 0.07f, 0.090f, 
                0f, 0f, 0f} );
        else
            v_flt = new List<float>(new float[6] {-0.75f, -0.07f, -0.12f,  
                0.70f, 0f, 0f} );

        List<string> s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_rot_z_offst", "wallR_rot_x_offst",
        "wallR_z_offst", "",  ""} );

        values_ref = s_arr;
        values_flt = v_flt;
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false});
        trns_fnc = use_specialSmooth = true; trns_back = false;
     
    }




    // -------------------------------
    //       CAM DEATH_ANIMATION
    // -------------------------------
    public void cam_death(string death_mode)
    {
        // register death position
        saved_deathPosition = player.position;


        reset_smoothDmpfnc();

         // +15%
        smoothTime_prc = 14f;

        new_fov = 85f;
        
        switch(death_mode)
        {
            case "front":
                iterator_ = 4; 
                List<float> v_flt = new List<float>(new float[6] {1.2f, 0.09f, 0.28f,
                0.7f, 0f, 0f} );

                List<string> s_arr = new List<string>(new string[6] {"wallR_y_offst", "wallR_rot_z_offst", "wallR_rot_x_offst",
                "wallR_z_offst", "",  ""} );

                values_ref = s_arr;
                values_flt = v_flt;
                break;
            case "damage":
                break;
            case "void":
                break;
            case "sideVoid":
                break;

  
        }
   
        trns_back_arr = new List<bool?>(new bool?[6] { false, false, false ,false, false, false});
        trns_fnc = use_specialSmooth = true; trns_back = false;
    }
    // ----------------------
    //     FINAL GAMEOVER
    // ----------------------
    public void cam_gameOver(string d_mode)
    {
        death_mode = d_mode;


        Vector3 pos = transform.position + new Vector3(0f, 6f - transform.position.y , 0f);

        // Lean cam position
        LeanTween.value(gameObject, transform.position, pos , 2f)
            .setEaseOutCubic()
            .setOnUpdate( (Vector3 value) => {
                death_CAM_pos = value; 
            });

        // Lean look position
        LeanTween.value(gameObject, death_V_pos, 
            new Vector3(death_V_pos.x, -5f, death_V_pos.z + 13f) , 2.3f)
            .setEaseOutCubic()
            .setOnUpdate( (Vector3 value) => {
                death_V_pos = value; 
            });
    }



    // jump => slow time
    private IEnumerator time_coroutine()
    {
        yield return new WaitForSeconds(0.2f);
        slow_time = true;
        yield return new WaitForSeconds(0.25f);
        slow_time = false;
    }



    // public recoil method
    public void shoot_recoil(bool r_side)
    {
        cam_recoil = r_side ? -0.008f : 0.008f;
    }
}
    