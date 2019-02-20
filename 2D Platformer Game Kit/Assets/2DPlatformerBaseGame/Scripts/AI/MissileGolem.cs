using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BTAI;
using UnityEngine.Events;
using System;

public class MissileGolem : MonoBehaviour
#if UNITY_EDITOR
    , BTAI.IBTDebugable
#endif
{
    [System.Serializable]
    public class BossRound
    {
        public float platformSpeed = 1;
        public MovingPlatform[] platforms;
        public GameObject[] enableOnProgress;
        public int bossHP = 10;
        public int shieldHP = 10;
    }

    public Transform target;

    public int laserStrikeCount = 2;
    public float laserTrackingSpeed = 30.0f;
    public float delay = 2;
    public float beamDelay, grenadeDelay, lightningDelay, cleanupDelay, deathDelay;

    public GameObject shield, beamLaser;
    public GunnerProjectile projectile;
    public Grenade grenade;
    public GameObject lightning;
    public Damageable damageable;
    public float lightningTime = 1;
    public Transform grenadeSpawnPoint;
    public Vector2 grenadeLaunchVelocity;
    [Space]
    public BossRound[] rounds;
    [Space]
    public GameObject[] disableOnDeath;
    public UnityEvent onDefeated;

    [Header("Audio")]
    public AudioClip bossDeathClip;
    public AudioClip playerDeathClip;
    public AudioClip postBossClip;
    public AudioClip bossMusic;
    [Space]
    public RandomAudioPlayer stepAudioPlayer;
    public RandomAudioPlayer laserFireAudioPlayer;
    public RandomAudioPlayer grenadeThrowAudioPlayer;
    public RandomAudioPlayer lightningAttackAudioPlayer;
    public RandomAudioPlayer takingDamage;
    public RandomAudioPlayer shieldUpAudioPlayer;
    public RandomAudioPlayer shieldDownAudioPlayer;
    [Space]
    public AudioSource roundDeathSource;
    public AudioClip startRound2Clip;
    public AudioClip startRound3Clip;
    public AudioClip deathClip;

    [Header("UI")]
    public Slider healthSlider;
    public Slider shieldSlider;

    bool onFloor = false;
    int round = 0;

    private int totalHealth = 0;
    private int currentHealth = 0;

    private Vector2 previousTargetPosition; // used to track target movement, to correct for it.

    public void SetPlayerFloor(bool onFloor)
    {
        this.onFloor = onFloor;
    }

    Animator animator;
    Root ai;
    Vector3 originShieldScale;

    void OnEnable()
    {
        if (PlayerCharacter.Instance != null)
            PlayerCharacter.Instance.damageable.OnDie.AddListener(PlayerDied);
        originShieldScale = shield.transform.localScale;
        animator = GetComponent<Animator>();

        round = 0;

        ai = BT.Root();
        ai.OpenBranch(
            //First Round
            BT.SetActive(beamLaser, false),
            BT.Repeat(rounds.Length).OpenBranch(
                BT.Call(NextRound),
                //Grenade enabled is true only on 2 and 3 round, so allow to just test if it's the 1st round or not here
                BT.If(GrenadeEnabled).OpenBranch(
                    BT.Trigger(animator, "Enabled")
                    ),
                BT.Wait(delay),
                BT.Call(ActivateShield),
                BT.Wait(delay),
                BT.While(ShieldIsUp).OpenBranch(
                    BT.RandomSequence(new int[] { 1, 6, 4, 4 }).OpenBranch(
                        BT.Root().OpenBranch(
                            BT.Trigger(animator, "Walk"),
                            BT.Wait(0.2f),
                            BT.WaitForAnimatorState(animator, "Idle")
                            ),
                        BT.Repeat(laserStrikeCount).OpenBranch(
                            BT.SetActive(beamLaser, true),
                            BT.Trigger(animator, "Beam"),
                            BT.Wait(beamDelay),
                            BT.Call(FireLaser),
                            BT.WaitForAnimatorState(animator, "Idle"),
                            BT.SetActive(beamLaser, false),
                            BT.Wait(delay)
                        ),
                        BT.If(PlayerOnFloor).OpenBranch(
                            BT.Trigger(animator, "Lightning"),
                            BT.Wait(lightningDelay),
                            BT.Call(ActivateLightning),
                            BT.Wait(lightningTime),
                            BT.Call(DeactivateLightning),
                            BT.Wait(delay)
                        ),
                        BT.If(GrenadeEnabled).OpenBranch(
                            BT.Trigger(animator, "Grenade"),
                            BT.Wait(grenadeDelay),
                            BT.Call(ThrowGrenade),
                            BT.WaitForAnimatorState(animator, "Idle")
                        )
                    )
                ),
                BT.SetActive(beamLaser, false),
                BT.Trigger(animator, "Grenade", false),
                BT.Trigger(animator, "Beam", false),
                BT.Trigger(animator, "Lightning", false),
                BT.Trigger(animator, "Disable"),
                BT.While(IsAlive).OpenBranch(BT.Wait(0))
            ),
            BT.Trigger(animator, "Death"),
            BT.SetActive(damageable.gameObject, false),
            BT.Wait(cleanupDelay),
            BT.Call(Cleanup),
            BT.Wait(deathDelay),
            BT.Call(Die),
            BT.Terminate()
        );

        BackgroundMusicPlayer.Instance.ChangeMusic(bossMusic);
        BackgroundMusicPlayer.Instance.Play();
        BackgroundMusicPlayer.Instance.Unmute(2.0f);

        //We aggregate the total health to set the slider to the proper value
        //As the boss is actually "killed" every round and regenerated, we can't use directly its current health)
        for (int i = 0; i < rounds.Length; i++)
        {
            totalHealth += rounds[i].bossHP;
        }
        currentHealth = totalHealth;

        healthSlider.maxValue = totalHealth;
        healthSlider.value = totalHealth;

        if (target != null)
            previousTargetPosition = target.transform.position;
    }

    void OnDisable()
    {
        if (PlayerCharacter.Instance != null)
            PlayerCharacter.Instance.damageable.OnDie.RemoveListener(PlayerDied);
    }

    void PlayerDied(Damager d, Damageable da)
    {
        BackgroundMusicPlayer.Instance.PushClip(playerDeathClip);
    }

    void ActivateShield()
    {
        shieldUpAudioPlayer.PlayRandomSound();

        shield.SetActive(true);
        shield.transform.localScale = Vector3.one * 0.01f;

        shieldSlider.GetComponent<Animator>().Play("BossShieldActivate");

        Damageable shieldDamageable = shield.GetComponent<Damageable>();

        //need to be set after enabled happens, otherwise enable resets health. That's why we use round - 1, round was already advance at that point
        shieldDamageable.SetHealth(rounds[round - 1].shieldHP);
        shieldSlider.maxValue = rounds[round - 1].shieldHP;
        shieldSlider.value = shieldSlider.maxValue;
    }

    void FireLaser()
    {
        laserFireAudioPlayer.PlayRandomSound();

        var p = Instantiate(projectile);
        var dir = -beamLaser.transform.right;
        p.transform.position = beamLaser.transform.position;
        p.initialForce = new Vector3(dir.x, dir.y) * 1000;
    }

    void ThrowGrenade()
    {
        grenadeThrowAudioPlayer.PlayRandomSound();

        var p = Instantiate(grenade);
        p.transform.position = grenadeSpawnPoint.position;
        p.initialForce = grenadeLaunchVelocity;
    }

    bool GrenadeEnabled()
    {
        return round > 1;
    }

    void ActivateLightning()
    {
        lightningAttackAudioPlayer.PlayRandomSound();

        var p = Instantiate(lightning) as GameObject;
        p.transform.position = transform.position;
        Destroy(p, lightningTime);
    }

    void DeactivateLightning()
    {
        lightningAttackAudioPlayer.Stop();
    }

    private void FixedUpdate()
    {
        if (target != null)
            previousTargetPosition = target.position;
    }

    void Update()
    {
        ai.Tick();
        if (beamLaser != null && target != null)
        {
            Vector2 targetMovement = (Vector2)target.position - previousTargetPosition;
            targetMovement.Normalize();
            Vector3 targetPos = target.position + Vector3.up * (1.0f + targetMovement.y * 0.5f);

            beamLaser.transform.rotation = Quaternion.RotateTowards(beamLaser.transform.rotation, Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.left, targetPos - beamLaser.transform.position, Vector3.forward)), laserTrackingSpeed * Time.deltaTime);
        }

        shield.transform.localScale = Vector3.Lerp(shield.transform.localScale, originShieldScale, Time.deltaTime);
    }

    void Cleanup()
    {
        shieldSlider.GetComponent<Animator>().Play("BossShieldDefeat");
        healthSlider.GetComponent<Animator>().Play("BossHealthDefeat");

        BackgroundMusicPlayer.Instance.ChangeMusic(postBossClip);
        BackgroundMusicPlayer.Instance.PushClip(bossDeathClip);

        roundDeathSource.clip = deathClip;
        roundDeathSource.loop = false;
        roundDeathSource.Play();

        foreach (var g in disableOnDeath)
            g.SetActive(false);
        shield.SetActive(false);
        beamLaser.SetActive(false);
    }

    void Die()
    {
        onDefeated.Invoke();
    }

    bool PlayerOnPlatform()
    {
        return !onFloor;
    }

    bool PlayerOnFloor()
    {
        return onFloor;
    }

    bool IsAlive()
    {
        bool alive = damageable.CurrentHealth > 0;
        return alive;
    }

    bool IsNotAlmostDead()
    {
        bool alive = damageable.CurrentHealth > 1;
        return alive;
    }

    bool ShieldIsUp()
    {
        return shield.GetComponent<Damageable>().CurrentHealth > 0;
    }

    void NextRound()
    {
        damageable.SetHealth(rounds[round].bossHP);
        damageable.EnableInvulnerability(true);
        foreach (var p in rounds[round].platforms)
        {
            p.gameObject.SetActive(true);
            p.speed = rounds[round].platformSpeed;
        }
        foreach (var g in rounds[round].enableOnProgress)
        {
            g.SetActive(true);
        }
        round++;

        if (round == 2)
        {
            roundDeathSource.clip = startRound2Clip;
            roundDeathSource.loop = true;
            roundDeathSource.Play();
        }
        else if (round == 3)
        {
            roundDeathSource.clip = startRound3Clip;
            roundDeathSource.loop = true;
            roundDeathSource.Play();
        }
    }

    void Disabled()
    {

    }

    void Enabled()
    {

    }

    public void Damaged(Damager damager, Damageable damageable)
    {
        takingDamage.PlayRandomSound();

        currentHealth -= damager.damage;
        healthSlider.value = currentHealth;
    }

    public void ShieldDown()
    {
        shieldDownAudioPlayer.PlayRandomSound();
        damageable.DisableInvulnerability();
    }

    public void ShieldHit()
    {
        shieldSlider.value = shield.GetComponent<Damageable>().CurrentHealth;
    }

    public void Playstep()
    {
        stepAudioPlayer.PlayRandomSound();
    }

#if UNITY_EDITOR
    public BTAI.Root GetAIRoot()
    {
        return ai;
    }
#endif
}
