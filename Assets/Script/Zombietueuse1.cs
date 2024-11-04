using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombietueuse : MonoBehaviour
{
    public float speed = 3f;
    public int damage = 10;
    private Transform player;
    public Animator animator;
    public float rotationSpeed = 5.0f;
    private Rigidbody rb;
    private LifeManager lifeManager; // Référence au LifeManager
    private bool isDead = false; // Indicateur de mort
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        lifeManager = GetComponent<LifeManager>();

       /* if (lifeManager != null)
        {
            lifeManager.OnDie.AddListener(OnDeath);
        }
        */
        
    }

    void Update()
    {
        if (player != null && !isDead)
        {
            animator.SetTrigger("manger");
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //int attackChoice = Random.Range(0, 2);
             //animator.SetTrigger(attackChoice == 0 ? "attack1" : "attack2");
            Debug.Log("colisionplayer");
            animator.SetTrigger("attack2");

             LifeManager playerLife = collision.gameObject.GetComponent<LifeManager>();
            if (playerLife != null)
            {
                playerLife.TakeDamage(damage);
            }
        }
        else if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("bullet collision");
            animator.SetTrigger("hit");
        }
    }

    private void OnDeath()
    {
        isDead = true;
        animator.SetTrigger("mort");
        //rb.isKinematic = true;
        this.enabled = false; // Désactive le script pour éviter tout mouvement supplémentaire
    }
}
