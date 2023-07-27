using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVectors : MonoBehaviour
{
    // Init grnds to be slippery arr that gets updated
    private GameObject[] acutal_grnds = new GameObject[15];
    // Slippery Inspector Mat
    public PhysicMaterial slippery_mat;

    private void Start(){}

    // Update and Replace slippery arr
    public void slippery_trigr(bool is_exit, GameObject init_gmbj){
        Debug.Log(init_gmbj);
        // Disable previous slippery arr
        if(acutal_grnds[0] != null){
            for(int j = 0; j < acutal_grnds.Length; j++){
                Collider col_ = acutal_grnds[j].GetComponent<Collider>();
                col_.material = null;
            }
        }
 

        GameObject prnt_gmbj = init_gmbj.transform.parent.gameObject;
        int i = 0;
        // maybe parent is ground obj
        if(prnt_gmbj.GetComponent<MeshCollider>().tag == "ground" || prnt_gmbj.GetComponent<BoxCollider>().tag == "ground"){
            // acutal_grnds[0]
            acutal_grnds[i] = prnt_gmbj;
            i = 1;
        }else{
            foreach(Transform chld_ in prnt_gmbj.transform){
                if(chld_.gameObject.GetComponent<BoxCollider>().tag == "ground" || chld_.gameObject.GetComponent<MeshCollider>().tag == "ground"){
                    acutal_grnds[i] = chld_.gameObject;
                    i++;
                }
            }
        }

        // Clear rest of prev slippery arr
        for(int p = i; p < acutal_grnds.Length; p ++){ acutal_grnds[p] = null;}

        // Apply slippery
        for (int k = 0; k < acutal_grnds.Length; k++){
            // get main collider
            Collider col_ = acutal_grnds[k].GetComponent<Collider>();
            col_.material = slippery_mat;
        }
    }

    // Apply vectors to near walls
    private void apply_vector(){
        
    }
}
