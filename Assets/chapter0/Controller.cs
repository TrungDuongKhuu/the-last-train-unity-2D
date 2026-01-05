using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Lớp quản lý buff đang hoạt động
public class ActiveBuff
{
    public BuffEffect Buff { get; private set; }
    public float RemainingTime { get; set; }
    public bool IsPermanent { get; private set; }

    public ActiveBuff(BuffEffect buff)
    {
        Buff = buff;
        RemainingTime = buff.duration;
        IsPermanent = buff.duration <= 0;
    }
}

public class Controller : MonoBehaviour
{
    [Header("Movement")]
    public Rigidbody2D rb;
    public Animator animator;
    public float baseWalkSpeed = 5f;
    public float baseRunSpeed = 10f;

    private float currentWalkSpeed;
    private float currentRunSpeed;

    // Input movement + animation idle direction
    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public bool isRun;
    [HideInInspector] public float lastX = 0;
    [HideInInspector] public float lastY = -1;

    private float currentSpeed;
    private Actions action;

    [Header("Combat")]
    public float baseDamageMultiplier = 1f;
    private float currentDamageMultiplier;

    [Header("Mana")]
    public GameObject manaObject;
    public float manaRecoverRate = 0.1f;
    public float manaConsumeRate = 0.2f;

    [Header("Health")]
    public GameObject healthObject;
    public float healthRate;

    [Header("Audio")]
    public AudioClip walkSound;
    public AudioClip runSound;
    public AudioClip useItemSound;
    [Range(0f, 1f)] public float movementSoundVolume = 0.5f;
    [Range(0f, 1f)] public float itemSoundVolume = 0.7f;

    private Slider manaSlider;
    private Slider healthSlider;
    private AudioSource audioSource;
    private bool wasMoving = false;

    private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

    // Đang đứng gần item nhặt
    [HideInInspector] public bool nearInteractableItem = false;

    private void OnEnable()
    {
        manaSlider = manaObject.GetComponent<Slider>();
        healthSlider = healthObject.GetComponent<Slider>();

        action = new Actions();
        action.MainPlayer.Enable();

        // Input chạy
        action.MainPlayer.Run.performed += ctx => TryStartRun();
        action.MainPlayer.Run.canceled += ctx => StopRun();

        // Input di chuyển
        action.MainPlayer.Move.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
            if (moveInput != Vector2.zero)
            {
                lastX = moveInput.x;
                lastY = moveInput.y;
            }
        };
        action.MainPlayer.Move.canceled += ctx => moveInput = Vector2.zero;

        // Input sử dụng item
        action.MainPlayer.UseItem.performed += ctx => UseSelectedItem();

