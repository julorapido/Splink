using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using PathCreation.Utility;

public class PlayerCollisions : MonoBehaviour
{
    //public Collider mmbr_collider;

    [Header ("Collisions Constants")]
    public string[] colsions_values = new string[4]{"ground", "frontwall","sidewall", "slider"};

    [Header ("SubCollisions Constants")]
    private string[] subcolsions_values = new string[5]{"obstacleHit", "launcherHit","tyro", "bumper", "tapTapJump"};

    [Header ("-> Selected Collision <-")]
    public string slcted_clsion;

    [Header ("Ground Physical Mat")]
    [HideInInspector] public PhysicMaterial grnd_mat;

    [Header ("Start Delay")]
    private float strt_delay = 0.5f;
    private bool can_trgr = false;
    private Transform ply_transform;
    private Vector3 currentVelocity;

    [Header ("Sider Wall last_registered_gm")]
    private int lst_wall;

    private void Start(){
        StartCoroutine(delay_trgrs(strt_delay));
    }

    // private void OnCollisionEnter(Collision other) {
    //     Vector3 _size = other.collider.bounds.size;
    //     switch (slcted_clsion){
    //             case "ground":
    //                 // Grnd hit
    //                 if(other.collider.gameObject.tag == "ground")
    //                 {
    //                     FindObjectOfType<PlayerMovement>().animateCollision("groundHit", _size);
    //                     if(grnd_mat != null)
    //                     {
    //                         Collider p =  other.collider.gameObject.GetComponent<Collider>();
    //                         if(p){ p.sharedMaterial = grnd_mat;}
    //                     }
    //                 }
    //                 break;
    //             default:
    //                 break;
    //     }
    // }

    private void OnTriggerEnter(Collider collision) {
        if(slcted_clsion.Length > 0 && can_trgr){
            Vector3 _size = collision.bounds.size;
            switch (slcted_clsion){
                case "ground":
                    // Grnd hit
                    if(collision.gameObject.tag == "ground")
                    {
                        FindObjectOfType<PlayerMovement>().animateCollision("groundHit", _size);
                        if(grnd_mat != null)
                        {
                            Collider p =  collision.gameObject.GetComponent<Collider>();
                            if(p){ p.sharedMaterial = grnd_mat;}
                        }
                    }
                    // Obstcl hit
                    if(collision.gameObject.tag == "obstacle") FindObjectOfType<PlayerMovement>().animateCollision("obstacleHit", _size, collision.gameObject);

                    // Launcher jmp
                    if(collision.gameObject.tag == "launcher") FindObjectOfType<PlayerMovement>().animateCollision("launcherHit", _size);

                    // Tyro hit
                    if(collision.gameObject.tag == "tyro") FindObjectOfType<PlayerMovement>().tyro_movement(collision.gameObject);

                    // Bumper jmp
                    if(collision.gameObject.tag == "bumper")
                    {
                        GameObject pr_gm = collision.gameObject.transform.parent.gameObject == null ? collision.gameObject.transform.parent.gameObject : collision.gameObject;
                        LeanTween.scale(pr_gm, pr_gm.transform.localScale * 1.2f, 0.4f).setEasePunch();
                        FindObjectOfType<PlayerMovement>().animateCollision("bumperJump", _size);
                    }

                    // Tap Tap Jump
                    if(collision.gameObject.tag == "tapTapJump")
                    {
                         FindObjectOfType<PlayerMovement>().animateCollision("tapTapJump", _size, collision.gameObject);
                    }
                    break;

                case "frontwall":
                    // front wall gameover
                    if(collision.gameObject.tag == "ground")
                    {
                        FindObjectOfType<PlayerMovement>().animateCollision("frontWallHit", _size);
                    }
                    break;

                case "sidewall":
                    // Sidewall hit
                    if(collision.gameObject.tag == "ground")
                    {
                        if(lst_wall != null && lst_wall != collision.gameObject.GetInstanceID())
                        {
                            lst_wall = (collision.gameObject.GetInstanceID());
                            Vector3 targetDir = collision.gameObject.transform.position - ply_transform.position;
                            float angle = Vector3.Angle(targetDir, transform.forward);
                            FindObjectOfType<PlayerMovement>().animateCollision("wallRunHit", _size);
                            FindObjectOfType<PlayerVectors>().slippery_trigr(false, collision.gameObject);
                            FindObjectOfType<CameraMovement>().wal_rn_offset(false, collision.gameObject.transform);
                        }
                    }
                    break; 

                case "slider":
                    if(collision.gameObject.tag == "slider"){
                        //LeanTween.scale(collision.gameObject, collision.gameObject.transform.localScale * 1.05f, 0.4f).setEasePunch();
                        Vector3 dwned = collision.gameObject.transform.position;
                        dwned.y -= 0.1f;
                        Vector3 smoothDwn_ = Vector3.SmoothDamp(collision.gameObject.transform.position, dwned, ref currentVelocity, 0.4f); 
                        FindObjectOfType<PlayerMovement>().animateCollision("sliderHit", _size);
                        FindObjectOfType<CameraMovement>().sld_offset(false);
                    }  
                    break;

                default:
                    break;
            }
        }
    }
    
    private void OnTriggerExit(Collider collision) {
        if(slcted_clsion.Length > 0 && can_trgr){
            Vector3 _size = collision.bounds.size;
            switch (slcted_clsion)
            {
                case "ground":
                    if(collision.gameObject.tag == "ground")
                    {
                        FindObjectOfType<PlayerMovement>().animateCollision("groundLeave", _size);
                    }
                    if(collision.gameObject.tag == "obstacle")
                    {
                        FindObjectOfType<PlayerMovement>().animateCollision("obstacleLeave", _size, collision.gameObject);
                    }
                    break;

                case "sidewall":
                    if(collision.gameObject.tag == "ground")
                    {
                        FindObjectOfType<PlayerMovement>().animateCollision("wallRunExit", _size);
                        FindObjectOfType<PlayerVectors>().slippery_trigr(true, collision.gameObject);
                        FindObjectOfType<CameraMovement>().wal_rn_offset(true, collision.gameObject.transform);
                    }
                    break; 

                case "slider":
                    if(collision.gameObject.tag == "slider")
                    {
                        LeanTween.scale(collision.gameObject, collision.gameObject.transform.localScale * 1.08f, 1f).setEasePunch();
                        Vector3 upped = collision.gameObject.transform.position;
                        upped.y += 0.1f;
                        Vector3 smoothUp_ = Vector3.SmoothDamp(collision.gameObject.transform.position, upped, ref currentVelocity, 0.4f); 
                        
                        FindObjectOfType<PlayerMovement>().animateCollision("groundLeave", _size);
                        FindObjectOfType<PlayerMovement>().animateCollision("sliderLeave", _size);
                        FindObjectOfType<CameraMovement>().sld_offset(true);
                    }  
                    break;

                default:
                    break;
            }
        }
    }

    private IEnumerator delay_trgrs(float dl){
        yield return new WaitForSeconds(dl);
        can_trgr = true;
    }
    
    private IEnumerator sid_wl_delay(){
        yield return new WaitForSeconds(0.6f);
    }
}
