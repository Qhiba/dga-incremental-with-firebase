using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapText : MonoBehaviour
{
    public float spawnTimer = 0.5f;
    public Text showedText;

    private float _spawnTime;

    private void OnEnable()
    {
        _spawnTime = spawnTimer;
    }

    // Update is called once per frame
    void Update()
    {
        _spawnTime -= Time.unscaledDeltaTime;
        if (_spawnTime <= 0f)
        {
            gameObject.SetActive(false);
        }
        else
        {
            showedText.CrossFadeAlpha(0f, 0.5f, false);
            if (showedText.color.a == 0f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
