using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraMovement : MonoBehaviour
{
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

    [Header ("Player Side Htboxs")]
    private float side_x_offst = 0.0f;
    private float side_y_offst = 0.0f;
    private float side_rot_y_offst = 0.0f;


    private void Start(){
        def_offset = offset;
        //Application.targetFrameRate = 60;
        xRot = gameObject.transform.rotation.x;
        gameObject.transform.position = player.position + offset;
        desired_  = (player.position + offset);

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

    // FXED UPDATE for Values
    private Vector3 desired_;
    private float lst_offst_x;
    private Quaternion desired_rt;
    private float x_offst;
    //[SerializeField] float smoothSpeed = 9.0f;
    private float smoothTime = 2f ;
    private Vector3 currentVelocity;

    private bool tyro_on = false;
    private bool rotate_back = false;

    private void Update(){
        if (!Input.GetKeyDown("q") && !Input.GetKeyDown("d") ){rotate_back = true;}else{
            rotate_back = false;
        }
    }

    private void FixedUpdate(){
        x_offst = (player_rb.rotation.eulerAngles.y > 298.0f ? -1 *   (60 - (player_rb.rotation.eulerAngles.y - 300.0f)) : player_rb.rotation.eulerAngles.y);
        // Lerp Position
        if(((-0.050f * x_offst)) != lst_offst_x){
            desired_  = (player.position + offset);
            desired_.x = desired_.x +  ((-0.050f * x_offst));
            desired_.z = desired_.z +  (Math.Abs(x_offst)) / 100;
            lst_offst_x = ((-0.050f * x_offst));
        }

        tyro_on = FindObjectOfType<PlayerMovement>().plyr_tyro;
        //lst_frm_desired_ = desired_;
        //desired_.y += vert_y_pos;
   
    }

    private void LateUpdate() {
       // transform.rotation.x = xRot
       
        if (!game_Over_){
            // Dampen towards the target rotation
            //Quaternion initial_rt  = new Quaternion(15, gameObject.transform.rotation.y, 0, 1);  
            Quaternion desired_rt  = new Quaternion(xRot, (x_offst / 130.0f) + side_rot_y_offst, (x_offst / 1500.0f), 1);
            transform.localRotation = Quaternion.Slerp(gameObject.transform.rotation, desired_rt, 0.09f);

            // Smooth Damp
            Vector3 smoothFollow = Vector3.SmoothDamp(
                transform.position,
                desired_ + (tyro_on ? new Vector3(0f, 0.5f, 3.0f) : new Vector3(0f,0f,0f)) + new Vector3(side_x_offst, side_y_offst, 0f),
                ref currentVelocity,
                tyro_on ? 0.175f : 0.08f
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


    private IEnumerator rtation(){
      yield return new WaitForSeconds(1f);
    }

    public void wal_rn_offset(bool is_ext, Transform gm_){
        if(is_ext){
            side_x_offst = 0.0f; side_rot_y_offst = 0.0f; side_y_offst = 0.0f;
        }else{
            side_y_offst = -1.20f;
            float sns =  player.position.x - gm_.position.x;
            if(sns < 0 ){
                side_x_offst = -1.75f; 
                side_rot_y_offst = 0.20f;
            }else{
                side_x_offst = 1.75f;
                side_rot_y_offst = -0.20f;
            }
        }
    }
}