        // Cuộn thay item
        action.MainPlayer.ChangeItem.performed += ctx => ChangeItem(ctx.ReadValue<float>());
    }

    private void OnDisable() => action.MainPlayer.Disable();

    private void Start()
    {
        currentWalkSpeed = baseWalkSpeed;
        currentRunSpeed = baseRunSpeed;
        currentDamageMultiplier = baseDamageMultiplier;

        currentSpeed = currentWalkSpeed;

        if (healthSlider != null) healthSlider.value = healthSlider.maxValue;
        if (manaSlider != null) manaSlider.value = manaSlider.maxValue;

        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        HandleBuffs();
        HandleMana();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    // ============================
    // MOVEMENT
    // ============================
    private void HandleMovement()
    {
        rb.linearVelocity = moveInput.normalized * currentSpeed;

        animator.SetFloat("moveX", moveInput.x);
        animator.SetFloat("moveY", moveInput.y);
        animator.SetBool("isRunning", isRun);

        bool isMoving = moveInput.magnitude > 0.1f;

        if (isMoving && !wasMoving)
            PlayMovementSound();
        else if (!isMoving && wasMoving)
            StopMovementSound();
        else if (isMoving)
            UpdateMovementSound();

        wasMoving = isMoving;
    }

    private void TryStartRun()
    {
        if (manaSlider.value > 0)
        {
            isRun = true;
            currentSpeed = currentRunSpeed;
        }
    }

    private void StopRun()
    {
        isRun = false;
        currentSpeed = currentWalkSpeed;
    }

    // ============================
    // ITEM
    // ============================
    private void UseSelectedItem()
    {
        if (nearInteractableItem)
        {
            Debug.Log("Cannot use item while near interactable item.");
            return;
        }

        ItemData item = InventoryManager.instance.GetSelectedItem();

        if (item != null)
        {
            PlayUseItemSound();
            item.Use(this);
        }
        else Debug.Log("No item to use.");
    }

    private void ChangeItem(float scroll)
    {
        if (scroll > 0) InventoryManager.instance.ChangeSelectedItem(1);
        else if (scroll < 0) InventoryManager.instance.ChangeSelectedItem(-1);
    }

    // ============================
    // BUFF
    // ============================
    public void ApplyBuff(BuffEffect buff)
    {
        var existing = activeBuffs.FirstOrDefault(b => b.Buff == buff);
        if (existing != null) existing.RemainingTime = buff.duration;
        else activeBuffs.Add(new ActiveBuff(buff));

        RecalculateStats();
    }

    private void HandleBuffs()
    {
        bool statChanged = false;

        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            var buff = activeBuffs[i];
            if (!buff.IsPermanent)
            {
                buff.RemainingTime -= Time.deltaTime;
                if (buff.RemainingTime <= 0)
                {
                    activeBuffs.RemoveAt(i);
                    statChanged = true;
                }
            }
        }

        if (statChanged) RecalculateStats();
    }

    private void RecalculateStats()
    {
        currentWalkSpeed = baseWalkSpeed;
        currentRunSpeed = baseRunSpeed;
        currentDamageMultiplier = baseDamageMultiplier;

        foreach (var b in activeBuffs)
        {
            switch (b.Buff.statToModify)
            {
                case StatType.WalkSpeed:
                    currentWalkSpeed = b.Buff.isMultiplier ?
                        currentWalkSpeed * b.Buff.value : currentWalkSpeed + b.Buff.value;
                    break;

                case StatType.RunSpeed:
                    currentRunSpeed = b.Buff.isMultiplier ?
                        currentRunSpeed * b.Buff.value : currentRunSpeed + b.Buff.value;
                    break;

                case StatType.DamageMultiplier:
                    currentDamageMultiplier = b.Buff.isMultiplier ?
                        currentDamageMultiplier * b.Buff.value : currentDamageMultiplier + b.Buff.value;
                    break;
            }
        }

        currentSpeed = isRun ? currentRunSpeed : currentWalkSpeed;
    }

    // ============================
    // MANA
    // ============================
    private void HandleMana()
    {
        if (isRun)
        {
            if (manaSlider.value > 0)
                manaSlider.value -= manaConsumeRate * Time.deltaTime;
            else
                StopRun();
        }
        else if (!action.MainPlayer.Run.IsPressed() && manaSlider.value < manaSlider.maxValue)
            manaSlider.value += manaRecoverRate * Time.deltaTime;
    }

    public void RestoreMana(float amount)
    {
        if (manaSlider != null)
            manaSlider.value = Mathf.Min(manaSlider.maxValue, manaSlider.value + amount);
    }

    // ============================
    // HEALTH
    // ============================
    public bool RestoreHealth(float amount)
    {
        if (healthSlider != null && healthSlider.value < healthSlider.maxValue)
        {
            healthSlider.value = Mathf.Min(healthSlider.maxValue, healthSlider.value + amount);
            return true;
        }
        return false;
    }

    public void TakeDamage(float damage)
    {
        if (healthSlider != null)
        {
            float oldHealth = healthSlider.value;
            healthSlider.value = Mathf.Max(0, healthSlider.value - damage);
            Debug.Log($"Player took {damage} damage. Health: {oldHealth} → {healthSlider.value} (Max: {healthSlider.maxValue})");
            
            if (healthSlider.value <= 0)
            {
                Debug.Log("Player has died. Reloading scene...");
                StartCoroutine(ReloadSceneAfterDelay(2f));
            }
        }
    }

    private IEnumerator ReloadSceneAfterDelay(float delay)
    {
        DisableControl();
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ============================
    // AUDIO
    // ============================
    private void PlayMovementSound()
    {
        if (!audioSource) return;

        AudioClip clip = isRun ? runSound : walkSound;
        if (clip != null && audioSource.clip != clip)
        {
            audioSource.clip = clip;
            audioSource.volume = movementSoundVolume;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void UpdateMovementSound()
    {
        if (!audioSource) return;

        AudioClip clip = isRun ? runSound : walkSound;
        if (clip != null && audioSource.clip != clip)
        {
            audioSource.clip = clip;
            audioSource.volume = movementSoundVolume;
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
    }

    private void StopMovementSound()
    {
        if (audioSource && audioSource.isPlaying)
            audioSource.Stop();
    }

    private void PlayUseItemSound()
    {
        if (useItemSound != null && audioSource != null)
            audioSource.PlayOneShot(useItemSound, itemSoundVolume);
    }

    // ============================
    // CONTROL ENABLE / DISABLE
    // ============================
    public void DisableControl()
    {
        moveInput = Vector2.zero;
        isRun = false;
        currentSpeed = 0;
        action.MainPlayer.Disable();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public void EnableControl()
    {
        action.MainPlayer.Enable();
        currentSpeed = currentWalkSpeed;
    }
}
