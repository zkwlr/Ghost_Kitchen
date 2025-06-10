using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class SkewerThrowSound : MonoBehaviour
{
    public AudioClip throwSound;
    public float minThrowVelocity = 1.0f; // 이 속도 이상이면 '던짐'으로 간주

    private AudioSource audioSource;
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        if (grabInteractable != null)
            grabInteractable.selectExited.AddListener(OnReleased);
    }

    void OnDisable()
    {
        if (grabInteractable != null)
            grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (rb != null && rb.velocity.magnitude >= minThrowVelocity)
        {
            if (throwSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(throwSound);
            }
        }
    }
}
