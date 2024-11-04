using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyBullet01
{
    public class Bullet : MonoBehaviour
    {
        public float projectileSpeed = 5000f;
        public float lifetime = 2f; // Durée de vie du projectile, en secondes
        [SerializeField] private float damageAmount = 10f; // Dégâts infligés par la balle


        private void Start()
        {
            // Détruit le projectile automatiquement après 'lifetime' secondes pour éviter les accumulations inutiles
            Destroy(gameObject, lifetime);

               

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

            if (lifeManager != null && collision.gameObject.tag != "Player")
            {
                lifeManager.TakeDamage(damageAmount); // Applique les dégâts
                Debug.Log("impact");
            }

            Destroy(gameObject);
        }

        public void Initialize(Vector3 targetPoint)
        {
            // Orientation et vitesse
            transform.LookAt(targetPoint);
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = (targetPoint - transform.position).normalized * projectileSpeed;
        }
    }
}
