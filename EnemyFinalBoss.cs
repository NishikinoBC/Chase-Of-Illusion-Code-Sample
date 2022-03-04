using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFinalBoss : Enemy
{
    private bool isTowardsLeft = true;
    private float selfScaleX;

    public float IdleTime;
    private float IdleTimer;
    public float IdleToWarpTiming;

    public float BltPreTime;
    private float BltPreTimer;

    public float BltAtkTime;
    private float BltAtkTimer;

    public float FireBallPreTime;
    private float FireBallPreTimer;

    public float FireBallAtkTime;
    private float FireBallAtkTimer;

    public float SpearPreTime;
    private float SpearPreTimer;

    public float SpearAtkTime;
    private float SpearAtkTimer;

    public float WarpPreTime;
    private float WarpPreTimer;
      
    public float WarpHideTime;
    private float WarpHideTimer;

    public float WarpShowTime;
    private float WarpShowTimer;

    public float ChangePhaseTime;
    private float ChangePhaseTimer;

    public GameObject player;
    [SerializeField]
    private EnemyState state;
    //[SerializeField]
    public bool isBossTriggered = false;

    //Boss Warp Point
    public Transform[] WarpPointGroup;
    private int warpPointCount;

    public int warpAfterSkillUse = 3;
    public int warpAfterSkillUseP2 = 5;
    [SerializeField]
    private int skillUsed;


    //Boss Data
    public float P1HP = 40f;
    public float P2HP = 40f;

    public int phase = 1;

    [SerializeField]
    private bool isBossInvincible = false;




    //Place holder hint
    public GameObject BulletHint;
    public GameObject FBHint;
    public GameObject SpearHint;
    public GameObject AtkHint;

    //Bullet
    public GameObject bossBullet;
    public GameObject bossFB;
    public GameObject bossSpear;



    //SkillParameter
    public float bulletInterval;
    private float BltRoundTimer;
    private float bulletShootAngle;
    public float bulletBiasAngle;
    public float bulletSpeed;

    public float FBInterval;
    private float FBRoundTimer;
    public float FBSpeed;
    public Transform[] FBGenerator = new Transform[17];
    public Transform[] FBGeneratorL = new Transform[17];
    public Transform[] FBGeneratorR = new Transform[17];

    public float BossAwayTime;
    private float BossAwayTimer;
    public float GateAwayTime;
    private float GateAwayTimer;
    public Transform[] WarpPoints = new Transform[3];
    public float BossAppearTime;
    private float BossAppearTimer;
    private Transform NextWarpPoint;

    private bool isBossAway = false;
    private bool isGateAway = false;

    public Transform[] SpearGenerator = new Transform[12];
    public GameObject SpearGroup;
    public int SpearGenPlat;
    public float SpearRoundTime;
    private float SpearRoundTimer;



    //Other
    //private GameObject AM;
    private GameObject BossParentObj;
    public GameObject BossWarpGate;
    public GameObject FinalSceneGate;
    public GameObject PhaseTwoPlats;

    private AudioSource FireBallSFX;
    private AudioSource FireBallHitSFX;
    private AudioSource SpearSFX;
    private AudioSource WarpSFX;

    private bool isChangingPhase = false;//use it to break current state

    //Animation
    private Animator anim;
    
    /*
     * Animation State Instruction:
     * 0: Idle
     * 1: Warp
     * 2: Warp Reverse
     * 3: Bullet Pre
     * 4: Bullet
     * 5: Fire Ball Pre
     * 6: Fire Ball
     * 7: Spear Pre
     * 8: Spear
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     */



    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        HP = P1HP;
        player = GameObject.Find("Player");
        selfScaleX = transform.localScale.x;
        state = EnemyState.Idle;
        warpPointCount = WarpPointGroup.Length;
        skillUsed = 0;
        bulletShootAngle = 0;

        AM = GameObject.Find("AudioManager");
        if (AM)
        {
            FireBallSFX = AM.GetComponent<AudioManager>().BossFBFallSFX;
            FireBallHitSFX = AM.GetComponent<AudioManager>().BossFBHitSFX;
            SpearSFX = AM.GetComponent<AudioManager>().BossSpearSFX;
            WarpSFX = AM.GetComponent<AudioManager>().BossWarpSFX;
        }


        BossParentObj = GameObject.Find("=====BOSS=====");

        anim = GetComponent<Animator>();
        if (anim)
        {
            Debug.Log("AnimGet" + anim);
        }

        InitAwake();
    }

    // Update is called once per frame
    void Update()
    {
        HitFeedback();


        if (isBossTriggered)
        {
            if (state == EnemyState.BossChangeState)
            {
                ChangePhaseTimer += Time.deltaTime;

                if (ChangePhaseTimer >= ChangePhaseTime)
                {
                    ChangePhaseTimer = 0;
                    transform.position = new Vector3(99999, 99999, 99999);
                }
            }

            if (state == EnemyState.BossChangeStateEnd)
            {
                WarpShowTimer += Time.deltaTime;

                BossAppearTimer += Time.deltaTime;
                if (BossAppearTimer >= BossAppearTime)
                {
                    BossAppearTimer = 0;
                    anim.SetInteger("AnimState", 2);
                    transform.position = WarpPoints[3].position;
                }

                if (WarpShowTimer >= WarpShowTime)
                {
                    WarpShowTimer = 0;
                    //BossWarpGate.SetActive(false);

                    state = EnemyState.Idle;
                }
            }

            if (state == EnemyState.Idle && skillUsed < warpAfterSkillUse)
            {
                IdleTimer += Time.deltaTime;
                if (IdleTimer >= IdleTime && !isChangingPhase)
                {
                    IdleTimer = 0;

                    int nextSkill;//0:Bullet, 1:FireBall, 2:FireSpear

                    if (phase == 1)
                    {
                        nextSkill = Mathf.FloorToInt(Random.Range(0.001f, 1.999f));
                    }
                    else
                    {
                        nextSkill = Mathf.FloorToInt(Random.Range(0.001f, 2.999f));/////////////////////
                    }

                    if (nextSkill == 0)
                    {
                        anim.SetInteger("AnimState", 3);
                        state = EnemyState.BossBulletPre;
                    }
                    else if (nextSkill == 1)
                    {
                        anim.SetInteger("AnimState", 5);
                        state = EnemyState.BossFireBallPre;
                    }
                    else if (nextSkill == 2)
                    {
                        anim.SetInteger("AnimState", 3);
                        state = EnemyState.BossSpearPre;
                    }
                }

                if (isChangingPhase)
                {
                    IdleTimer = 0;
                    anim.SetInteger("AnimState", 1);
                    state = EnemyState.BossChangeState;
                    skillUsed = 0;
                    if (AM)
                    {
                        WarpSFX.Play();
                    }
                }

            }
            
            //Use warp pre time as warp pre + hide, warp as warp show
            if (state == EnemyState.Idle && skillUsed >= warpAfterSkillUse)
            {
                if (!isChangingPhase)
                {
                    skillUsed = 0;
                    //BossWarpGate.SetActive(true);
                    anim.SetInteger("AnimState", 1);
                    state = EnemyState.BossWarpPre;
                    if (AM)
                    {
                        WarpSFX.Play();
                    }
                }

                if (isChangingPhase)
                {
                    skillUsed = 0;
                    anim.SetInteger("AnimState", 1);
                    state = EnemyState.BossChangeState;
                    skillUsed = 0;
                    if (AM)
                    {
                        WarpSFX.Play();
                    }
                }
            }

            if(state == EnemyState.BossWarpPre)
            {
                WarpPreTimer += Time.deltaTime;

                if (!isChangingPhase)
                {
                    if (!isBossAway)
                    {
                        BossAwayTimer += Time.deltaTime;
                    }
                    if (BossAwayTimer >= BossAwayTime)
                    {
                        isBossAway = true;
                        BossAwayTimer = 0;
                        transform.position = new Vector3(99999, 99999, 99999);
                    }

                    if (!isGateAway)
                    {
                        GateAwayTimer += Time.deltaTime;
                    }
                    if (GateAwayTimer >= GateAwayTime)
                    {
                        isGateAway = true;
                        GateAwayTimer = 0;
                        //BossWarpGate.transform.position = new Vector3(99999, 99999, 99999);
                    }

                    if (WarpPreTimer >= WarpPreTime)
                    {
                        WarpPreTimer = 0;

                        int nextWP = phase == 1 ? Mathf.FloorToInt(Random.Range(0.001f, 2.999f)) : Mathf.FloorToInt(Random.Range(3.001f, 8.999f));
                        NextWarpPoint = WarpPoints[nextWP];
                        //BossWarpGate.transform.position = NextWarpPoint.position;

                        isBossAway = false;
                        isGateAway = false;

                        //anim.SetInteger("AnimState", 2);
                        state = EnemyState.BossWarp;
                    }
                }

                if (isChangingPhase)
                {
                    BossAwayTimer = 0;
                    GateAwayTimer = 0;
                    WarpPreTimer = 0;
                    anim.SetInteger("AnimState", 1);
                    state = EnemyState.BossChangeState;
                    skillUsed = 0;
                    if (AM)
                    {
                        WarpSFX.Play();
                    }
                }

            }

            if (state == EnemyState.BossWarp)
            {
                WarpShowTimer += Time.deltaTime;

                /**/
                if (!isChangingPhase)
                {
                    BossAppearTimer += Time.deltaTime;
                    if (BossAppearTimer >= BossAppearTime)
                    {
                        BossAppearTimer = 0;
                        anim.SetInteger("AnimState", 2);
                        transform.position = NextWarpPoint.position;
                    }

                    if (WarpShowTimer >= WarpShowTime)
                    {
                        WarpShowTimer = 0;
                        //BossWarpGate.SetActive(false);

                        state = EnemyState.Idle;
                    }
                }
                
                if (isChangingPhase)
                {
                    WarpShowTimer = 0;
                    BossAppearTimer = 0;
                    anim.SetInteger("AnimState", 1);
                    state = EnemyState.BossChangeState;
                    skillUsed = 0;
                    if (AM)
                    {
                        WarpSFX.Play();
                    }
                }

            }

            //Boss Spear
            if (state == EnemyState.BossSpearPre)
            {
                SpearPreTimer += Time.deltaTime;
                if (SpearPreTimer >= SpearPreTime)
                {
                    SpearPreTimer = 0;
                    anim.SetInteger("AnimState", 4);
                    state = EnemyState.BossSpear;
                }
            }

            if (state == EnemyState.BossSpear)
            {
                SpearAtkTimer += Time.deltaTime;

                SpearRoundTimer += Time.deltaTime;
                if (SpearRoundTimer >= SpearRoundTime)
                {
                    SpearRoundTimer = 0;
                    GameObject Spears = Instantiate(bossSpear, SpearGenerator[SpearGenPlat].transform.position, Quaternion.identity, BossParentObj.transform);
                }



                if (SpearAtkTimer >= SpearAtkTime)
                {
                    SpearAtkTimer = 0;
                    SpearRoundTimer = 0;
                    state = EnemyState.Idle;
                    skillUsed++;
                }
            }


            //Boss Bullet
            if (state == EnemyState.BossBulletPre)
            {
                if (!isChangingPhase)
                {
                    //BulletHint.SetActive(true);

                    BltPreTimer += Time.deltaTime;
                    if (BltPreTimer >= BltPreTime)
                    {
                        BltPreTimer = 0;
                        anim.SetInteger("AnimState", 4);
                        state = EnemyState.BossBullet;
                        //BulletHint.SetActive(false);
                    }
                }

                if (isChangingPhase)
                {
                    //BulletHint.SetActive(false);
                    BltPreTimer = 0;
                    anim.SetInteger("AnimState", 1);
                    state = EnemyState.BossChangeState;
                    skillUsed = 0;
                    if (AM)
                    {
                        WarpSFX.Play();
                    }
                }

            }

            if (state == EnemyState.BossBullet)
            {
                //AtkHint.SetActive(true);

                if (!isChangingPhase)
                {
                    BltAtkTimer += Time.deltaTime;

                    BltRoundTimer += Time.deltaTime;
                    if (BltRoundTimer >= bulletInterval)
                    {
                        BltRoundTimer = 0;

                        GameObject blt01 = Instantiate(bossBullet, transform.position, Quaternion.identity, BossParentObj.transform);
                        GameObject blt02 = Instantiate(bossBullet, transform.position, Quaternion.identity, BossParentObj.transform);
                        GameObject blt03 = Instantiate(bossBullet, transform.position, Quaternion.identity, BossParentObj.transform);
                        GameObject blt04 = Instantiate(bossBullet, transform.position, Quaternion.identity, BossParentObj.transform);
                        GameObject blt05 = Instantiate(bossBullet, transform.position, Quaternion.identity, BossParentObj.transform);
                        GameObject blt06 = Instantiate(bossBullet, transform.position, Quaternion.identity, BossParentObj.transform);
                        GameObject blt07 = Instantiate(bossBullet, transform.position, Quaternion.identity, BossParentObj.transform);
                        GameObject blt08 = Instantiate(bossBullet, transform.position, Quaternion.identity, BossParentObj.transform);

                        float m_fireAngle01 = 0 + bulletShootAngle;
                        float m_fireAngle02 = 45 + bulletShootAngle;
                        float m_fireAngle03 = 90 + bulletShootAngle;
                        float m_fireAngle04 = 135 + bulletShootAngle;
                        float m_fireAngle05 = 180 + bulletShootAngle;
                        float m_fireAngle06 = 225 + bulletShootAngle;
                        float m_fireAngle07 = 270 + bulletShootAngle;
                        float m_fireAngle08 = 315 + bulletShootAngle;

                        blt01.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, bulletShootAngle) * Vector3.down * bulletSpeed;
                        blt02.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, bulletShootAngle) * new Vector3(1, -1, 0).normalized * bulletSpeed;
                        blt03.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, bulletShootAngle) * Vector3.right * bulletSpeed;
                        blt04.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, bulletShootAngle) * new Vector3(1, 1, 0).normalized * bulletSpeed;
                        blt05.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, bulletShootAngle) * Vector3.up * bulletSpeed;
                        blt06.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, bulletShootAngle) * new Vector3(-1, 1, 0).normalized * bulletSpeed;
                        blt07.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, bulletShootAngle) * Vector3.left * bulletSpeed;
                        blt08.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, bulletShootAngle) * new Vector3(-1, -1, 0).normalized * bulletSpeed;

                        blt01.transform.eulerAngles = new Vector3(0, 0, m_fireAngle01);
                        blt02.transform.eulerAngles = new Vector3(0, 0, m_fireAngle02);
                        blt03.transform.eulerAngles = new Vector3(0, 0, m_fireAngle03);
                        blt04.transform.eulerAngles = new Vector3(0, 0, m_fireAngle04);
                        blt05.transform.eulerAngles = new Vector3(0, 0, m_fireAngle05);
                        blt06.transform.eulerAngles = new Vector3(0, 0, m_fireAngle06);
                        blt07.transform.eulerAngles = new Vector3(0, 0, m_fireAngle07);
                        blt08.transform.eulerAngles = new Vector3(0, 0, m_fireAngle08);

                        bulletShootAngle += bulletBiasAngle;
                    }

                    if (BltAtkTimer > BltAtkTime)
                    {
                        bulletShootAngle = Random.Range(0, 90);
                        //AtkHint.SetActive(false);
                        BltAtkTimer = 0;
                        BltRoundTimer = 0;
                        state = EnemyState.Idle;
                        skillUsed++;
                    }
                }

                if (isChangingPhase)
                {
                    BltAtkTimer = 0;
                    BltRoundTimer = 0;
                    bulletShootAngle = Random.Range(0, 90);
                    anim.SetInteger("AnimState", 1);
                    state = EnemyState.BossChangeState;
                    skillUsed = 0;
                    if (AM)
                    {
                        WarpSFX.Play();
                    }
                }
            }

            //Boss Fire Ball
            if (state == EnemyState.BossFireBallPre)
            {
                if (!isChangingPhase)
                {
                    //FBHint.SetActive(true);

                    FireBallPreTimer += Time.deltaTime;
                    if (FireBallPreTimer >= FireBallPreTime)
                    {
                        FireBallPreTimer = 0;
                        anim.SetInteger("AnimState", 6);
                        state = EnemyState.BossFireBall;
                        //FBHint.SetActive(false);
                        if (AM)
                        {
                            FireBallSFX.Play();
                        }
                    }
                }

                if (isChangingPhase)
                {
                    //FBHint.SetActive(false);
                    FireBallPreTimer = 0;
                    anim.SetInteger("AnimState", 1);
                    state = EnemyState.BossChangeState;
                    skillUsed = 0;
                    if (AM)
                    {
                        WarpSFX.Play();
                    }
                }

            }

            if (state == EnemyState.BossFireBall)
            {
                //AtkHint.SetActive(true);

                if (!isChangingPhase)
                {
                    FireBallAtkTimer += Time.deltaTime;

                    FBRoundTimer += Time.deltaTime;

                    if (FBRoundTimer >= FBInterval)
                    {
                        FBRoundTimer = 0;

                        //Do a round of attack
                        int FBPattern = Mathf.FloorToInt(Random.Range(0.001f, 6.999f));
                        int[] FBSlot = new int[9];

                        switch (FBPattern)
                        {
                            case 0:
                                FBSlot[0] = 1;
                                FBSlot[1] = 3;
                                FBSlot[2] = 6;
                                FBSlot[3] = 7;
                                FBSlot[4] = 9;
                                FBSlot[5] = 12;
                                FBSlot[6] = 13;
                                FBSlot[7] = 14;
                                FBSlot[8] = 16;
                                break;
                            case 1:
                                FBSlot[0] = 2;
                                FBSlot[1] = 3;
                                FBSlot[2] = 4;
                                FBSlot[3] = 6;
                                FBSlot[4] = 9;
                                FBSlot[5] = 10;
                                FBSlot[6] = 11;
                                FBSlot[7] = 12;
                                FBSlot[8] = 17;
                                break;
                            case 2:
                                FBSlot[0] = 3;
                                FBSlot[1] = 4;
                                FBSlot[2] = 7;
                                FBSlot[3] = 8;
                                FBSlot[4] = 9;
                                FBSlot[5] = 11;
                                FBSlot[6] = 12;
                                FBSlot[7] = 13;
                                FBSlot[8] = 15;
                                break;
                            case 3:
                                FBSlot[0] = 1;
                                FBSlot[1] = 3;
                                FBSlot[2] = 4;
                                FBSlot[3] = 5;
                                FBSlot[4] = 7;
                                FBSlot[5] = 9;
                                FBSlot[6] = 10;
                                FBSlot[7] = 12;
                                FBSlot[8] = 13;
                                break;
                            case 4:
                                FBSlot[0] = 4;
                                FBSlot[1] = 5;
                                FBSlot[2] = 6;
                                FBSlot[3] = 9;
                                FBSlot[4] = 11;
                                FBSlot[5] = 12;
                                FBSlot[6] = 15;
                                FBSlot[7] = 16;
                                FBSlot[8] = 17;
                                break;
                            case 5:
                                FBSlot[0] = 1;
                                FBSlot[1] = 2;
                                FBSlot[2] = 4;
                                FBSlot[3] = 7;
                                FBSlot[4] = 9;
                                FBSlot[5] = 10;
                                FBSlot[6] = 11;
                                FBSlot[7] = 15;
                                FBSlot[8] = 17;
                                break;
                            case 6:
                                FBSlot[0] = 1;
                                FBSlot[1] = 3;
                                FBSlot[2] = 5;
                                FBSlot[3] = 6;
                                FBSlot[4] = 9;
                                FBSlot[5] = 10;
                                FBSlot[6] = 14;
                                FBSlot[7] = 15;
                                FBSlot[8] = 17;
                                break;
                        }

                        GameObject FB01 = new GameObject();
                        GameObject FB02 = new GameObject();
                        GameObject FB03 = new GameObject();
                        GameObject FB04 = new GameObject();
                        GameObject FB05 = new GameObject();
                        GameObject FB06 = new GameObject();
                        GameObject FB07 = new GameObject();
                        GameObject FB08 = new GameObject();
                        GameObject FB09 = new GameObject();

                        int LorR;

                        if (phase == 1)
                        {
                            LorR = 999;

                            FB01 = Instantiate(bossFB, FBGenerator[FBSlot[0] - 1].position, Quaternion.identity, BossParentObj.transform);
                            FB02 = Instantiate(bossFB, FBGenerator[FBSlot[1] - 1].position, Quaternion.identity, BossParentObj.transform);
                            FB03 = Instantiate(bossFB, FBGenerator[FBSlot[2] - 1].position, Quaternion.identity, BossParentObj.transform);
                            FB04 = Instantiate(bossFB, FBGenerator[FBSlot[3] - 1].position, Quaternion.identity, BossParentObj.transform);
                            FB05 = Instantiate(bossFB, FBGenerator[FBSlot[4] - 1].position, Quaternion.identity, BossParentObj.transform);
                            FB06 = Instantiate(bossFB, FBGenerator[FBSlot[5] - 1].position, Quaternion.identity, BossParentObj.transform);
                            FB07 = Instantiate(bossFB, FBGenerator[FBSlot[6] - 1].position, Quaternion.identity, BossParentObj.transform);
                            FB08 = Instantiate(bossFB, FBGenerator[FBSlot[7] - 1].position, Quaternion.identity, BossParentObj.transform);
                            FB09 = Instantiate(bossFB, FBGenerator[FBSlot[8] - 1].position, Quaternion.identity, BossParentObj.transform);
                        }
                        else
                        {
                            LorR = Mathf.FloorToInt(Random.Range(0.001f, 1.999f));
                            if (LorR == 0)
                            {
                                FB01 = Instantiate(bossFB, FBGeneratorL[FBSlot[0] - 1].position, Quaternion.identity, BossParentObj.transform);
                                //FB02 = Instantiate(bossFB, FBGeneratorL[FBSlot[1] - 1].position, Quaternion.identity, BossParentObj.transform);
                                FB03 = Instantiate(bossFB, FBGeneratorL[FBSlot[2] - 1].position, Quaternion.identity, BossParentObj.transform);
                                //FB04 = Instantiate(bossFB, FBGeneratorL[FBSlot[3] - 1].position, Quaternion.identity, BossParentObj.transform);
                                FB05 = Instantiate(bossFB, FBGeneratorL[FBSlot[4] - 1].position, Quaternion.identity, BossParentObj.transform);
                                //FB06 = Instantiate(bossFB, FBGeneratorL[FBSlot[5] - 1].position, Quaternion.identity, BossParentObj.transform);
                                FB07 = Instantiate(bossFB, FBGeneratorL[FBSlot[6] - 1].position, Quaternion.identity, BossParentObj.transform);
                                //FB08 = Instantiate(bossFB, FBGeneratorL[FBSlot[7] - 1].position, Quaternion.identity, BossParentObj.transform);
                                FB09 = Instantiate(bossFB, FBGeneratorL[FBSlot[8] - 1].position, Quaternion.identity, BossParentObj.transform);
                            }
                            else
                            {
                                FB01 = Instantiate(bossFB, FBGeneratorR[FBSlot[0] - 1].position, Quaternion.identity, BossParentObj.transform);
                                //FB02 = Instantiate(bossFB, FBGeneratorR[FBSlot[1] - 1].position, Quaternion.identity, BossParentObj.transform);
                                FB03 = Instantiate(bossFB, FBGeneratorR[FBSlot[2] - 1].position, Quaternion.identity, BossParentObj.transform);
                                //FB04 = Instantiate(bossFB, FBGeneratorR[FBSlot[3] - 1].position, Quaternion.identity, BossParentObj.transform);
                                FB05 = Instantiate(bossFB, FBGeneratorR[FBSlot[4] - 1].position, Quaternion.identity, BossParentObj.transform);
                                //FB06 = Instantiate(bossFB, FBGeneratorR[FBSlot[5] - 1].position, Quaternion.identity, BossParentObj.transform);
                                FB07 = Instantiate(bossFB, FBGeneratorR[FBSlot[6] - 1].position, Quaternion.identity, BossParentObj.transform);
                                //FB08 = Instantiate(bossFB, FBGeneratorR[FBSlot[7] - 1].position, Quaternion.identity, BossParentObj.transform);
                                FB09 = Instantiate(bossFB, FBGeneratorR[FBSlot[8] - 1].position, Quaternion.identity, BossParentObj.transform);
                            }
                        }

                        if (phase == 1)
                        {
                            FB01.GetComponent<Rigidbody2D>().velocity = Vector2.down * FBSpeed;
                            FB02.GetComponent<Rigidbody2D>().velocity = Vector2.down * FBSpeed;
                            FB03.GetComponent<Rigidbody2D>().velocity = Vector2.down * FBSpeed;
                            FB04.GetComponent<Rigidbody2D>().velocity = Vector2.down * FBSpeed;
                            FB05.GetComponent<Rigidbody2D>().velocity = Vector2.down * FBSpeed;
                            FB06.GetComponent<Rigidbody2D>().velocity = Vector2.down * FBSpeed;
                            FB07.GetComponent<Rigidbody2D>().velocity = Vector2.down * FBSpeed;
                            FB08.GetComponent<Rigidbody2D>().velocity = Vector2.down * FBSpeed;
                            FB09.GetComponent<Rigidbody2D>().velocity = Vector2.down * FBSpeed;
                        }
                        else
                        {
                            if (LorR == 0)
                            {
                                FB01.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, 28) * Vector2.down * FBSpeed;
                                //FB02.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, 28) * Vector2.down * FBSpeed;
                                FB03.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, 28) * Vector2.down * FBSpeed;
                                //FB04.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, 28) * Vector2.down * FBSpeed;
                                FB05.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, 28) * Vector2.down * FBSpeed;
                                //FB06.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, 28) * Vector2.down * FBSpeed;
                                FB07.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, 28) * Vector2.down * FBSpeed;
                                //FB08.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, 28) * Vector2.down * FBSpeed;
                                FB09.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, 28) * Vector2.down * FBSpeed;
                                FB01.transform.eulerAngles = new Vector3(0, 0, 28);
                                //FB02.transform.eulerAngles = new Vector3(0, 0, 28);
                                FB03.transform.eulerAngles = new Vector3(0, 0, 28);
                                //FB04.transform.eulerAngles = new Vector3(0, 0, 28);
                                FB05.transform.eulerAngles = new Vector3(0, 0, 28);
                                //FB06.transform.eulerAngles = new Vector3(0, 0, 28);
                                FB07.transform.eulerAngles = new Vector3(0, 0, 28);
                                //FB08.transform.eulerAngles = new Vector3(0, 0, 28);
                                FB09.transform.eulerAngles = new Vector3(0, 0, 28);
                            }
                            else
                            {
                                FB01.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, -28) * Vector2.down * FBSpeed;
                                //FB02.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, -28) * Vector2.down * FBSpeed;
                                FB03.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, -28) * Vector2.down * FBSpeed;
                                //FB04.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, -28) * Vector2.down * FBSpeed;
                                FB05.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, -28) * Vector2.down * FBSpeed;
                                //FB06.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, -28) * Vector2.down * FBSpeed;
                                FB07.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, -28) * Vector2.down * FBSpeed;
                                //FB08.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, -28) * Vector2.down * FBSpeed;
                                FB09.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, -28) * Vector2.down * FBSpeed;
                                FB01.transform.eulerAngles = new Vector3(0, 0, -28);
                                //FB02.transform.eulerAngles = new Vector3(0, 0, -28);
                                FB03.transform.eulerAngles = new Vector3(0, 0, -28);
                                //FB04.transform.eulerAngles = new Vector3(0, 0, -28);
                                FB05.transform.eulerAngles = new Vector3(0, 0, -28);
                                //FB06.transform.eulerAngles = new Vector3(0, 0, -28);
                                FB07.transform.eulerAngles = new Vector3(0, 0, -28);
                                //FB08.transform.eulerAngles = new Vector3(0, 0, -28);
                                FB09.transform.eulerAngles = new Vector3(0, 0, -28);
                            }
                        }
                        
                    }

                    if (FireBallAtkTimer >= FireBallAtkTime)
                    {
                        if (AM)
                        {
                            FireBallSFX.Stop();
                        }
                        //AtkHint.SetActive(false);
                        FireBallAtkTimer = 0;
                        FBRoundTimer = 0;
                        anim.SetInteger("AnimState", 0);
                        state = EnemyState.Idle;
                        skillUsed++;
                    }
                }

                if (isChangingPhase)
                {
                    FireBallAtkTimer = 0;
                    FBRoundTimer = 0;
                    if (AM)
                    {
                        FireBallSFX.Stop();
                    }
                    anim.SetInteger("AnimState", 1);
                    state = EnemyState.BossChangeState;
                    skillUsed = 0;
                    if (AM)
                    {
                        WarpSFX.Play();
                    }
                }
            }

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet") && isBossTriggered)
        {
            Hit(collision.GetComponent<PlayerBullet>().damage);
            if (HP <= 0 && phase == 1)
            {
                ChangePhase();



                //To P2 here
            }
            else if (HP <= 0 && phase == 2)
            {
                Die();
            }
        }
    }




    public void Die()
    {
        FinalSceneGate.SetActive(true);

        //BossWarpGate.SetActive(false);
        gameObject.SetActive(false);

        if (AM)
        {
            WarpSFX.Stop();
            FireBallSFX.Stop();
            SpearSFX.Stop();
            FireBallHitSFX.Stop();
        }
    }

    public void TriggerBoss()
    {
        isBossTriggered = true;
    }

    public void ChangePhase()
    {
        Invoke("ActivePlats", 2f);
        phase = 2;
        HP = P2HP;
        isChangingPhase = true;
        WarpShowTimer = 0;
        BossAppearTimer = 0;
        skillUsed = 0;
        warpAfterSkillUse = warpAfterSkillUseP2;
        if (AM)
        {
            object val = 5;
            AM.SendMessage("FadeNowSetNext", val);
        }
    }

    public void ActivePlats()
    {
        PhaseTwoPlats.SetActive(true);
    }

    public void ActivatePhaseTwo()
    {
        state = EnemyState.BossChangeStateEnd;
        isChangingPhase = false;
        if (AM)
        {
            AM.SendMessage("PlayNext");
        }
    }

    public void SetSpearGenPlat(int genPlatNo)
    {
        SpearGenPlat = genPlatNo;
        Debug.Log("Boss side plat set" + SpearGenPlat);
    }

}
