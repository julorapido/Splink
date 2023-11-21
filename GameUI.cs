using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;


public class GameUI : MonoBehaviour
{
    [Header ("Player Stats")]
    private int max_health = 200;
    private int player_health = 200;
    private int set_health
    {
        get { return 0; }
        set{ if(value > max_health) player_health = max_health; else value = max_health;}
    }

    [Header ("UI Objects")]
    private Transform[] parents_3d;

    [Header ("Scripts")]
    private Weapon weapon_scrpt;
    
    [Header ("Player")]
    private Transform plyr_transform;
    private Rigidbody plyr_rgBody;

    [Header ("TopRight Gun")]
    [SerializeField] private GameObject gun_ui;
    [SerializeField] private TextMeshPro ammo_txt;
    [SerializeField] private GameObject gun_prefab;
    private Vector3 saved_prefabPos_, saved_prefabScale_;
    private Quaternion saved_prefabRotation_;

    [Header ("TopRight GunMaterials")]
    [SerializeField] private Material[] Gun_Materials;
    private GameObject[] gun_fetchedPrefabs;


    [Header ("Enemy Ui")]
    [SerializeField] private GameObject enemy_information;
    private Transform aimed_enemy;
    private AutoTurret aimed_turretScrpt;
    private int enemy_health;

    [Header ("Score")]
    [SerializeField] private TextMeshProUGUI score_txt;
    private bool countScore_ = false;
    private uint lastZ = 0;
    private uint score = 0;
    private decimal v_score = 0;


    // Start is called before the first frame update
    private void Start()
    {
        weapon_scrpt = FindObjectOfType<Weapon>();

        PlayerMovement pm = FindObjectOfType<PlayerMovement>();
        plyr_transform = pm.transform;
        plyr_rgBody  = pm.transform.GetComponent<Rigidbody>();

        gun_fetchedPrefabs = weapon_scrpt.get_weaponsResourcesPrefab_buffer;

        int l_prefb = gun_ui.transform.childCount - 1;
        saved_prefabPos_ = gun_ui.transform.GetChild(l_prefb).GetChild(0).transform.position;
        saved_prefabScale_ = gun_ui.transform.GetChild(l_prefb).GetChild(0).transform.localScale;
        saved_prefabRotation_ = gun_ui.transform.GetChild(l_prefb).GetChild(0).transform.localRotation;

        List<Transform> ts = new List<Transform>(10); // 10-size buffer
        int c = 0;
        for(int i = 0; i < 2; i++) // TOP-lft & TOP-rght
        {
            for(int j = 0 ; j < transform.GetChild(i).transform.childCount; j++)
            {
                Transform f =  transform.GetChild(i).transform.GetChild(j);
                // var temp_ts = ts.ToList(); temp_ts.Add(f);
                // ts = temp_ts.ToArray();
                ts.Add(f);
                c++;
             }
        }
        // ts[c + 1] = enemy_information.transform;
        parents_3d = ts.ToArray();
        
        newEnemy_UI(true);
        Invoke("countScore", 0.25f);
    }
    private void countScore()
    { countScore_ = true; }


    // FixedUpdate for responsive gun rotations
    private void FixedUpdate()
    {
        if( ammo_txt.text.ToString() != weapon_scrpt.get_ammo.ToString())
        {
            ammo_txt.text = weapon_scrpt.get_ammo.ToString();
        }

        if(plyr_rgBody != null && plyr_transform != null)
        {
            for(int i = 0; i < parents_3d.Length; i++)
            {
                if(parents_3d[i] == null
                    || LeanTween.isTweening ( parents_3d[i].gameObject)
                ) continue;

                parents_3d[i].transform.rotation = Quaternion.Slerp( parents_3d[i].transform.rotation, Quaternion.Euler(
                    plyr_rgBody.velocity.y * 0.25f,
                    (plyr_transform.rotation.eulerAngles.y > 180f ? 
                        (-1 * (plyr_transform.rotation.eulerAngles.y - 360f) ):
                        -1 * plyr_transform.rotation.eulerAngles.y
                    ) * 0.35f,
                // (plyr_transform.rotation.eulerAngles.y > 270f ? plyr_transform.rotation.eulerAngles.y : plyr_transform.rotation.eulerAngles.y - 360f) / 10,
                    0
                ), 0.2f);

                // parents_3d[i].transform.localPosition = Vector3.Lerp( parents_3d[i].transform.localPosition, new Vector3(
                //     parents_3d[i].transform.localPosition.x + (plyr_rgBody.velocity.x* 0.25f),
                //     parents_3d[i].transform.localPosition.y + (plyr_rgBody.velocity.y* 0.25f),
                //     0
                // ), 0.2f);
            }

            
        }

 
     
        // score
        if(v_score == 0 && countScore_)
        { v_score = (uint) plyr_transform.position.z; }

        if( ( ((decimal) plyr_transform.position.z - v_score) > ((decimal) 0.10f) ) && (v_score > 0) )
        { 
            v_score+= (decimal) 0.1f;
            score += (( v_score % 2) == 0 ? (uint)4 : (uint)3);
        }
        score_txt.text = score.ToString();


        // aimed enemy
        if(aimed_turretScrpt != null)
        {
            if(enemy_health != aimed_turretScrpt.get_health)
            {
                if(aimed_turretScrpt.get_health == 0)
                {
                    newEnemy_UI(true);
                }else
                {
                    float dmg_done = enemy_health - aimed_turretScrpt.get_health;
                    enemy_health = aimed_turretScrpt.get_health;
                    float x_health = (((float)enemy_health / (float)aimed_turretScrpt.turret_maxHealth) * 100f);
                    LeanTween.scale(enemy_information.transform.GetChild(1).GetChild(1).gameObject, new Vector3(x_health / 100, 1, 1), 0.25f).setEaseInOutCubic();
                    LeanTween.scale(enemy_information.transform.GetChild(1).GetChild(2).gameObject, 
                        new Vector3(x_health / 100, 1, 1), 
                        0.2f + (dmg_done / 70)
                    ).setEaseInOutCubic();
                    // enemy_information.transform.GetChild(1).GetChild(1).localScale = new Vector3(x_health / 100, 1, 1);
                }

            }
        }
    }



