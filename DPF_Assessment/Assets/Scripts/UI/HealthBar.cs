using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Health _health;
    private Canvas _canvas;
    [SerializeField] RectTransform _foreground;

    private void Awake()
    {
        _health = GetComponentInParent<Health>();
        _canvas = GetComponent<Canvas>();
    }

    private void Update()
    {
        if (_canvas == null || _health == null || _foreground == null) return;

        if (Mathf.Approximately(_health.HealthPercentage(), 0) || Mathf.Approximately(_health.HealthPercentage(), 1))
        {
            _canvas.enabled = false;
        }
        else
        {
            _canvas.enabled = true;
        }

        _foreground.localScale = new Vector3(_health.HealthPercentage(), 1.0f, 1.0f);


    }

    private void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;
    }
}
