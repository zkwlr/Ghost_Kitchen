using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAttackSound : MonoBehaviour
{
    public AudioClip attackSound;
    private AudioSource audioSource;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void PlayAttackSound()
    {
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }
}
