// Writen by Lukasz Dziedziczak
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Health health;
    private Canvas canvas;
    [SerializeField] RectTransform foreground;

    private void Awake()
    {
        health = GetComponentInParent<Health>();
        canvas = GetComponent<Canvas>();
    }

    private void Update()
    {
        if (canvas == null || health == null || foreground == null) return;

        if (Mathf.Approximately(health.HealthPercentage(), 0) || Mathf.Approximately(health.HealthPercentage(), 1))
        {
            canvas.enabled = false;
        }
        else
        {
            canvas.enabled = true;
        }

        foreground.localScale = new Vector3(health.HealthPercentage(), 1.0f, 1.0f);


    }

    private void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;
    }
}
// Writen by Lukasz Dziedziczak