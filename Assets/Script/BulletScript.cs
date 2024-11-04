using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Mybullet // Remplacez par votre propre nom de namespace
{
    public class BulletScript : MonoBehaviour

    {
        public float projectileSpeed = 20f;
        public float lifetime = 2f; // Durée de vie du projectile, en secondes

        private void Start()
        {
            // Détruit le projectile automatiquement après 'lifetime' secondes pour éviter les accumulations inutiles
            Destroy(gameObject, lifetime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Détruit le projectile dès qu'il entre en collision avec un autre objet
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
