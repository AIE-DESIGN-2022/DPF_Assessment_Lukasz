using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MiniMap : MonoBehaviour
{
    [SerializeField] private Camera miniMapCamera;
    [SerializeField] private GameCameraController gameCamera;
    [SerializeField] private float offsetX = 10;
    [SerializeField] private float offsetZ = 10;
    private LayerMask terrainMask;

    private void Awake()
    {
        terrainMask |= (1 << LayerMask.NameToLayer("Terrain"));
    }

    // Update is called once per frame
    void Update()
    {
        if (PointerIsOverUIObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 newPosition = new Vector3();
                Ray ray = miniMapCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000,  terrainMask))
                {
                    newPosition = hit.point;
                    newPosition.x += offsetX;
                    newPosition.z += offsetZ;
                    gameCamera.MoveTo(newPosition);
                }
                
            }
        }
    }

    private bool PointerIsOverUIObject()
    {
        PointerEventData EventDataCurrentPosition = new PointerEventData(EventSystem.current);
        EventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> result = new List<RaycastResult>();
        EventSystem.current.RaycastAll(EventDataCurrentPosition, result);
        return result.Count > 0;
    }
}
