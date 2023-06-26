using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    // Update is called once per frame
    private void Update()
    {
       // if (FindObjectOfType<GameManager>().gameHasEnded == false ){
        //    transform.position = player.position + offset;
      //  }else {
       gameObject.transform.position = new Vector3(player.position.x + offset.x, player.position.y + offset.y, player.position.z + offset.z);
       // }
    }
}
