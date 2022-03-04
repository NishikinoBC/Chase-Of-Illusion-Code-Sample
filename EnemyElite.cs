using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class EnemyElite : Enemy
{
    private bool isTowardsLeft = true;
    private float selfScaleX;

    public float IdleTime;
    private float IdleTimer;

    public float JumpPreTime;
    private float JumpPreTimer;

    public float JumpAtkTime;
    private float JumpAtkTimer;

    public float DashPreTime;
    private float DashPreTimer;

    public float DashAtkTime;
    private float DashAtkTimer;

    [SerializeField]
    private float g;
    private float targetX;
    private float selfX;
    private float selfY;

    private float initY;

    public float MaxJumpHeight;

    [SerializeField]
    private bool dashLock;

    public float dashDistance;


    public GameObject ItemParent;
    public GameObject DJAmulet;
    private GameObject player;

    public Transform boundL;
    public Transform boundR;

    private Rigidbody2D elite_rb;

    [SerializeField]
    private EnemyState state;


    //Temp art hint
    public GameObject JumpPreHint;
    public GameObject DashPreHint;
    public GameObject AttackAngerHint;
    public PolygonCollider2D n_Confiner;
    public GameObject vcam;

    private bool EliteTriggered = false;

    //private GameObject AM;
    private int playNum;

    private AudioSource EliteJumpSFX;
    private AudioSource EliteDashSFX;
    private AudioSource EliteLandSFX;

    public float djBias = 1f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        player = GameObject.Find("Player");
        selfScaleX = transform.localScale.x;
        state = EnemyState.Idle;
        elite_rb = GetComponent<Rigidbody2D>();
        dashLock = false;

        initY = transform.position.y;

        AM = GameObject.Find("AudioManager");
        playNum = 1;

        InitAwake();
        if (AM)
        {
            EliteJumpSFX = AM.GetComponent<AudioManager>().EliteJumpSFX;
            EliteDashSFX = AM.GetComponent<AudioManager>().EliteDashSFX;
            EliteLandSFX = AM.GetComponent<AudioManager>().EliteLandSFX;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HitFeedback();

    }

    private void FixedUpdate()
    {
        if (EliteTriggered)
        {
            //Facing
            if (state == EnemyState.Idle)
            {
                float player_dir = player.transform.position.x - transform.position.x;
                if (player_dir <= 0)
                {
                    isTowardsLeft = true;
                }
                else
                {
                    isTowardsLeft = false;
                }
            }

            if (isTowardsLeft)
            {
                transform.localScale = new Vector3(selfScaleX, transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-selfScaleX, transform.localScale.y, transform.localScale.z);
            }


            //Idle
            if (state == EnemyState.Idle)
            {
                IdleTimer += Time.fixedDeltaTime;
                if (IdleTimer >= IdleTime)
                {
                    IdleTimer = 0;

                    int nextSkill;//0:Jump, 1:Dash
                    nextSkill = Mathf.FloorToInt(Random.Range(0.1f, 1.9f));

                    Debug.Log(nextSkill);

                    if (nextSkill == 0)
                    {
                        targetX = player.transform.position.x;
                        selfX = transform.position.x;
                        selfY = transform.position.y;

                        state = EnemyState.ElitePreAtkJump;
                    }
                    else if (nextSkill == 1)
                    {
                        targetX = player.transform.position.x;
                        selfX = transform.position.x;
                        selfY = transform.position.y;

                        state = EnemyState.ElitePreAtkDash;
                    }

                }
            }

            //Jump attack pre
            if (state == EnemyState.ElitePreAtkJump)
            {
                JumpPreHint.SetActive(true);

                JumpPreTimer += Time.fixedDeltaTime;
                if (JumpPreTimer >= JumpPreTime)
                {
                    JumpPreTimer = 0;

                    JumpPreHint.SetActive(false);
                    AttackAngerHint.SetActive(true);

                    state = EnemyState.EliteAtkJump;
                    if (AM)
                    {
                        EliteJumpSFX.Play();
                    }
                }

            }

            //Jump attack
            if (state == EnemyState.EliteAtkJump)
            {
                JumpAtkTimer += Time.fixedDeltaTime;

                float px;
                float py;

                px = selfX + (targetX - selfX) * JumpAtkTimer / JumpAtkTime;
                py = selfY + (4 * MaxJumpHeight * JumpAtkTimer) * (JumpAtkTime - JumpAtkTimer) / (JumpAtkTime * JumpAtkTime);

                transform.position = py > selfY ? new Vector3(px, py) : new Vector3(targetX, selfY);



                if (JumpAtkTimer >= JumpAtkTime)
                {
                    JumpAtkTimer = 0;
                    AttackAngerHint.SetActive(false);

                    elite_rb.velocity = new Vector2(0, 0);

                    state = EnemyState.Idle;

                    if (AM)
                    {
                        EliteLandSFX.Play();
                    }
                }
            }

            //Dash attack pre
            if (state == EnemyState.ElitePreAtkDash)
            {
                DashPreHint.SetActive(true);

                DashPreTimer += Time.fixedDeltaTime;
                if (DashPreTimer >= DashPreTime)
                {
                    DashPreTimer = 0;

                    DashPreHint.SetActive(false);
                    AttackAngerHint.SetActive(true);

                    targetX = player.transform.position.x;
                    selfX = transform.position.x;
                    selfY = transform.position.y;

                    dashLock = false;

                    state = EnemyState.EliteAtkDash;
                    if (AM)
                    {
                        EliteDashSFX.Play();
                    }
                }

            }

            //Dash attack
            if (state == EnemyState.EliteAtkDash)
            {
                DashAtkTimer += Time.fixedDeltaTime;

                float px = selfX > targetX ? selfX + -dashDistance * DashAtkTimer / DashAtkTime : selfX + dashDistance * DashAtkTimer / DashAtkTime;
                if (px >= boundL.position.x && px <= boundR.position.x && !dashLock)
                {
                    elite_rb.MovePosition(new Vector2(px, transform.position.y));
                }





                if (DashAtkTimer >= DashAtkTime)
                {
                    DashAtkTimer = 0;

                    AttackAngerHint.SetActive(false);

                    elite_rb.velocity = new Vector2(0, 0);

                    dashLock = false;

                    state = EnemyState.Idle;
                }
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet") && EliteTriggered)
        {
            Hit(collision.GetComponent<PlayerBullet>().damage);
            if (HP <= 0)
            {
                Die();
            }
        }

        if (collision.CompareTag("Player"))
        {
            dashLock = true;
        }


    }


    public void Die()
    {
        Debug.Log("Die:" + gameObject.name);
        //DJAmulet.SetActive(true);

        Instantiate(DJAmulet, new Vector3(transform.position.x, initY + djBias, 1f), Quaternion.identity, ItemParent.transform);

        vcam.GetComponent<CinemachineConfiner>().m_BoundingShape2D = n_Confiner;

        object val = playNum;
        if (AM)
        {
            AM.SendMessage("FadeCutTo", val);
        }

        gameObject.SetActive(false);
    }

    public void TriggerElite()
    {
        EliteTriggered = true;
    }

}
