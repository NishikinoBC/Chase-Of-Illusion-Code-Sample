using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;

public class Player : MonoBehaviour
{
    Rigidbody2D rb_player;

    //Ground check
    public Transform GroundCheck;
    public Transform GroundCheckSecondary;
    public float groundCheckDistance = 0.02f;
    public LayerMask groundMask;
    public bool isGrounded;

    //Player sprite
    public GameObject sprite_player;

    //Player orientation (facing at)
    private bool isTowardsRight = true;

    //Fire
    private float nextFire;
    public float fireRate = 0.25f;
    public GameObject PlayerBullet;
    public float bulletSpeed = 25f;

    //Camera
    public Camera cam;
    public Transform camTrack;
    public Transform camTrackDown;
    public Transform camTrackUp;
    public GameObject Cinemachine;
    public float trackAwaitTime;
    private float trackTimerDown;
    private float trackTimerUp;
    private bool camTrackRestricted = false;
    private Transform prevCamTrack;

    //Run
    public float runSpeed = 600f;
    

    //Jump
    public int nowJumpCount;
    public float jumpTime = 0.3f;
    public float jumpTimeCount;
    public float cancelJumpSpeed;
    public float jumpVelocity = 10f;

    //Ability switch
    public int stance = 0;//0: Bullet, 1: Melee
    public bool dash = false;
    public int jumpCount = 1;

    //Dash
    private bool isDashing = false;
    private float dashCDTimer = 0;
    private float dashingTimer = 0;
    public float dashCD = 0.85f;
    public float dashingLength = 0.2f;
    public float dashingVelocity = 6000f;
    private bool dashReady = false;

    //Audios
    private AudioSource JumpAudio;
    private AudioSource GroundedAudio;
    private AudioSource DashAudio;
    private AudioSource ShootAudio;
    private AudioSource HitAudio;
    private AudioSource GetItemAudio;

    public GameObject AudioMana;

    //Animation
    public  Animator anim;
    public float animPlaySpeed = 2f;
    /*
     * Animation instruction:
     * 0:Idle
     * 1:Run
     * 2:Dashing
     * 3:Jumping
     * 4:Falling
     * 
     */

    //Properties
    public float HP = 50f;

    //UI relevance
    public Image UI_HPBar;
    public float temp_MaxHPCullUI = 0.93f;
    public float temp_MinHPCullUI = 0.08f;
    public Image Abi_one_dark;
    public Image Abi_one_bright;
    public Image Abi_two_dark;
    public Image Abi_two_bright;
    public GameObject UI_GO;

    //Hurt Invincible
    public float hurtInvincibleTime = 1.2f;
    public float FTDInvincibleTime = 6f;
    public float hurtInvTimer;

    public float hurtMoveTime = 0.4f;//Must less than hurtInvincibleTime
    public float hurtMoveTimer;
    public float hurtMoveSpeed = 1200f;

    public bool isHurtingInv = false;
    public bool isHuringMove = false;

    private Vector3 hitVec;

    public float hitOpacity = 0.3f;
    public float hitR = 1f;
    public float hitG = 0.85f;
    public float hitB = 0.85f;

    //Other Parameters
    private float gravityScale;
    public float touchHitForce = 200f;
    public float fallDieHP = 10f;
    public bool disableControl = false;
    public GameObject gameManager;
    public GameObject interactingObject;


    //Temp
    public GameObject djhint;
    public GameObject dashhint;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager");
        if (gameManager)
        {
            LoadPlayerStatus();
        }

        rb_player = GetComponent<Rigidbody2D>();
        gravityScale = rb_player.gravityScale;
        anim.speed = animPlaySpeed;

        nowJumpCount = jumpCount;
        jumpTimeCount = jumpTime;

        AudioMana = GameObject.Find("AudioManager");

