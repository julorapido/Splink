 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    private PlayerMovement pm_;
    
    [Header ("References")]
    [SerializeField] private Rigidbody rb_;
    [SerializeField] private Transform plyr_pos;
    [SerializeField] private Transform gunTip;
    [SerializeField] private LayerMask wt_grappleable;

    [Header ("Grappling Cooldowns & MaxDistance")]
    [SerializeField] private float mx_grappl_distance;
    [SerializeField] private float max_grpl_time;
    private float grpl_TimeValue;

    [Header ("Grappnling Jump Overshoot")]
    public float overshootYAxis;

    [Header ("Grappnling Point & GM")]
    private Vector3 gplr_point;
    private GameObject gplr_gm;


    [Header ("Grapple Hold Booleans")]
    private bool is_grpling_;
    private bool can_reGrappl = true;


    [Header ("Grapple Dash Boolean")]
    private bool activ_grapple = false;


    [Header ("Spring Hold Grapple")]
    private SpringJoint hld_joint;


    [Header ("Grapple Hold LineRenderer")]
    [SerializeField] private AnimationCurve line_curve;
    [SerializeField] private LineRenderer lr;
    private SpringJoint line_spring;
    private int line_quality = 50;
    public float waveCount = 1f, waveHeight = 20f;

    [Header ("Grapple Dash LineRenderer")]
    [SerializeField] private LineRenderer dash_lr;
    private int resolution = 100, waveCountd_ = 8, wobbleCount = 5;
    private float waveSize = 2f, animSpeed = 0.1f;


    private void Start()
    {
        pm_ = GetComponent<PlayerMovement>();
    }

    private void LateUpdate()
    {
        // DRAW ROPE
        DrawRope(); 
    }

    private void Update()
    {
        if (Input.GetKeyDown("e")) StartGrapple();

        if ( (Input.GetKeyUp("e") ||  grpl_TimeValue < 0.01f) && is_grpling_) StopGrapple();
            
        
        if(grpl_TimeValue > 0) grpl_TimeValue -= Time.deltaTime;



        // Detect Grapple Ballz
        Collider[] hitColliders = Physics.OverlapSphere(plyr_pos.position, 20f);
        if(hitColliders.Length > 0)
        {
            foreach (var hitCollider in hitColliders)
            {
                if(hitCollider.gameObject.tag == "grapple_ball")
                {
                    //Debug.Log("grappl ball in sight");
                    // GRAPPLE JUMP
                    // Invoke(nameof(ExecuteGrapple), grpl_dely_time);
                }
            }
        }


    }


    public void soft_Grapple()
    {
        if(hld_joint)
            hld_joint.spring *= 1.2f; // ELASTIC STRENGTH
            hld_joint.damper *= 1.2f;
            hld_joint.maxDistance = hld_joint.maxDistance * 1.05f;

    }



    private void DrawRope()
    {
        if(!is_grpling_)
        {
            lr.positionCount = 0;
            if(line_spring != null) Destroy(line_spring);
            return;
        }

        GrappleDashRope(gplr_point);

        if(line_spring == null) {
            line_spring = gameObject.AddComponent<SpringJoint>();
            //line_spring.autoConfigureConnectedAnchor = false;
            line_spring.connectedAnchor = gplr_point;
        }

        if(lr.positionCount == 0)
        {
            lr.positionCount = line_quality + 0;
        }

        hld_joint.spring = 500f; // Elastic Strength
        hld_joint.damper = 14f; // Elastic Damper
        hld_joint.massScale = 10f; // Elastic Mass

        Vector3 up = Quaternion.LookRotation( (gplr_point - gunTip.position ).normalized) * Vector3.up;
        for(int i = 0; i < line_quality; i ++)
        {
            var delta = i / (float) line_quality;
            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI)  * line_curve.Evaluate(delta) ;

            lr.SetPosition(i, Vector3.Lerp(gunTip.position, gplr_point, delta) + offset);
        }
    }




    private void StartGrapple()
    {
        if(grpl_TimeValue > 0 || !can_reGrappl) return;
        bool trgrd_ = false;

        RaycastHit[] ray_hits = new RaycastHit[(155 * 2) + 1];
        int indx_ = 1;
        Physics.Raycast(plyr_pos.position, plyr_pos.forward, out ray_hits[0], mx_grappl_distance, wt_grappleable);

        for(int i = 1; i < 155; i++){// LEFT
            Physics.Raycast(plyr_pos.position + new Vector3(-i/12, 0.65f + (i/12), 0.2f), plyr_pos.forward, out ray_hits[indx_], mx_grappl_distance, wt_grappleable);
            if(ray_hits[indx_].point != Vector3.zero ){
                trgrd_ = true;
                //break;
                indx_++;
            }
        }

        for(int j = 1; j < 155; j++){// RIGHT
            Physics.Raycast(plyr_pos.position + new Vector3(j/12, 0.65f + (j/12), 0.2f), plyr_pos.forward , out ray_hits[indx_], mx_grappl_distance, wt_grappleable);
            if(ray_hits[indx_].point != Vector3.zero){
                trgrd_ = true;
                //break; 
                indx_++;
            }
        }

  
        // DEFINE GRPL POINT
        for(int k = 0; k < indx_; k++)
        {
            int pt_ = Random.Range(1, indx_);
            if(ray_hits[k].point != Vector3.zero)
            {
                Vector3  p = ray_hits[k].point;
                float d_fm_p = Vector3.Distance(plyr_pos.position, p);
                float z_dst = p.z - plyr_pos.position.z;
                if( (d_fm_p > 15 && d_fm_p < 35) && z_dst >= 12)
                {
                    // 3f height minimum
                    if( (p.y - plyr_pos.position.y) >= 4f)
                    {
                        //if(ray_hits[k].transform.gameObject.collider.tag == "ground"){
                            gplr_point = p;
                            gplr_gm = ray_hits[k].transform.gameObject;
                            break;
                        //}
                    }
                }
            }
        }
        if(gplr_point == Vector3.zero) return;


        // GRAPPLE SPRING HOLD
        hld_joint  = gameObject.AddComponent<SpringJoint>();
        hld_joint.autoConfigureConnectedAnchor = false;
        hld_joint.connectedAnchor = gplr_point;

    
        float dist_frm_point = Vector3.Distance(plyr_pos.position, gplr_point);

        // the distance grapple try to keep from grappl point
        hld_joint.maxDistance = dist_frm_point * 1.10f;
        hld_joint.minDistance = dist_frm_point * 0.60f;

        // cutsom values
        hld_joint.spring = 3f; // ELASTIC STRENGTH
        hld_joint.damper = 14f;
        hld_joint.massScale = 10f;


        if(!trgrd_)
        {
            lr.enabled = false;
        }else
        {
            grpl_TimeValue = max_grpl_time;

            // Set lineRenderer
            lr.enabled = true;
            // lr.SetPosition(0, gunTip.position);
            // lr.SetPosition(1, gplr_point);

            is_grpling_ = true;

            // CALL PLAYER MOVEMENT GRAPPLE ANIMATION
            FindObjectOfType<PlayerMovement>().swing_anm(false, gplr_point);
            FindObjectOfType<PlayerMovement>().rotate_bck();

            // CALL CAMERA GRAPPLE MOVEMENTS
            FindObjectOfType<CameraMovement>()._grplPoint_ = gplr_point;
            FindObjectOfType<CameraMovement>().grpl_offset(false, gplr_gm.transform);
        }
      
    }

    
    private void StopGrapple()
    {
        is_grpling_ = false;
        grpl_TimeValue = 0f;
        Destroy(hld_joint);

        gplr_point = new Vector3(0, 0, 0);
        lr.enabled = false;

        // Player Mvmnt fnc call
        FindObjectOfType<PlayerMovement>().swing_anm(true, new Vector3(0,0,0));
        FindObjectOfType<CameraMovement>().grpl_offset(true);

        Rigidbody ply_r = plyr_pos.gameObject.GetComponent<Rigidbody>();

        ply_r.velocity = new Vector3(ply_r.velocity.x, ply_r.velocity.y, ply_r.velocity.z / 1.75f);
        ply_r.AddForce( new Vector3(0, 10, 2), ForceMode.VelocityChange);
    }




    // // // // // // // GRAPPLE DASH // // // // // // // // // // 
    private void ExecuteGrapple()
    {  
        float grapplePointRelativeYPos = gplr_point.y - (plyr_pos.transform.position.y - 1.5f);
        float highestPointOnArc = (grapplePointRelativeYPos < 0) ? overshootYAxis : (grapplePointRelativeYPos + overshootYAxis);

        JumpToPosition(gplr_point,  highestPointOnArc); 
    }


    private void JumpToPosition (Vector3 targetPos, float traj_height)
    {
        activ_grapple = true;
        rb_.velocity = CalculateJumpVelocity(plyr_pos.position, targetPos, traj_height);
        activ_grapple = false;
    }

    private Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
                + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));
        
        return velocityXZ + velocityY;
    }


    private IEnumerator GrappleDashRope(Vector3 target_)
    {
        dash_lr.enabled = true;
        dash_lr.positionCount = resolution;

        float percent = 0f;
        float angle = Vector3.Angle(target_, gunTip.position);

        while(percent <= 1f)
        {
            percent += Time.deltaTime * animSpeed;
            SetDashRopePoints(percent, angle);
            yield return null;
        }
        SetDashRopePoints(1, angle);
        yield return null;
    }

    private void SetDashRopePoints(float percent, float angl)
    {
        Vector3 ropeEnd = Vector3.Lerp(gunTip.position, gplr_point, percent);
        float len = Vector2.Distance(gunTip.position, ropeEnd);

        for (int i = 0; i < resolution; i ++)
        {
            float xPos = (float)i / resolution * len;

            float amplitude = Mathf.Sin( (1 - percent) * wobbleCount * Mathf.PI);
        
            float yPos = Mathf.Sin( (float) waveCountd_ * i / resolution * 2 * Mathf.PI * (1 - percent) ) * amplitude;

            //Vector2 pos = RotatePoint(new Vector2(gunTip.position.x + xPos, gunTip.position.y + yPos), gunTip.position, angl);

            Vector3 pos3 = new Vector3(xPos, yPos, gplr_point.z);
            dash_lr.SetPosition(i, pos3);
        }
    }

    private Vector2 RotatePoint(Vector2 point, Vector2 pivot, float angle)
    {
        Vector2 dir = point - pivot;
        dir = Quaternion.Euler(0, 0, angle) * dir;
        point = dir + pivot;
        return point;
    }
}

 