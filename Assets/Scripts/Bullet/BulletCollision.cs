using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCollision : MonoBehaviour
{
    private AudioSource myAudioSource;

    [SerializeField]
    [Range(-3f, 3f)]
    float MinPitch;

    [SerializeField]
    [Range(-3, 3f)]
    float MaxPitch;

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
        BulletManager.Instance.RegisterBullet(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        myAudioSource.pitch = Random.Range(MinPitch, MaxPitch);
        if (myAudioSource.pitch == 0.0f) //< if it randoms to 0 it will be silent
            myAudioSource.pitch = 1.0f;
        myAudioSource.Play();

        if (collision.transform.root.CompareTag("Player"))
        {
            collision.transform.root.GetComponentInChildren<PlayerCollision>().HitByBullet();
            KillBullet();
        }
    }

    public void KillBullet()
    {
        Destroy(gameObject);
        BulletManager.Instance.RemoveBullet(this);
    }
}