        if (AudioMana)
        {
            JumpAudio = AudioMana.GetComponent<AudioManager>().PlayerJumpSFX;
            GroundedAudio = AudioMana.GetComponent<AudioManager>().PlayerLandSFX;
            DashAudio = AudioMana.GetComponent<AudioManager>().PlayerDashSFX;
            ShootAudio = AudioMana.GetComponent<AudioManager>().PlayerShootSFX;
            HitAudio = AudioMana.GetComponent<AudioManager>().PlayerHitSFX;
            GetItemAudio = AudioMana.GetComponent<AudioManager>().ItemPickupSFX;
        }

        UI_HPBar = GameObject.Find("PH_HPBar").GetComponent<Image>();

    }


    // Update is called once per frame
    void Update()
    {
        //Ground check and jumping
        bool isGroundedThisFrame = isGrounded;
        isGrounded = Physics2D.OverlapCircle(GroundCheck.position, groundCheckDistance, groundMask) || Physics2D.OverlapCircle(GroundCheckSecondary.position, groundCheckDistance, groundMask);
        if (isGroundedThisFrame == false && isGrounded == true)
        {
            if (GroundedAudio)
            {
                GroundedAudio.Play();
            }
        }
        //Reset double jump
        if (isGrounded)
        {
            nowJumpCount = jumpCount;
        }

        //Jump
        if (rb_player.velocity.y < -5f && nowJumpCount == jumpCount)
        {
            nowJumpCount--;
        }

        if (Input.GetKeyDown(KeyCode.Space) && nowJumpCount > 0 && !isDashing && !disableControl)
        {
            jumpTimeCount = jumpTime;
            if (JumpAudio)
            {
                JumpAudio.Play();
            }
        }

        if (Input.GetKey(KeyCode.Space) && nowJumpCount > 0 && !isDashing && jumpTimeCount > 0 && !disableControl)
        {
            rb_player.velocity = new Vector2(rb_player.velocity.x, jumpVelocity);
            jumpTimeCount -= Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (rb_player.velocity.y >= cancelJumpSpeed)
            {
                rb_player.velocity = new Vector2(rb_player.velocity.x, cancelJumpSpeed);//Hollow Knight-like jumping control
            }
            nowJumpCount--;
        }

        //Anti slope sliding
        if (isGrounded)
        {
            rb_player.gravityScale = 0;
        }
        else
        {
            rb_player.gravityScale = gravityScale;
        }

        //Facing
        if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) && !isDashing && !disableControl)
        {
            isTowardsRight = false;
            sprite_player.GetComponent<SpriteRenderer>().flipX = true;
        }
        else if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) && !isDashing && !disableControl)
        {
            isTowardsRight = true;
            sprite_player.GetComponent<SpriteRenderer>().flipX = false;
        }


        //Firing
        nextFire = nextFire + Time.deltaTime;
        if (Input.GetMouseButton(0) && nextFire > fireRate && !isDashing && !disableControl)
        {
            //Get mouse position and bullet angle
            Vector3 m_mousePosition = Input.mousePosition;
            m_mousePosition = cam.ScreenToWorldPoint(m_mousePosition);
            m_mousePosition.z = 0;
            float m_fireAngle = Vector2.SignedAngle(m_mousePosition - transform.position, Vector2.up);

            //Reset firing timer after firing
            nextFire = 0;

            GameObject m_bullet = Instantiate(PlayerBullet, transform) as GameObject;

            if (ShootAudio)
            {
                ShootAudio.Play();
            }

            //Set bullet projectile
            m_bullet.GetComponent<Rigidbody2D>().velocity = ((m_mousePosition - transform.position).normalized * bulletSpeed);
            m_bullet.transform.eulerAngles = new Vector3(0, 0, -m_fireAngle + 90);
        }

        //Interacting
        if (interactingObject && Input.GetKeyDown(KeyCode.W))
        {
            interactingObject.SendMessage("Interact");
        }

        //Dashing Part I
        if (dash && !isDashing && dashReady && stance == 0 && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1)) && !disableControl)
        {
            isDashing = true;
            dashReady = false;
            GetComponent<Rigidbody2D>().gravityScale = 0;
            if (DashAudio)
            {
                DashAudio.Play();
            }
            anim.SetInteger("AnimState", 2);
        }

        //Camera tracking & Look up / Look down
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0 && !disableControl && !camTrackRestricted)
        {
            if (Cinemachine.GetComponent<CinemachineVirtualCamera>().Follow == camTrackDown)
            {
                Cinemachine.GetComponent<CinemachineVirtualCamera>().Follow = camTrack;
            }
            else if (Cinemachine.GetComponent<CinemachineVirtualCamera>().Follow == camTrack)
            {
                Cinemachine.GetComponent<CinemachineVirtualCamera>().Follow = camTrackUp;
            }
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0 && !disableControl && !camTrackRestricted)
        {
            if (Cinemachine.GetComponent<CinemachineVirtualCamera>().Follow == camTrackUp)
            {
                Cinemachine.GetComponent<CinemachineVirtualCamera>().Follow = camTrack;
            }
            else if (Cinemachine.GetComponent<CinemachineVirtualCamera>().Follow == camTrack)
            {
                Cinemachine.GetComponent<CinemachineVirtualCamera>().Follow = camTrackDown;
            }
        }



        //UI Changes
        UI_HPBar.GetComponent<Image>().fillAmount = temp_MinHPCullUI + (temp_MaxHPCullUI - temp_MinHPCullUI) * HP / 50f;

        /*
        //Temp reset scene
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            SceneManager.LoadScene("m_GreyBox");
        }*/

        if (HP <= 0)
        {
            UI_GO.SetActive(true);
            //transform.position = new Vector3(99999, 99999, 99999);
            sprite_player.SetActive(false);
            rb_player.gravityScale = 0;
            rb_player.velocity = new Vector2(0, 0);
            disableControl = true;
        }


    }

    private void FixedUpdate()
    {
        //Moving
        if (!isDashing && !disableControl)
        {
            rb_player.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * runSpeed * Time.fixedDeltaTime, rb_player.velocity.y);

            /**/
            //Animation
            if (Mathf.Abs(rb_player.velocity.x) > 0.01 && Mathf.Abs(rb_player.velocity.y) < 10f && isGrounded)
            {
                anim.SetInteger("AnimState", 1);
            }
            else if (Mathf.Abs(rb_player.velocity.y) < 5f && isGrounded)
            {
                anim.SetInteger("AnimState", 0);
            }
            else if (rb_player.velocity.y > 0f)
            {
                anim.SetInteger("AnimState", 3);
            }
            else if (rb_player.velocity.y < 0f)
            {
                anim.SetInteger("AnimState", 4);
            }
        }

        //Dashing Part II
        if (isDashing && !disableControl)
        {
            dashingTimer += Time.fixedDeltaTime;
            if (dashingTimer >= dashingLength)
            {
                isDashing = false;
                dashingTimer = 0;
                GetComponent<Rigidbody2D>().gravityScale = gravityScale;
                if (Mathf.Abs(rb_player.velocity.x) > 0.01 && Mathf.Abs(rb_player.velocity.y) < 0.5f && isGrounded)
                {
                    anim.SetInteger("AnimState", 1);
                }
                else if (Mathf.Abs(rb_player.velocity.y) < 5f && isGrounded)
                {
                    anim.SetInteger("AnimState", 0);
                }
                else if (rb_player.velocity.y > 0f)
                {
                    anim.SetInteger("AnimState", 3);
                }
                else if (rb_player.velocity.y < 0f)
                {
                    anim.SetInteger("AnimState", 4);
                }
            }

            if (isTowardsRight)
            {
                rb_player.velocity = Vector2.right * dashingVelocity * Time.fixedDeltaTime;
            }
            else
            {
                rb_player.velocity = Vector2.left * dashingVelocity * Time.fixedDeltaTime;
            }
        }

        //Dash CD timer action
        if (!dashReady)
        {
            dashCDTimer += Time.fixedDeltaTime;
            if (dashCDTimer >= dashCD)
            {
                dashReady = true;
                dashCDTimer = 0;
            }
        }


        //Hurt Invincible Movement
        if (isHuringMove)
        {
            if (hitVec.x <= 0)//left
            {
                rb_player.velocity = Vector2.left * hurtMoveSpeed * Time.fixedDeltaTime;
            }
            else
            {
                rb_player.velocity = Vector2.right * hurtMoveSpeed * Time.fixedDeltaTime;
            }

            hurtMoveTimer += Time.fixedDeltaTime;
            if (hurtMoveTimer >= hurtMoveTime)
            {
                HurtInvincibleMoveRecover();
            }
        }

        if (isHurtingInv)
        {
            sprite_player.GetComponent<SpriteRenderer>().color = new Color(hitR, hitG, hitB, hitOpacity + (1 - hitOpacity) * Mathf.Abs(Mathf.Cos(4 * Mathf.PI * hurtInvTimer / hurtInvincibleTime)));

            hurtInvTimer += Time.fixedDeltaTime;
            if (hurtInvTimer >= hurtInvincibleTime)
            {
                HurtInvincibleEnd();
            }

        }


    }


    public void GetItem(int itemIndex)
    {
        switch (itemIndex)
        {
            case 0:
                jumpCount++;
                if (gameManager)
                {
                    gameManager.GetComponent<GameManager>().playerJumpCount = jumpCount;
                }
                Abi_one_dark.gameObject.SetActive(false);
                Abi_one_bright.gameObject.SetActive(true);

                if (djhint)
                {
                    djhint.SetActive(true);
                }

                break;
            case 1:
                Abi_two_dark.gameObject.SetActive(false);
                Abi_two_bright.gameObject.SetActive(true);
                dash = true;
                if (gameManager)
                {
                    gameManager.GetComponent<GameManager>().playerDash = dash;
                }

                if (dashhint)
                {
                    dashhint.SetActive(true);
                }

                break;
            case 2:
                if (HP <= 30f)
                {
                    HP += 20f;
                    if (gameManager)
                    {
                        gameManager.GetComponent<GameManager>().playerHP = HP;
                    }
                }
                else
                {
                    HP = 50f;
                    if (gameManager)
                    {
                        gameManager.GetComponent<GameManager>().playerHP = HP;
                    }
                }
                break;
        }

        if (GetItemAudio)
        {
            GetItemAudio.Play();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GetHit(collision);
        FallToDie(collision);

        if (collision.CompareTag("Interactable"))
        {
            interactingObject = collision.gameObject;
        }

        if (collision.CompareTag("CamTrackRestriction"))
        {
            camTrackRestricted = true;

            prevCamTrack = Cinemachine.GetComponent<CinemachineVirtualCamera>().Follow;

            switch (collision.GetComponent<CamRestrictTrigger>().camTrack)
            {
                case CamRestrictTrigger.CamTrack.Up:
                    Cinemachine.GetComponent<CinemachineVirtualCamera>().Follow = camTrackUp;
                    break;
                case CamRestrictTrigger.CamTrack.Middle:
                    Cinemachine.GetComponent<CinemachineVirtualCamera>().Follow = camTrack;
                    break;
                case CamRestrictTrigger.CamTrack.Down:
                    Cinemachine.GetComponent<CinemachineVirtualCamera>().Follow = camTrackDown;
                    break;
            }
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Interactable"))
        {
            interactingObject = null;
        }

        if (collision.CompareTag("CamTrackRestriction"))
        {
            camTrackRestricted = false;
            Cinemachine.GetComponent<CinemachineVirtualCamera>().Follow = prevCamTrack;
        }
    }

    private void LoadPlayerStatus()
    {
        //HP, Jump count, Dash status
        HP = gameManager.GetComponent<GameManager>().playerHP;
        jumpCount = gameManager.GetComponent<GameManager>().playerJumpCount;
        dash = gameManager.GetComponent<GameManager>().playerDash;

        Debug.Log("Status loaded");
    }

    private void GetHit(Collider2D HitGO)
    {
        //Touched enemy and get damage
        if (HitGO.gameObject.CompareTag("Enemy"))
        {
            if (HitAudio)
            {
                HitAudio.Play();
            }

            HP -= HitGO.gameObject.GetComponent<Enemy>().touchDamage;
            if (gameManager)
            {
                gameManager.GetComponent<GameManager>().playerHP = HP;
            }

            //Hit invincible action
            hitVec = Vector3.Normalize(new Vector3(transform.position.x - HitGO.transform.position.x, 0, 0));
            HurtInvincible(hurtInvincibleTime);

            //Metrics collecting
            if (MetricManagerScript.instance != null)
            {
                MetricManagerScript.instance.LogString(HitGO.gameObject.name, "" + HP);
            }
        }

        //Get hit by enemy bullet
        if (HitGO.gameObject.CompareTag("EnemyBullet"))
        {
            if (HitAudio)
            {
                HitAudio.Play();
            }

            HP -= HitGO.gameObject.GetComponent<EnemyBullet>().damage;
            if (gameManager)
            {
                gameManager.GetComponent<GameManager>().playerHP = HP;
            }

            //Hit invincible action
            hitVec = Vector3.Normalize(new Vector3(transform.position.x - HitGO.transform.position.x, 0, 0));
            HurtInvincible(hurtInvincibleTime);

            //Metrics collecting
            if (MetricManagerScript.instance != null)
            {
                MetricManagerScript.instance.LogString(HitGO.gameObject.name, "" + HP);
            }

            Destroy(HitGO.gameObject);

        }
    }

    private void FallToDie(Collider2D FTDGO)
    {
        //Touch a FTD Trigger
        if (FTDGO.gameObject.CompareTag("DieTrigger"))
        {
            HP -= fallDieHP;
            if (HP > 0)
            {
                transform.position = FTDGO.GetComponent<DieTrigger>().respawnPoint.transform.position;
            }
            if (gameManager)
            {
                gameManager.GetComponent<GameManager>().playerHP = HP;
            }

            //FTD invincibile action




            //Metrics collecting
            if (MetricManagerScript.instance != null)
            {
                MetricManagerScript.instance.LogString(FTDGO.gameObject.name, "" + HP);
            }

        }
    }

    private void HurtInvincible(float invTime)
    {
        //if dashing, reset it
        if (isDashing)
        {
            isDashing = false;
            dashingTimer = 0;

            //Later turn this into hurt animation
            if (Mathf.Abs(rb_player.velocity.x) > 0.01 && Mathf.Abs(rb_player.velocity.y) < 0.5f && isGrounded)
            {
                anim.SetInteger("AnimState", 1);
            }
            else if (Mathf.Abs(rb_player.velocity.y) < 5f && isGrounded)
            {
                anim.SetInteger("AnimState", 0);
            }
            else if (rb_player.velocity.y > 0f)
            {
                anim.SetInteger("AnimState", 3);
            }
            else if (rb_player.velocity.y < 0f)
            {
                anim.SetInteger("AnimState", 4);
            }
        }

        //turn on move switch, set move velocity
        disableControl = true;
        isHuringMove = true;
        isHurtingInv = true;
        GetComponent<Rigidbody2D>().gravityScale = 0;

        gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");

        /*Play Audio*/
        /*Set Anim*/


        //Change opacity, placeholder visual feedback for invincible time
        //sprite_player.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, hitOpacity);



        //Invoke("HurtInvincibleEnd", invTime);
    }

    private void HurtInvincibleMoveRecover()
    {
        hurtMoveTimer = 0;
        isHuringMove = false;
        disableControl = false;
        GetComponent<Rigidbody2D>().gravityScale = gravityScale;
    }

    private void HurtInvincibleEnd()
    {
        hurtInvTimer = 0;
        isHurtingInv = false;
        gameObject.layer = LayerMask.NameToLayer("Player");

        //Change opacity, placeholder visual feedback for invincible time
        sprite_player.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        Debug.Log("Recovered");
    }


}
