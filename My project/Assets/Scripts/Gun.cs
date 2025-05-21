using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    public float damage = 25f;         // Damage per shot
    public float range = 100f;         // Shooting range
    public float fireRate = 0.2f;      // Fire rate (seconds per shot)
    public int maxAmmo = 10;           // Magazine size
    private int currentAmmo = 9;           // Current bullets in mag
    private bool isReloading = false;  // Reload state

    [Header("References")]
    public Camera fpsCam;              // Camera for raycast direction
    public ParticleSystem muzzleFlash; // Muzzle effect
    public GameObject impactEffect;    // Bullet impact effect
    public Animator animator;          // Animator component

    private float nextTimeToFire = 0f; // Fire rate control

    void Start()
    {
        currentAmmo = maxAmmo; // Set ammo at start
    }

    void Update()
    {
        if (isReloading)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            Console.WriteLine("uguy");
            StartCoroutine(Reload());
            IEnumerator Reload()
            {
                isReloading = true;
                animator.Play("Reload");
                yield return new WaitForSeconds(1.5f); // Adjust to match reload animation length
                currentAmmo = maxAmmo; // Refill ammo
                isReloading = false;
            }
        }
    }

    void Shoot()
    {
        animator.Play("Fire");  // Play shoot animation
        muzzleFlash.Play();            // Play muzzle flash
        currentAmmo -= 1;                 // Decrease ammo

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log("Hit: " + hit.transform.name);

            // Apply damage if target has a health script
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                //target.TakeDamage(damage);
            }

            // Spawn impact effect
            Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }

   
}


