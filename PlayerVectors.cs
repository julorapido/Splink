using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVectors : MonoBehaviour
{
    // Init grnds to be slippery arr that gets updated
    private GameObject[] acutal_grnds = new GameObject[15];

    [Header ("Player Inspector")]
    public Transform plyr_trsnfm;

    [Header ("Player Side Htboxs")]
    public Collider side_plyr_cldr;

    // Slippery Inspector Mat
    [Header ("Slippery Material")]
    public PhysicMaterial slippery_mat;

    [Header ("Vector Gmobj")]
    public GameObject animated_vector;

    private void Start(){}

    // Update and Replace slippery arr
    public void slippery_trigr(bool is_exit, GameObject init_gmbj){
        // Htbox I/O
        side_plyr_cldr.enabled = true;
        if(is_exit){
            side_plyr_cldr.enabled = false;
        // make plyr rotate
        }else{ plyr_trsnfm.localRotation = Quaternion.Slerp(plyr_trsnfm.rotation, init_gmbj.transform.rotation, 0.4f);}

        // Disable previous slippery arr
        if(acutal_grnds[0] != null){
            for(int j = 0; j < acutal_grnds.Length; j++){
                if(acutal_grnds[j] == null){break;}
                Collider col_ = acutal_grnds[j].GetComponent<Collider>();
                col_.material = null;
            }
        }
 
        Collider pr_col_ = init_gmbj.transform.parent.gameObject.GetComponent<Collider>();
        GameObject prnt_gmbj =  ((pr_col_ != null) && (pr_col_?.tag == "ground")) ? null : init_gmbj.transform.parent.gameObject;
        
        int i = 0;
        // maybe parent is ground obj
        if(prnt_gmbj == null){
            // acutal_grnds[0]
            acutal_grnds[i] = init_gmbj.transform.parent.gameObject;
            i = 1;
        }else{
            foreach(Transform chld_ in prnt_gmbj.transform){
                Collider chld_cldrs = chld_.gameObject.GetComponent<Collider>();
                if(chld_cldrs != null){
                    //if(chld_?.gameObject?.GetComponent<BoxCollider>()?.tag == "ground" || chld_?.gameObject?.GetComponent<MeshCollider>()?.tag == "ground"){
                    if(chld_cldrs?.tag == "ground"){
                        acutal_grnds[i] = chld_.gameObject;
                        i++;
                    }
                }
            }
        }

        // Clear rest of prev slippery arr
        for(int p = i; p < acutal_grnds.Length; p ++){ acutal_grnds[p] = null;}

        // Apply slippery
        for (int k = 0; k < i; k++){
            // get main collider
            Collider col_ = acutal_grnds[k].GetComponent<Collider>();
            col_.material = slippery_mat;
        }
    }

    // Apply vectors to near walls
    private void apply_vector(){
        
    }
}
