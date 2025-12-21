using System.Collections;
using System.Collections.Generic;
using ObserverPattern;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f; // Thời gian crossfade

    
    [SerializeField] private AudioSource MusicSource_1;
    [SerializeField] private AudioSource MusicSource_2;
    [SerializeField] private Stack<AudioSource> FxSourcePool;
    [SerializeField] private GameObject FxSourceRoot;

    

    private AudioSource currentMusicSource;
    private AudioSource nextMusicSource;
    private List<AudioSource> allFxSources;

    public static AudioManager Instance { get; private set; }


    void Awake()
    {
        //Khoi tạo singleton
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }

        //Khoi tạo pool âm thanh
        if(allFxSources == null)
        {
            FxSourcePool = new Stack<AudioSource>();
            allFxSources = new List<AudioSource>(FxSourceRoot.GetComponentsInChildren<AudioSource>(true));
            foreach (AudioSource source in allFxSources)
            {
                FxSourcePool.Push(source);
            }
        }

        currentMusicSource = MusicSource_1;
        nextMusicSource = MusicSource_2;

        //Dang ky các sự kiện
        Observer.AddListener(EvenID.ChangeMusic, ChangeMusic);
        Observer.AddListener(EvenID.PlayFX, PlayFx);
    }

    void Start()
    {
        StartCoroutine(InitiationVolumes());
    }

    IEnumerator InitiationVolumes()
    {
        yield return new WaitForSecondsRealtime(0.1f); // Chờ một chút để đảm bảo dữ liệu đã sẵn sàng
        float musicVolume = GameManager.Instance.PlayerDataManager.PlayerData.MusicVolume;

        Debug.Log("Initial Music Volume: " + musicVolume);

        currentMusicSource.Play();
        nextMusicSource.volume = 0;
    }

    public void ChangeMusicVolume(float volume)
    {
        currentMusicSource.volume = volume;
    }

    public void ChangeFxVolume(float volume)
    {
        foreach (AudioSource source in allFxSources)
        {
            source.volume = volume;
        }
    }

    //Thay đổi nhạc nền
    private void ChangeMusic(object[] data)
    {
        if (data == null || data.Length == 0) return; // Kiểm tra dữ liệu có hợp lệ không

        AudioClip newClip = (AudioClip)data[0];
        if(newClip == currentMusicSource.clip) return; //Nếu âm thanh mới giống âm thanh hiện tại thì không làm gì cả

        float targetVolume = GameManager.Instance.PlayerDataManager.PlayerData.MusicVolume;

        //gắn clip mới vào nguồn nhạc tiếp theo
        nextMusicSource.clip = newClip;
        nextMusicSource.volume = 0f;
        nextMusicSource.Play();

        // Stop các tween cũ nếu có
        currentMusicSource.DOKill();
        nextMusicSource.DOKill();

        // Crossfade
        currentMusicSource.DOFade(0f, fadeDuration);
        nextMusicSource.DOFade(targetVolume, fadeDuration)
            .OnComplete(() =>
            {
                currentMusicSource.Stop();
                currentMusicSource.volume = 0f;

                // Hoán đổi
                (nextMusicSource, currentMusicSource) = (currentMusicSource, nextMusicSource);
            });
    }
    
    
    private void PlayFx(object[] data)
    {
        FxAudioDataSO fxSO = (FxAudioDataSO)data[0];
        if (fxSO == null || fxSO.VersionsList.Count == 0) return; // Kiểm tra âm thanh có hợp lệ không
        
        AudioSource source;

        //Cấp phát nguồn âm từ pool
        if(FxSourcePool.Count > 0){
            source = FxSourcePool.Pop();
            source.gameObject.SetActive(true);

        } else { //Tạo mới nếu ko còn audiosource nào rảnh

            GameObject newFx = new("FxSource");
            newFx.transform.SetParent(FxSourceRoot.transform); // Gắn vào Root
            source = newFx.AddComponent<AudioSource>();

            //Đặt âm lượng cho source mới tạo 
            source.volume = GameManager.Instance.PlayerDataManager.PlayerData.SFXVolume;

            allFxSources.Add(source);
        }

        source.clip = fxSO.VersionsList[Random.Range(0, fxSO.VersionsList.Count)];
        source.pitch = Random.Range(fxSO.MinPitch, fxSO.MaxPitch);

        source.Play();
        StartCoroutine(ReleaseFxSource(source));
    }

    //Tra lai nguồn âm thanh về pool
    private IEnumerator ReleaseFxSource(AudioSource source)
    {
        if (source.clip != null)
        {
            yield return new WaitForSecondsRealtime(source.clip.length);
        }
        source.Stop();
        source.clip = null;
        FxSourcePool.Push(source);
        source.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        //Hủy đăng ký sự kiện
        Observer.RemoveListener(EvenID.ChangeMusic, ChangeMusic);
        Observer.RemoveListener(EvenID.PlayFX, PlayFx);
    }

}
