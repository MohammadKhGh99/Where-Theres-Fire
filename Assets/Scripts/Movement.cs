using UnityEngine;
using UnityEngine.Tilemaps;

public class Movement : MonoBehaviour
{
    public Tilemap tilemap;
    public Vector3Int gridSize; // size of the grid in grid cells
    public float gridMoveDuration; // time it takes to move from one grid cell to the next
    public bool moveByGrid = true;

    private Vector3Int currentGridPos; // current position of the object in grid cells
    private float gridMoveTimer; // timer for moving from one grid cell to the next
    private Vector3 _moveDirection;
    private Vector2 _lookAtDirection;

    void Start()
    {
        // Initialize the current grid position to the position of the object in world space
        currentGridPos = tilemap.WorldToCell(transform.position);
        _lookAtDirection = Vector3.right;
    }

    void Update()
    {
        if(!moveByGrid)
            return;
        // Update the timer
        gridMoveTimer += Time.deltaTime;

        // Check if it's time to move to the next grid cell
        if (gridMoveTimer >= gridMoveDuration)
        {
            // Reset the timer
            gridMoveTimer = 0;

            // Check input and move in the corresponding direction
            if (Input.GetKeyDown(KeyCode.W))// && currentGridPos.y < gridSize.y - 1)
            {
                currentGridPos.y++;
                _moveDirection.x = 0;
                _moveDirection.y = 1;

                var snapping = 90.0f;
                if (_moveDirection.sqrMagnitude > 0)
                {
                    var angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
                    angle = Mathf.Round(angle / snapping) * snapping;
                    transform.rotation = Quaternion.AngleAxis(90 + angle, Vector3.forward);
                    _moveDirection = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
                    _lookAtDirection = _moveDirection;
                }
            }
            else if (Input.GetKeyDown(KeyCode.S))// && currentGridPos.y > 0)
            {
                currentGridPos.y--;
            }
            else if (Input.GetKeyDown(KeyCode.D))// && currentGridPos.x < gridSize.x - 1)
            {
                currentGridPos.x++;
            }
            else if (Input.GetKeyDown(KeyCode.A))// && currentGridPos.x > 0)
            {
                currentGridPos.x--;
            }

            // Update the position of the object in world space
            transform.position = tilemap.GetCellCenterWorld(currentGridPos);
        }
    }
}