    // public Gun Lvl up method
    public void Gun_levelUp(int level)
    {
        if(gun_ui.transform.GetChild(gun_ui.transform.childCount - 1).GetChild(0).gameObject != null)
            Destroy((gun_ui.transform.GetChild(gun_ui.transform.childCount - 1)).GetChild(0).gameObject);
        
        GameObject ui_gunz = Instantiate(gun_fetchedPrefabs[level - 1], saved_prefabPos_, saved_prefabRotation_, gun_ui.transform.GetChild(gun_ui.transform.childCount - 1));
        ui_gunz.SetActive(false);

        Transform[] ui_t = ui_gunz.GetComponentsInChildren<Transform>();
        for(int j = 0; j < ui_t.Length; j ++)
        { ui_t[j].gameObject.layer = 5;}  // => UI LAYER

        ui_gunz.transform.localScale = saved_prefabScale_;
        ui_gunz.SetActive(true);

        // LeanTween.rotateY(ui_gunz, -10f, 1.5f).setEasePunch();

        if(level == 1)
        {
            LeanTween.moveLocal(gun_ui, new Vector3(0, 0, gun_ui.transform.position.z), 0.7f).setEaseInOutCubic();
            LeanTween.scale(gun_ui, gun_ui.transform.localScale * 1.4f, 1.4f).setEasePunch();
            LeanTween.rotateZ(gun_ui, 20f, 1.8f).setEasePunch();
        }else
        {
            LeanTween.scale(gun_ui, gun_ui.transform.localScale * 1.35f, 1.2f).setEasePunch();
            LeanTween.rotateZ(gun_ui, 40f, 1f).setEasePunch();
            Transform[] rects_ = (gun_ui.transform.GetChild(0).gameObject).GetComponentsInChildren<Transform>();

            for(int i = 0; i < rects_.Length; i++)
            {
                if(rects_[i].gameObject == gun_ui.transform.GetChild(0).gameObject) continue;
                MeshRenderer mr  = rects_[i].gameObject.GetComponent<MeshRenderer>();

                if( (i % 2) != 0)
                {  mr.sharedMaterial = Gun_Materials[level < 2 ? level - 1 : (2 * (level -1 ))]; } 
                else { mr.sharedMaterial = Gun_Materials[level < 2 ? level : (2 * (level -1 )) + 1];}
            }
        }

    }

    // public enemy
    public void newEnemy_UI(bool is_empty, Transform enemy_ = null)
    {
        if(is_empty)
        {
            // Invoke("enemyUi_off", 0.6f);
            aimed_turretScrpt = null;
            enemy_information.SetActive(false);
        }else
        {
            if(enemy_ == null) return;

            if(aimed_enemy != null && aimed_enemy != enemy_ )
            {
                Vector3 p = plyr_transform.position - enemy_.position;

                enemy_information.transform.localPosition = new Vector3(p.x * 50, p.y * 30, 0);
                enemy_information.transform.localScale = new Vector3(enemy_information.transform.localScale.x / 10, enemy_information.transform.localScale.y / 10, 1);

                LeanTween.moveLocal(enemy_information, new Vector3(0, 0, 0), 0.7f).setEaseInOutCubic();
                LeanTween.scale(enemy_information, new Vector3(1, 1, 1), 1f).setEaseInOutCubic();
            }else
            {
                enemy_information.transform.localPosition = new Vector3(0, 0, 0);
            }

            aimed_enemy = enemy_;

            enemy_information.SetActive(true);
            AutoTurret at_turret = (enemy_ as Transform).GetComponent<AutoTurret>();
            aimed_turretScrpt = at_turret;
            enemy_information.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = at_turret.turret_name;
            enemy_information.transform.GetChild(3).GetComponent<TMPro.TextMeshProUGUI>().text = at_turret.turret_level.ToString();

            enemy_health = at_turret.get_health;
            float x_health = (((float)enemy_health / (float)aimed_turretScrpt.turret_maxHealth) * 100f);
            enemy_information.transform.GetChild(1).GetChild(1).localScale = new Vector3(x_health / 100, 1, 1);
            enemy_information.transform.GetChild(1).GetChild(2).localScale = new Vector3(x_health / 100, 1, 1);

        }
    }
    private void enemyUi_off()
    {
        if(aimed_enemy != null) enemy_information.SetActive(false);
    }

}
