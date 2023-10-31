using UnityEngine;
using System.Collections;
using TMPro;
using System;
using System.Linq;
using System.Collections.Generic;

public class Collectible : MonoBehaviour {

    [HideInInspector] public bool isAnimated = true;
    private const float scaleAndFloatRate = 1f;

    [Header ("Settings")]
    [SerializeField] private bool isRotating = false;
    [SerializeField] private bool isFloating = false;
    [SerializeField] private bool isScaling = false;

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

    [Header ("Collectible Weapon")]
    [SerializeField] private bool is_weapon;
    
    [Header ("Weapon Objects")]
    private  Weapon.GunLevel last_weapon_EnumValue;

    [SerializeField] private List<Material> weapon_rarityMats;
    private Weapon weapon_scrpt;
    private TextMeshPro[] weapon_lvls = new TextMeshPro[]{null, null};
    private GameObject gun_prefab;
    private Transform[] rectangles_;
    private Transform[] stars_;
    private Vector3[] prefabs_positionsAndScales = new Vector3[4]{Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero};
    private Quaternion[] prefabs_rotations = new Quaternion[2]{Quaternion.identity, Quaternion.identity};
    private Transform chld_nOne;

    
    private int weapon_levelValue = 0;

    [Header ("Weapon Resources")]
    private GameObject[] weaponsPrefabs;


    private void Awake ()
    {

    }
    
	private void Start ()
    {
        // init start scale 
        startScale = gameObject.transform.localScale;
        if(isScaling)
        {
            StartCoroutine(infiniteScale());
        }
        
        if(is_weapon)
        {
            // weapon_scrpt = FindObjectOfType<Weapon>();
            // weaponsPrefabs = weapon_scrpt.get_weaponsResourcesPrefab_buffer;
            
            // last_weapon_EnumValue = weapon_scrpt.get_GunLvl;
        
            // TextMeshPro[] txts = GetComponentsInChildren<TextMeshPro>();
            // weapon_lvls[0] = txts[txts.Length - 1]; weapon_lvls[1] = txts[txts.Length - 2];

            // rectangles_ = transform.GetChild(0).gameObject.GetComponentsInChildren<Transform>();

            // stars_ = transform.GetChild(transform.childCount - 1).gameObject.GetComponentsInChildren<Transform>();

            // prefabs_positionsAndScales[0] = (transform.GetChild(1)).GetChild(0).localPosition; prefabs_positionsAndScales[1] = (transform.GetChild(1)).GetChild(1).localPosition;
            // prefabs_rotations[0] = (transform.GetChild(1)).GetChild(0).localRotation; prefabs_rotations[1] = (transform.GetChild(1)).GetChild(1).localRotation;
            // prefabs_positionsAndScales[2] = (transform.GetChild(1)).GetChild(0).localScale; prefabs_positionsAndScales[3] = (transform.GetChild(1)).GetChild(1).localScale;

            // chld_nOne = transform.GetChild(1);

            // UpgradeGunCollectible();
        }
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
                transform.Translate( (goingUp ? Vector3.up : Vector3.down) * (Time.fixedDeltaTime / 4), Space.World);

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


    private void FixedUpdate()
    {
        if(is_weapon && weapon_scrpt == null)
        {
            weapon_scrpt = FindObjectOfType<Weapon>();
            if(weapon_scrpt == null) return;
            weaponsPrefabs = weapon_scrpt.get_weaponsResourcesPrefab_buffer;
            
            last_weapon_EnumValue = weapon_scrpt.get_GunLvl;
        
            TextMeshPro[] txts = GetComponentsInChildren<TextMeshPro>();
            weapon_lvls[0] = txts[txts.Length - 1]; weapon_lvls[1] = txts[txts.Length - 2];

            rectangles_ = transform.GetChild(0).gameObject.GetComponentsInChildren<Transform>();

            stars_ = transform.GetChild(transform.childCount - 1).gameObject.GetComponentsInChildren<Transform>();

            prefabs_positionsAndScales[0] = (transform.GetChild(1)).GetChild(0).localPosition; prefabs_positionsAndScales[1] = (transform.GetChild(1)).GetChild(1).localPosition;
            prefabs_rotations[0] = (transform.GetChild(1)).GetChild(0).localRotation; prefabs_rotations[1] = (transform.GetChild(1)).GetChild(1).localRotation;
            prefabs_positionsAndScales[2] = (transform.GetChild(1)).GetChild(0).localScale; prefabs_positionsAndScales[3] = (transform.GetChild(1)).GetChild(1).localScale;

            chld_nOne = transform.GetChild(1);

            UpgradeGunCollectible();
        }


        if(is_weapon && weapon_scrpt != null)
        {
            if(last_weapon_EnumValue != null)
            {
                if(last_weapon_EnumValue != null && weapon_scrpt.get_GunLvl != null)
                {
                    if(last_weapon_EnumValue != weapon_scrpt.get_GunLvl)
                    {
                        UpgradeGunCollectible();
                        last_weapon_EnumValue = weapon_scrpt.get_GunLvl;
                    }
                }
            }
        }


    }


    // private method to get enum position number
    private int GetEnumPosition<T>(T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src);
        return j;            
    }

