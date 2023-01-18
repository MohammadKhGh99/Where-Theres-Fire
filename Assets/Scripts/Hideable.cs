using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hideable : MonoBehaviour
{
    [SerializeField] private bool startInvisible;
    private SpriteRenderer _spriteRenderer;
    private const KeyCode Hide = KeyCode.R;

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (startInvisible)
            _spriteRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // ** make Invisible **
        if (Input.GetKeyDown(Hide))
            ShowOrHide();
    }

    public void ShowOrHide(bool reShow = false)
    {
        if (reShow)
        {
            if (!_spriteRenderer.enabled)
                StartCoroutine(GameManager.Instance.FadeIn(_spriteRenderer));
            return;
        }
        
        StartCoroutine(_spriteRenderer.enabled
            ? GameManager.Instance.FadeOut(_spriteRenderer)
            : GameManager.Instance.FadeIn(_spriteRenderer));
    }
}
