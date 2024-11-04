using UnityEngine;
using UnityEngine.UI;

public class LifeDisplay : MonoBehaviour
{
    [SerializeField] private Image _filledBar;
    [SerializeField] private LifeManager _lifeManager;

    private void Awake()
    {
        _lifeManager.OnLifeChanged.AddListener(OnLifeChanged);
    }

    private void OnLifeChanged(float life)
    {
        _filledBar.fillAmount = _lifeManager.LifePercent;
    }
}