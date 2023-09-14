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
    [HideInInspector] public float supl_xRot = 0.0f;
    [HideInInspector] public float supl_yOff = 0.0f;

    private float mathfRef_grpl = 0.0f;

    private string dy_cm_inf = "";

    [Header ("Camera WallRun Values")]  

    //  public float wallR_rot_x_offst = 0.0f;
    // private float wallR_rot_y_offst = 0.0f;
    // private float wallR_rot_z_offst = 0.0f;

    [SerializeField]
    private static IDictionary<string, float> rot_dc = new Dictionary<string, float>(){
        {"wallR_rot_x_offst", 0.0f  },
        {"wallR_rot_y_offst", 0.0f },  {"wallR_rot_z_offst",  0.0f }
    };
    private static IDictionary<string, float> pos_dc = new Dictionary<string, float>(){
        { "wallR_x_offst", 0.0f },
        { "wallR_y_offst", 0.0f },  {"wallR_z_offst", 0.0f}
    };

    //[Header ("Camera WallRun Getter/Setters")]
    // private float x_off {
    //     get{ return wallR_x_offst; }  set{ wallR_x_offst = value; }
    // }
    // private float y_off {
    //     get{ return wallR_y_offst; }  set{ wallR_y_offst = value; }
    // }
    // private float z_off {  get{ return wallR_z_offst; }  set{ wallR_z_offst = value; } }


    [Header ("SmoothDamp Functions")]
    public bool trns_fnc = false;
    private bool trns_back = false;

    private float trns_vlue;
    private List<float> values_flt = new List<float>(new float[6] {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f});
    private List<string> values_ref = new List<string>(new string[6] {
      "wallR_x_offst", "wallR_y_offst", "wallR_z_offst", "wallR_rot_x_offst", "wallR_rot_y_offst", "wallR_rot_z_offst"
    });    
    private List<bool?> trns_back_arr = new List<bool?>(new bool?[6] {
        false, false, false ,false, false, false
    });    
    private List<float> mathRef_arr = new List<float>(new float[6] {
        0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f
    }); 
    private float mathfRef = 0.0f;
    private float mathfRef0_ = 0.0f;

    public float mathfRef_pos = 0.0f;
    public float mathfRef0_pos = 0.0f;

    [Header ("Fov Floats")]
    private float start_fov;
    private float new_fov;

    [Header ("Grappl SmoothDamp Booleans")]
    private bool smthDmp_grpl = false;
    private bool smthDmp_grpl_end = false;

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

    [Header ("Grapple Point Position")]
    [HideInInspector] public Vector3 _grplPoint_;
    private bool end_trans_called = false;

    private int iterator_;

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


    private List<string> nms_ = new List<string>(new string[3] {"wallR_x_offst", "wallR_y_offst", "wallR_z_offst"});
    private void Update(){
        if (!Input.GetKeyDown("q") && !Input.GetKeyDown("d") ){rotate_back = true;}else{
            rotate_back = false;
        }
        if(c_.fieldOfView != new_fov){
            c_.fieldOfView = Mathf.Lerp(c_.fieldOfView, new_fov, 0.85f);
        }

        if(smthDmp_grpl) rot_dc["wallR_rot_x_offst"] = Mathf.SmoothDamp(rot_dc["wallR_rot_x_offst"], -0.08f, ref mathfRef_grpl, 0.295f);
        if(smthDmp_grpl_end) rot_dc["wallR_rot_x_offst"] = Mathf.SmoothDamp(rot_dc["wallR_rot_x_offst"], -0.74f, ref mathfRef_grpl, 0.295f);

        if(trns_fnc){
            int it_ = 0;
            for (int i = 0; i < values_flt.Count; i++){
                if( values_flt[i] == 0.0f) break;
                
                bool s_ =  nms_.Contains(values_ref[i]);
                if(!s_){
                    // Debug.Log( ( !s_ ? rot_dc[values_ref[i]] : pos_dc[values_ref[i]] ) +
                    //             " vs " + values_ref[i] );
                }

                // const float indexed_ref = mathRef_arr[i];

                if(trns_back_arr[i] == null)
                {
                    it_ ++;
                }else 
                {

                    if(!s_) // ROTATIONS MOVEMENTS
                    {
                        if(trns_back_arr[i] == false)
                        {
                            rot_dc[values_ref[i]] = Mathf.SmoothDamp( rot_dc[values_ref[i]], values_flt[i], ref mathfRef, 0.070f); 
                            if( Math.Abs(rot_dc[values_ref[i]]) >= Math.Abs(values_flt[i]) - 0.002f) {
                                trns_back_arr[i] = true;
                            }
                        }
                        else if (trns_back_arr[i] == true)
                        { 
                            rot_dc[values_ref[i]] = Mathf.SmoothDamp(rot_dc[values_ref[i]], 0.00f, ref mathfRef0_, 0.250f); 
                            //if( rot_dc[values_ref[i]] == 0.0f) {
                            if(Math.Abs(rot_dc[values_ref[i]]) < 0.002f ){
                                it_++;
                                trns_back_arr[i] = null;
                            }
                        }  
                    }
                    else // POSITIONS MOVEMENTS 
                    {
                        if(trns_back_arr[i] == false)
                        {
                            pos_dc[values_ref[i]] = Mathf.SmoothDamp( pos_dc[values_ref[i]], values_flt[i], ref mathfRef_pos, 0.100f);
                            if( Math.Abs(pos_dc[values_ref[i]]) >= Math.Abs(values_flt[i]) - 0.001f ) { 
                                trns_back_arr[i] = true; 
                            }
                        }
                        else if (trns_back_arr[i] == true)
                        { 
                            pos_dc[values_ref[i]] = Mathf.SmoothDamp(pos_dc[values_ref[i]], 0.00f, ref mathfRef0_pos, 0.070f); 
                            if(Math.Abs(pos_dc[values_ref[i]]) < 0.0005f ){
                            //if( pos_dc[values_ref[i]] == 0.0f){
                                it_++;
                                trns_back_arr[i] = null;
                            }
                        }  
                    }    

                }
      
                if (it_ == iterator_){ trns_fnc = false; } 
                if(!trns_fnc) {
                    //Debug.Log("WHOLE RESET !");
                    reset_smoothDmpfnc();
                    break;
                }
            }
        }

    }

    private void reset_smoothDmpfnc() {

        mathfRef = mathfRef0_ = mathfRef_pos = mathfRef0_pos = 0.0f; 
        mathRef_arr = new List<float>(new float[6] {
            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f
        }); 
        for(int b = 0; b < iterator_; b ++)
        {
            bool s_ =  nms_.Contains(values_ref[b]);
            if(s_) rot_dc[values_ref[b]] = 0.0f;
            else pos_dc[values_ref[b]] = 0.0f;
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

        if(_grplPoint_ != new Vector3(0,0,0)){
            if( !end_trans_called && (player.position.z > _grplPoint_.z + 2.0f) )
            {
                end_grpl_Cm();
            }
        } else { end_trans_called = false; }

        tyro_on = FindObjectOfType<PlayerMovement>().plyr_tyro;
        sliding_on =  FindObjectOfType<PlayerMovement>().plyr_sliding;

    }

    private void LateUpdate() {
       // transform.rotation.x = xRot
       
        if (!game_Over_){
            // Dampen towards the target rotation
            //Quaternion initial_rt  = new Quaternion(15, gameObject.transform.rotation.y, 0, 1);  
            Quaternion desired_rt  = new Quaternion(xRot + supl_xRot + rot_dc["wallR_rot_x_offst"],
                 (x_offst / 112.0f) + rot_dc["wallR_rot_y_offst"], 
                (x_offst / 10000.0f) + rot_dc["wallR_rot_z_offst"],
                 1
            );

            transform.localRotation = Quaternion.Slerp(gameObject.transform.rotation, desired_rt,  tyro_on ? 0.07f : 0.15f);

            // Smooth Damp
            Vector3 smoothFollow = Vector3.SmoothDamp(
                transform.position,
                desired_ + (tyro_on ? new Vector3(0f, 0.5f, 3.0f) : new Vector3(0f,0f,0f)) + new Vector3(pos_dc["wallR_x_offst"], pos_dc["wallR_y_offst"] + supl_yOff, pos_dc["wallR_z_offst"]),
                ref currentVelocity,
                tyro_on ? 0.15f : 0.06f
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
            pos_dc["wallR_z_offst"] = 2.20f;

            float sns =  player.position.x - gm_.position.x;
            if(sns < 0 ){
                pos_dc["wallR_x_offst"] = -1.35f; 
                rot_dc["wallR_rot_y_offst"] = 0.150f; rot_dc["wallR_rot_z_offst"] = 0.14f;

            }else{
                pos_dc["wallR_x_offst"] = 1.35f;
                rot_dc["wallR_rot_y_offst"] = -0.150f; rot_dc["wallR_rot_z_offst"] = -0.14f;
            }
        }
    }




    // PUBLIC CAM VALUES TRANS FOR GRAPPLING
    public void grpl_offset(bool is_ext, Transform gm_ = null){
        if(is_ext){
            //FovTrans(85f, 0.5f);
            supl_xRot = 0.0f;
            new_fov = start_fov;
          
            pos_dc["wallR_x_offst"] = 0.0f; Invoke("delay_yOf_grpl", 0.55f); pos_dc["wallR_z_offst"] = 0.0f;
            rot_dc["wallR_rot_z_offst"] = 0.0f; rot_dc["wallR_rot_y_offst"] = 0.0f; rot_dc["wallR_rot_x_offst"] = 0.0f;
        }else{
            if(gm_){
                // // //
                reset_smoothDmpfnc();
                // // //

                new_fov = 87f;
                float sns =  player.position.x - gm_.position.x;
                // CLOSE UP Z OFFSET
                pos_dc["wallR_z_offst"] = 1.45f;
                pos_dc["wallR_y_offst"] = -0.65f;

                rot_dc["wallR_rot_x_offst"] = 0.25f;

                // SMOOTH DAMP FOR X ROTATION boolean
                //wallR_rot_x_offst = Mathf.SmoothDamp(wallR_rot_x_offst, -0.10f, ref mathfRef, 3f);
                smthDmp_grpl = true;

                if(sns < 0 ) { pos_dc["wallR_x_offst"] = 0.3f; rot_dc["wallR_rot_z_offst"] = -0.055f;} else{
                    rot_dc["wallR_rot_z_offst"] = 0.055f;
                    pos_dc["wallR_x_offst"] = -0.3f; 
                }
            }
        }
    }
    private void delay_yOf_grpl(){
        _grplPoint_ = new Vector3(0,0,0);
        smthDmp_grpl_end = false; smthDmp_grpl = false;
        rot_dc["wallR_y_offst"] = 0f;
    }



    // END GRAPPLING CAMERA MOVEMENT TRANSITION
    private void end_grpl_Cm(){
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
    public void sld_offset(bool is_ext){
        if(is_ext){
            //FovTrans(85f, 0.5f);
            supl_xRot = 0.0f;
            new_fov = start_fov;
            pos_dc["wallR_x_offst"] = 0.0f; pos_dc["wallR_y_offst"] = 0.0f; pos_dc["wallR_z_offst"] = 0.0f;
            rot_dc["wallR_rot_z_offst"] = 0.0f; rot_dc["wallR_rot_y_offst"] = 0.0f; rot_dc["wallR_rot_x_offst"] = 0.0f;
        }else{
            reset_smoothDmpfnc();

            new_fov = 86f;
            // CLOSE UP Z OFFSET
            pos_dc["wallR_z_offst"] = 1.00f;
            pos_dc["wallR_y_offst"] = -0.35f;

            // SMOOTH DAMP FOR X ROTATION
            rot_dc["wallR_rot_x_offst"] = -0.090f;       
        }
    }




    // PUBLIC CAM VALUES TRANS FOR OBSTCL JMP
    public void obs_offset(){
        // SMOOTH DAMP FOR X ROTATION
        pos_dc["wallR_y_offst"] = 0.35f;
        rot_dc["wallR_rot_x_offst"] = 0.14f;  
        reset_smoothDmpfnc();  
        Invoke("obst_rst", 0.75f);   
    }
    private void obst_rst(){
        supl_xRot = 0.0f;
        pos_dc["wallR_x_offst"] = 0.0f; pos_dc["wallR_y_offst"] = 0.0f; pos_dc["wallR_z_offst"] = 0.0f;
        rot_dc["wallR_rot_z_offst"] = 0.0f; rot_dc["wallR_rot_y_offst"] = 0.0f; rot_dc["wallR_rot_x_offst"] = 0.0f;
    }




    // PUBLIC FUNC FOR JUMP SUPER SMOOTH TRANS
    public void jmp(bool is_dblJmp){
        // wallR_rot_x_offst = -0.052f; 
        // wallR_y_offst = -0.20f;    
          
        iterator_ = 3;
        List<float> v_flt = new List<float>(new float[6] {-0.095f, -0.85f,is_dblJmp ?  -0.025f : 0.025f, 0.0f, 0.0f, 0.0f} ); 
        List<string> s_arr = new List<string>(new string[6] {"wallR_rot_x_offst", "wallR_y_offst", "wallR_rot_z_offst", "", "",""} ); 
 
        values_ref = s_arr;
        values_flt = v_flt;
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
