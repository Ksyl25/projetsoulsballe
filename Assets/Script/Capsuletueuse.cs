using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Capsuletueuse : MonoBehaviour
{
    public float speed = 3f; // Vitesse de d�placement de la capsule
    public int damage = 10; // D�g�ts inflig�s au joueur
    private Transform player; // R�f�rence au joueur

    void Start()
    {
        // Trouver le joueur par son tag
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // Si le joueur est trouv�, poursuivre
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        // D�truit le projectile d�s qu'il entre en collision avec un autre objet
        LifeManager lifeManager = collision.gameObject.GetComponent<LifeManager>();

        Debug.Log(lifeManager);

        if (lifeManager == null)
        {
            Debug.LogWarning("LifeManager non trouv� sur " + collision.gameObject.name);
            return;
        }

        if (lifeManager != null)
        {
            lifeManager.TakeDamage(10f); // Applique les d�g�ts
            Debug.Log("impact");
        }
    }
}
