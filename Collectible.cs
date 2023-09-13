using UnityEngine;
using System.Collections;

public class Collectible : MonoBehaviour {

    [HideInInspector]public bool isAnimated = true;
    private const float scaleAndFloatRate = 0.5f;

    [Header ("Settings")]
    public bool isRotating = false;
    public bool isFloating = false;
    public bool isScaling = false;

    [SerializeField]
    private Vector3 rotationAngle;
    private float rotationSpeed = 1f;

    [Header ("Floating Settings")]
    private float floatSpeed = 1f;
    private bool goingUp = true;
    private float floatTimer;
   
    private Vector3 startScale;
     [SerializeField]
    private Vector3 scaleEffect;

    private bool scalingUp = true;
    private float scaleSpeed = 1f;
    [Header ("Scale Settings")]
    private float scaleTimer;

	// Use this for initialization
	private void Start () {
        // init start scale 
        startScale = gameObject.transform.localScale;
	}
	
	// Update is called once per frame
	private void Update () {       
        
        if(isAnimated)
        {
            if(isRotating)
            {
                transform.Rotate(rotationAngle * rotationSpeed * Time.deltaTime);
            }

            if(isFloating)
            {
                floatTimer += Time.deltaTime;
                Vector3 moveDir = new Vector3(0.0f, floatSpeed / 130f, 0.0f);
                transform.Translate(moveDir);

                if (goingUp && floatTimer >= scaleAndFloatRate)
                {
                    goingUp = false;
                    floatTimer = 0;
                    floatSpeed = -floatSpeed;
                }

                else if(!goingUp && floatTimer >= scaleAndFloatRate)
                {
                    goingUp = true;
                    floatTimer = 0;
                    floatSpeed = +floatSpeed;
                }
            }

            if(isScaling)
            {
                scaleTimer += Time.deltaTime;

                if (scalingUp)
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, startScale + scaleEffect, scaleSpeed * Time.deltaTime);
                }
                else if (!scalingUp)
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, startScale, scaleSpeed * Time.deltaTime);
                }

                if(scaleTimer >= scaleAndFloatRate)
                {
                    if (scalingUp) { scalingUp = false; }
                    else if (!scalingUp) { scalingUp = true; }
                    scaleTimer = 0;
                }
            }
        }
	}
}
