using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraMovement : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    private Vector3 svd_offst;
    private float smoothTrans_  = 2.5f;
    private bool game_Over_ = false;
    // private float updtd_plyr_offset;
    // private float prev_plyr_offset;
    // Update is called once per frame
    private float vert_Trns = 0.9f; // VERT CLAMP TIME
    private float vert_y_pos = 0f;
    private float vert_y_rot = 0f;

    private void Start(){
        svd_offst = offset;
    }
    public void cam_GamerOver_(){
        game_Over_ = true;
    }

    public void fly_dynm(bool is_leave){
        StopCoroutine(vert_rt(true)); 
        if(is_leave){
            //StartCoroutine(vert_rt(true));
            return;
        }
        //StartCoroutine(vert_rt(false));
    }

    private void FixedUpdate() {
        if (!game_Over_){
            //updtd_plyr_offset = ((FindObjectOfType<PlayerMovement>().swipe_offst) * 10 ) / 2; 
            float x_offst = (player.rotation.eulerAngles.y > 298.0f ? -1 *   (60 - (player.rotation.eulerAngles.y - 300.0f)) : player.rotation.eulerAngles.y) ;

            // Dampen towards the target rotation
            //Quaternion initial_rt  = new Quaternion(15, gameObject.transform.rotation.y, 0, 1);  
            Quaternion desired_rt  = new Quaternion(gameObject.transform.rotation.x + 0.00035f + vert_y_rot, x_offst / 185, 0, 1); 
            transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, desired_rt, 0.7f);

            // Lerp Position
            Vector3 desired_  = player.position + offset;
            desired_.x += -1 * (x_offst / 18);
            desired_.z += (Math.Abs(x_offst)) / 80;
            desired_.y += vert_y_pos;
            Vector3 smoothed_ = Vector3.Lerp(player.position, desired_, 0.6f);
            gameObject.transform.position = desired_;

        }else{
            transform.LookAt(player);
        }
    }

    private IEnumerator vert_rt(bool loop_sns){
        float t_tick = vert_Trns / 120;
        float y_posOff_tick = 1.5f / 120;
        float y_posRot_tick = (1.25f / 120) * 0.0001f;

        for (int i = 0; i < 120; i ++){
            if(loop_sns){
                if(vert_y_pos < 1.5f){vert_y_pos += y_posOff_tick;}
                if(vert_y_rot < (1.25f * 0.00001f) ){vert_y_rot += y_posRot_tick;}
            }else{
                if(vert_y_pos > 0.1f){ vert_y_pos -= y_posOff_tick;}
                if (vert_y_rot > 0.1f){ vert_y_rot -= y_posRot_tick;}
            }
            yield return new WaitForSeconds(t_tick);
        }
        Debug.Log(vert_y_pos);
    }

    private IEnumerator rtation(){
      yield return new WaitForSeconds(1f);
    }
}
