using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Capsuletueuse : MonoBehaviour
{
    public float speed = 3f; // Vitesse de déplacement de la capsule
    public int damage = 10; // Dégâts infligés au joueur
    private Transform player; // Référence au joueur

    void Start()
    {
        // Trouver le joueur par son tag
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // Si le joueur est trouvé, poursuivre
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Détruit le projectile dès qu'il entre en collision avec un autre objet
        LifeManager lifeManager = collision.gameObject.GetComponent<LifeManager>();

        Debug.Log(lifeManager);

        if (lifeManager == null)
        {
            Debug.LogWarning("LifeManager non trouvé sur " + collision.gameObject.name);
            return;
        }

        if (lifeManager != null)
        {
            lifeManager.TakeDamage(10f); // Applique les dégâts
            Debug.Log("impact");
        }
    }
}
