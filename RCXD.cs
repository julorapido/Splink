using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RCXD : MonoBehaviour
{
    private enum robot_Type{
        Classic,
        ZE
    };
    [SerializeField]
    robot_Type t_type = new robot_Type();



    // Start is called before the first frame update
    private void Start()
    {
        
    }

  
}
    