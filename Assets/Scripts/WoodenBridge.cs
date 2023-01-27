using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodenBridge : MonoBehaviour
{
    [SerializeField] private LayerMask otherLayers;
    [SerializeField] private LayerMask woodenLayer;
    
    private Flammable _flammable;
    private bool _ignoreStatus;

    // Start is called before the first frame update
    void Start()
    {
        _flammable = GetComponent<Flammable>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (_flammable.CurrentStatus)
        {
            case Flammable.Status.NotOnFire when _ignoreStatus.Equals(false):
                _ignoreStatus = true;
                Physics2D.IgnoreLayerCollision(14, 7, _ignoreStatus);
                Physics2D.IgnoreLayerCollision(14, 10, _ignoreStatus);
                break;
            case Flammable.Status.OnFire or Flammable.Status.FinishedBurning when _ignoreStatus.Equals(true):
                _ignoreStatus = false;
                Physics2D.IgnoreLayerCollision(14, 7, false);
                Physics2D.IgnoreLayerCollision(14, 10, false);
                break;
        }
    }
}
