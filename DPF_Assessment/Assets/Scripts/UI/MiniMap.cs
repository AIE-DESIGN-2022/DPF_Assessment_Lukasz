using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MiniMap : MonoBehaviour
{
    [Tooltip("The Mini-Map Camera object")]
    [SerializeField] private Camera miniMapCamera;
    [Tooltip("The Game Camera Controller object")]
    [SerializeField] private GameCameraController gameCamera;
    [Tooltip("The X axis offset when main camera is moved by mouse click on minimap.")]
    [SerializeField] private float offsetX = 10;
    [Tooltip("The Z axis offset when main camera is moved by mouse click on minimap.")]
    [SerializeField] private float offsetZ = 10;

    // Layer Mask which only has the terrain layer.
    private LayerMask terrainMask;

    // Run at the start of the game
    private void Awake()
    {
        terrainMask |= (1 << LayerMask.NameToLayer("Terrain"));
    }

    // Update is called once per frame
    void Update()
    {
        if (PointerIsOverUIObject())
        {
            if (Input.GetMouseButtonDown(0)) // when mini map is clicked raycast down and find where it intersect terrain.
            {
                Vector3 newPosition = new Vector3();
                Ray ray = miniMapCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000,  terrainMask))
                {
                    newPosition = hit.point;
                    newPosition.x += offsetX;
                    newPosition.z += offsetZ;
                    gameCamera.MoveTo(newPosition); // after adding offset move to this location.
                }
                
            }
        }
    }

    /* Determins if mouse pointer is over the UI instead of in the game. True if in UI */
    private bool PointerIsOverUIObject()
    {
        PointerEventData EventDataCurrentPosition = new PointerEventData(EventSystem.current);
        EventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> result = new List<RaycastResult>();
        EventSystem.current.RaycastAll(EventDataCurrentPosition, result);
        return result.Count > 0;
    }
}
