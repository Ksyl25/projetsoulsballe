using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LifeManager : MonoBehaviour
{
    [SerializeField] private float _maxLife = 100f;
    [SerializeField] private UnityEvent<float> _onLifeChanged = new UnityEvent<float>();
    [SerializeField] private UnityEvent<float, float> _onTakeDamage = new UnityEvent<float, float>();
    [SerializeField] private UnityEvent<float, float> _onHeal = new UnityEvent<float, float>();
    [SerializeField] private UnityEvent _onDie = new UnityEvent();
    private float _currentLife = 0f;

    public float LifePercent => _currentLife / _maxLife;

    [ContextMenu("TestDamage")]
    public void TestDamage()
    {
        TakeDamage(20f);
    }

    public UnityEvent<float> OnLifeChanged => _onLifeChanged;
    public UnityEvent<float, float> OnTakeDamage => _onTakeDamage;
    public UnityEvent<float, float> OnHeal => _onHeal;
    public UnityEvent OnDie => _onDie;

    public void TakeDamage(float amount)
    {
        ChangeLife(-amount);
        _onTakeDamage.Invoke(amount, _currentLife);
    }

    public void Heal(float amount)
    {
        ChangeLife(amount);
        _onHeal.Invoke(amount, _currentLife);
    }

    private void Awake()
    {
        _currentLife = _maxLife;
    }

    private void ChangeLife(float amount)
    {
        _currentLife += amount;
        _onLifeChanged.Invoke(_currentLife);

        CheckIfDead();
    }

    private void CheckIfDead()
    {
        if (_currentLife > 0)
            return;

        Die();
    }

    private void Die()
    {
        _onDie.Invoke();
        Debug.Log("joueur mort");
        //Destroy(gameObject);
    }
}