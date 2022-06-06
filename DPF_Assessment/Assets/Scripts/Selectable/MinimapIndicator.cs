using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIndicator : MonoBehaviour
{
    MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetColor(Color newColor)
    {
        meshRenderer.material.color = newColor;
    }

    public void SetSize()
    {
        GameCameraController gameCameraController = FindObjectOfType<GameCameraController>();
        float newSize = gameCameraController.MapSize() / 500;
        transform.localScale += new Vector3(newSize, 1, newSize);
    }
}
