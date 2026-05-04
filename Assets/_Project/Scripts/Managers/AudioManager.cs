using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM")]
    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioClip gameBGM;
    [SerializeField] AudioClip mainMenuBGM;

    [Header("SFX")]
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioClip flagCollectSFX;
    [SerializeField] AudioClip jumpSFX;
    [SerializeField] AudioClip walkSFX;
    [SerializeField] AudioClip attackSFX;
    [SerializeField] AudioClip hurtSFX;
    [SerializeField] AudioClip deathSFX;
    [SerializeField] AudioClip bossHurtSFX;
    [SerializeField] AudioClip bossDeathSFX;
    [SerializeField] AudioClip enemyDeathSFX;

    [Header("Walk Settings")]
    [SerializeField] float walkSFXInterval = 0.4f;
    float walkTimer = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
            PlayBGM(mainMenuBGM);
        else
            PlayBGM(gameBGM);

        Debug.Log($"AudioManager — scene: {scene.name}");
    }

    void Start()
    {
        // Play BGM for whichever scene starts first
        string current = SceneManager.GetActiveScene().name;
        if (current == "MainMenu")
            PlayBGM(mainMenuBGM);
        else
            PlayBGM(gameBGM);
    }

    void Update()
    {
        if (walkTimer > 0f)
            walkTimer -= Time.deltaTime;
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.volume = 0.4f;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    public void PlayFlag()      => PlaySFX(flagCollectSFX, 0.9f);
    public void PlayJump()      => PlaySFX(jumpSFX, 0.8f);
    public void PlayAttack()    => PlaySFX(attackSFX, 0.7f);
    public void PlayHurt()      => PlaySFX(hurtSFX, 0.9f);
    public void PlayDeath()     => PlaySFX(deathSFX, 1f);
    public void PlayBossHurt()  => PlaySFX(bossHurtSFX, 0.8f);
    public void PlayBossDeath() => PlaySFX(bossDeathSFX, 1f);
    public void PlayEnemyDeath()=> PlaySFX(enemyDeathSFX, 0.7f);

    public void PlayWalk()
    {
        if (walkTimer > 0f) return;
        PlaySFX(walkSFX, 0.3f);
        walkTimer = walkSFXInterval;
    }

    void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }
}