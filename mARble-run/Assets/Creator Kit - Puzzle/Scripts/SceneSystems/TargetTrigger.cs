using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider)), RequireComponent(typeof(AudioSource))]
public class TargetTrigger : MonoBehaviour
{
    public ParticleSystem completeParticleSystem;

    AudioSource m_AudioSource;

    void Awake ()
    {
        m_AudioSource = GetComponent<AudioSource> ();
    }

    void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Marble")
        {
            completeParticleSystem.Play();
            m_AudioSource.PlayOneShot (m_AudioSource.clip);
        }
    }
}
