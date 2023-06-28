using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraMovement : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    private Vector3 svd_offst;
    private float smoothTrans_  = 0.1250f;

    // private float updtd_plyr_offset;
    // private float prev_plyr_offset;
    // Update is called once per frame
    private void Start(){
        svd_offst = offset;
    }

    private void FixedUpdate() {

        //updtd_plyr_offset = ((FindObjectOfType<PlayerMovement>().swipe_offst) * 10 ) / 2; 
        float x_offst = player.rotation.eulerAngles.y > 298.0f ? -1 *   (60 - (player.rotation.eulerAngles.y - 300.0f)) : player.rotation.eulerAngles.y;

        // Dampen towards the target rotation
        //Quaternion initial_rt  = new Quaternion(15, gameObject.transform.rotation.y, 0, 1);  
        Quaternion desired_rt  = new Quaternion(gameObject.transform.rotation.x, x_offst / 225, 0, 1); 
        transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, desired_rt, smoothTrans_ * 4);

        // Lerp Position
        Vector3 desired_  = player.position + offset;
        desired_.x += -1 * (x_offst / 24);
        desired_.z += (Math.Abs(x_offst)) / 100;
        Vector3 smoothed_ = Vector3.Lerp(player.position, desired_, smoothTrans_ * 2);
        gameObject.transform.position = desired_;

        //transform.LookAt(player);
    }

    private IEnumerator rtation(){
      yield return new WaitForSeconds(1f);
    }
}
