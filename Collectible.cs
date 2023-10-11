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

    [Header ("Scale Settings")]
    [SerializeField] private Vector3 scaleTowards;
    private bool scalingUp = true;
    private float scaleTimer;
    private Vector3 startScale;

    private int interpolationFramesCount = 60; // Number of frames to completely interpolate between the 2 positions
    private int elapsedFrames = 0;
    private int elapsedFrames2 = 0;


	private void Start ()
    {
        // init start scale 
        startScale = gameObject.transform.localScale;
        StartCoroutine(infiniteScale());
	}
	
	// Update is called once per frame
	private void Update ()
    {       
        
        if(isAnimated)
        {
            if(isRotating)
            {
                transform.Rotate(rotationAngle * rotationSpeed * Time.deltaTime, Space.World    );
            }


            if(isFloating)
            {
                floatTimer += Time.deltaTime;
                //Vector3 moveDir = new Vector3(0.0f, floatSpeed / 130f, 0.0f);
                transform.Translate( (goingUp ? Vector3.up : Vector3.down) * (Time.fixedDeltaTime / 5), Space.World);

                if (goingUp && floatTimer >= scaleAndFloatRate)
                {
                    goingUp = false;
                    floatTimer = 0;
                    // floatSpeed = -floatSpeed;
                }

                else if(!goingUp && floatTimer >= scaleAndFloatRate)
                {
                    goingUp = true;
                    floatTimer = 0;
                    // floatSpeed = +floatSpeed;
                }
            }


            if(isScaling && (false == true) && !isScaling)
            {
                scaleTimer += Time.deltaTime;
                float interpolationRatio = (float)(scalingUp ? elapsedFrames2 : elapsedFrames) / interpolationFramesCount;

                if (scalingUp)
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, scaleTowards, interpolationRatio);
                    // Vector3.SmoothDamp( transform.localScale, scaleTowards, ref currentVelocity, ( Time.deltaTime * scaleSpeed )); 

                    if(transform.localScale == scaleTowards) scalingUp = false;
                }
                else if (!scalingUp)
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, startScale, interpolationRatio);
                    // Vector3.SmoothDamp( transform.localScale, startScale, ref currentVelocity, ( Time.deltaTime * scaleSpeed )); 

                    if(transform.localScale == startScale) scalingUp = true;
                }

                // if(scaleTimer >= scaleAndFloatRate)
                // {
                //     if (scalingUp) { scalingUp = false; }
                //     else if (!scalingUp) { scalingUp = true; }
                //     scaleTimer = 0;
                // }

                elapsedFrames2 = (elapsedFrames2 + 1) % (interpolationFramesCount + 1);  // reset elapsedFrames to zero after it reached (interpolationFramesCount + 1)
                elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1);  // reset elapsedFrames to zero after it reached (interpolationFramesCount + 1)
            }


        }
	}

    private IEnumerator infiniteScale()
    {
        while(true)
        {
            LeanTween.scale(gameObject, scaleTowards, 1f);
            yield return new WaitForSeconds(1f);
            LeanTween.scale(gameObject, startScale, 1f);
            yield return new WaitForSeconds(1f);
        }
    }
}
