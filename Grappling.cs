 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header ("References")]
    private PlayerMovement pm_;
    public Transform cam;
    public Transform plyr_pos;
    public Transform gunTip;
    public LayerMask wt_grappleable;
    public LineRenderer lr;

    [Header ("Grappling")]
    public float mx_grappl_distance;
    public float grpl_dely_time;
    
    private Vector3 gplr_point;


    [Header ("Cooldown")]
    public float grpl_cd;
    private float grpl_cd_timer;


    [Header ("Input")]
    public KeyCode grapplekey;

    private bool is_grpling_;

    // Start is called before the first frame update
    private void Start()
    {
        pm_ = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown("e")) StartGrapple();

        if(grpl_dely_time > 0)
            grpl_cd_timer -= Time.deltaTime;
    }

    private void LateUpdate(){
        if(is_grpling_)
            lr.SetPosition(0, gunTip.position);
    }

    private void StartGrapple(){
        if(grpl_cd_timer > 0) return;
        is_grpling_ = true;

        RaycastHit hit;
        if(Physics.Raycast(plyr_pos.position, plyr_pos.forward, out hit, mx_grappl_distance, wt_grappleable) ){
            gplr_point = hit.point;
            Invoke(nameof(ExecuteGrapple), grpl_dely_time);
        }else{
            gplr_point = plyr_pos.position + plyr_pos.forward * mx_grappl_distance;  
            Invoke(nameof(StopGrapple), grpl_dely_time);
        }

        lr.enabled = true;
        lr.SetPosition(1, gplr_point);
    }

    private void ExecuteGrapple(){

    }

    private void StopGrapple(){
        is_grpling_ = false;
        grpl_cd_timer = grpl_cd;
        lr.enabled = false;
    }

}