    private void UpgradeGunCollectible()
    {
        
        int gun_lvl = GetEnumPosition(weapon_scrpt.get_GunLvl);
        weapon_levelValue = gun_lvl;

        //destroy lastPrefabs
        if( (transform.GetChild(1)).GetChild(0).gameObject != null)
            Destroy( ((transform.GetChild(1)).GetChild(0)).gameObject);

        if( (transform.GetChild(1)).GetChild(1).gameObject != null)
            Destroy( ((transform.GetChild(1)).GetChild(1)).gameObject);


        //update prefabs
        GameObject w1 = Instantiate(weaponsPrefabs[gun_lvl], 
            prefabs_positionsAndScales[0], prefabs_rotations[0],
            chld_nOne
        );
        GameObject w2 = Instantiate(weaponsPrefabs[gun_lvl], 
            prefabs_positionsAndScales[1], prefabs_rotations[1],
            chld_nOne
        );
        w1.transform.localPosition = prefabs_positionsAndScales[0]; w2.transform.localPosition = prefabs_positionsAndScales[1];
        w1.transform.localRotation = prefabs_rotations[0]; w2.transform.localRotation = prefabs_rotations[1];
        w1.transform.localScale = prefabs_positionsAndScales[2]; w2.transform.localScale = prefabs_positionsAndScales[3];


        weapon_lvls[0].text = weapon_lvls[1].text = (gun_lvl + 1 ).ToString();
        for(int i = 0; i < rectangles_.Length; i ++)
        {
            if(rectangles_[i] == transform.GetChild(0) ) continue;
            
            MeshRenderer mr  = rectangles_[i].gameObject.GetComponent<MeshRenderer>();
            Material m = mr.sharedMaterial;

            // var mats =  from mat in weapon_rarityMats;
            IEnumerable<UnityEngine.Material> matz =  weapon_rarityMats
            .Where((mat) => mat == m);

            List<int> mat_indx_ =  (weapon_rarityMats
            .Where((mat) => (mat == m))
            ).Select((x, index) => index).ToList();

            int mat_indx_i = 0;
            for(int x = 0; x < weapon_rarityMats.Count; x ++)
            {
                if(weapon_rarityMats[x] == m)
                    mat_indx_i = x;
            }

            mr.material = ( weapon_rarityMats[(gun_lvl > 0 ? 4 : 0) + mat_indx_i ] );

        }
        for (int j = 1; j < 7; j++)
        {
            if(stars_[j] == transform.GetChild(transform.childCount - 1) ) continue;

            if(j <= gun_lvl + 1)
            {
                stars_[j].gameObject.SetActive(true);
                stars_[6 + j].gameObject.SetActive(true);
            }else
            {
                stars_[j].gameObject.SetActive(false);
                stars_[6 + j].gameObject.SetActive(false);
            }
      
        }

        //disable last
        if(gun_lvl > 0) (transform.GetChild(2)).GetChild(gun_lvl - 1).gameObject.SetActive(false);

        // enable new ps system
        (transform.GetChild(2)).GetChild(gun_lvl).gameObject.SetActive(true);
        ParticleSystem[] ps = (transform.GetChild(2)).GetChild(gun_lvl).gameObject.GetComponentsInChildren<ParticleSystem>();
        for(int p = 0; p < ps.Length; p ++){ ps[p].Play();}
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
 