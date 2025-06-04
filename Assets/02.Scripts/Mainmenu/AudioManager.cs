using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    private void Awake()
    {
        // 싱글톤 패턴: 이미 오브젝트가 존재하면 중복 방지
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // 이 오브젝트를 씬 전환 시에도 파괴하지 않음
        DontDestroyOnLoad(gameObject);

        // AudioSource에 이미 할당된 AudioClip을 한 번 재생
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}