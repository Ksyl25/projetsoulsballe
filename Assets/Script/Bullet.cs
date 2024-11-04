using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyBullet01
{
    public class Bullet : MonoBehaviour
    {
        public float projectileSpeed = 5000f;
        public float lifetime = 2f; // Dur�e de vie du projectile, en secondes
        [SerializeField] private float damageAmount = 10f; // D�g�ts inflig�s par la balle


        private void Start()
        {
            // D�truit le projectile automatiquement apr�s 'lifetime' secondes pour �viter les accumulations inutiles
            Destroy(gameObject, lifetime);

               

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

            if (lifeManager != null && collision.gameObject.tag != "Player")
            {
                lifeManager.TakeDamage(damageAmount); // Applique les d�g�ts
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
