using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIndicator : MonoBehaviour
{
    MeshRenderer meshRenderer;
    Vector3 orginalScale;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        orginalScale = transform.localScale;
    }

    private void Start()
    {
        SetSize();
    }

    public void SetColor(Color newColor)
    {
        meshRenderer.material.color = newColor;
    }

    public void SetSize()
    {
        GameCameraController gameCameraController = FindObjectOfType<GameCameraController>();
        float newSize = gameCameraController.MapSize() / 500;
        transform.localScale = orginalScale + new Vector3(newSize, 1, newSize);
    }
}
