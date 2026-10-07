using System.Collections;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 

public class PlayerController : MonoBehaviour
{
    [Header("移動與跳躍參數")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public LayerMask whatIsGround; // 用來綁定 Ground 圖層

    [Header("地面偵測點陣列 (Array 要求)")]
    public Transform[] groundCheckPoints; 
    public float checkRadius = 0.1f; 

    [Header("虛空重生與生命系統")]
    public float voidYThreshold = -10f; 
    public int maxLives = 3; 
     [Header("結束場面 UI 設定")]
    public GameObject winUIWindow; 
    public GameObject gameOverUIWindow; 
     [Header("當前生命顯示 UI")]
    public Text livesText; 

    private int currentLives;
    private bool isGrounded;
    private bool isGameOver = false;

    private Vector2 spawnPoint; 

    private Rigidbody2D rb;
    private Collider2D playerCollider;

    void Start()
    {
        Time.timeScale = 1f;
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>(); 

        // 遊戲啟動瞬間，用 Vector2 記住出生點
        spawnPoint = new Vector2(transform.position.x, transform.position.y);
        currentLives = maxLives;

        if (winUIWindow != null) winUIWindow.SetActive(false);
        if (gameOverUIWindow != null) gameOverUIWindow.SetActive(false);
        
        UpdateLivesUI();

        Debug.Log($"【遊戲開始】當前剩餘生命：{currentLives} 次");
    }

    void Update()
    {
        // 如果遊戲結束（失敗或通關），立刻鎖定輸入
        if (isGameOver) return;

        if (transform.position.y < voidYThreshold)
        {
            HandlePlayerDeath(); 
        }

        // 偵測水平輸入（A/D 或 左右鍵）
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else if (horizontalInput < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }

        isGrounded = false;
        foreach (Transform checkPoint in groundCheckPoints)
        {
            if (Physics2D.OverlapCircle(checkPoint.position, checkRadius, whatIsGround))
            {
                isGrounded = true;
                break; // 只要有一隻腳踩到地面就判定落地
            }
        }

        // 偵測跳躍與平台下跳（空白鍵）
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                StartCoroutine(DisableCollision()); 
            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }
    }

    void FixedUpdate()
    {
        if (isGameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float horizontalInput = Input.GetAxisRaw("Horizontal");

        Vector2 movement = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        rb.linearVelocity = movement;
    }

    // 處理玩家落入虛空死亡
    void HandlePlayerDeath()
    {
        currentLives--; 
        UpdateLivesUI();

        
        if (currentLives > 0)
        {
            Debug.Log($"【作業提示】掉入虛空！扣除 1 點生命！剩餘生命：{currentLives} 次");
            Respawn(); 
        }
        else
        {
            Debug.Log("【作業提示】生命值已歸零！遊戲失敗 (Game Over)！");
            isGameOver = true; 
            rb.linearVelocity = Vector2.zero; 
             if (gameOverUIWindow != null) gameOverUIWindow.SetActive(true);
             Time.timeScale = 0f;

        }
    }

    void Respawn()
    {
        transform.position = spawnPoint; // 覆寫座標向量，瞬移回 Vector2 起點
        rb.linearVelocity = Vector2.zero; // 關鍵：清空掉落時累積的極高物理墜落速度
    }

    // 平台下跳的計時器協程
    private IEnumerator DisableCollision()
    {
        if (groundCheckPoints.Length > 0 && groundCheckPoints != null)
        {
            // 拿陣列第一個點（左腳）畫圓圈抓取腳底的平台碰撞體
            Collider2D platformCollider = Physics2D.OverlapCircle(groundCheckPoints[0].position, 0.2f, whatIsGround);
            
            if (platformCollider != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -3f);
                Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
                yield return new WaitForSeconds(0.5f);
                Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
            }
        }
    }

    // 當角色穿透進入過關區域時自動觸發
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Finish"))
        {
            isGameOver = true; 
            rb.linearVelocity = Vector2.zero;
            Debug.Log("【作業提示】恭喜！順利抵達終點，通關成功！");
             if (winUIWindow != null) winUIWindow.SetActive(true);
             Time.timeScale = 0f;

        }
    }

    void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = $"當前生命: {currentLives} / {maxLives}";
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
