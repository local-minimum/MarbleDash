using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatTextManager : MonoBehaviour {
   
    static FloatTextManager _instance;

    static FloatTextManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<FloatTextManager>();
            }
            return _instance;
        }
    }


    private void Awake()
    {
        if (_instance == null || _instance == this)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    [SerializeField]
    Camera viewCam;

    [SerializeField]
    RectTransform canvas;

    [SerializeField]
    FloatText prefab;

    public static Vector2 GetCanvasPos(Vector3 worldPos)
    {
        return   instance.viewCam.WorldToScreenPoint(worldPos);        
    }

    static List<FloatText> pool = new List<FloatText>();

    public static void AddToPool(FloatText fText)
    {
        pool.Add(fText);
    }

    public static void ShowText(Transform position, string text)
    {
        FloatText ft = GetText();
        ft.ShowText(position, text);
    }

    static FloatText GetText()
    {
        if (pool.Count > 0)
        {
            FloatText ft = pool[0];
            pool.RemoveAt(0);
            return ft;
        } else
        {
            return Instantiate(instance.prefab, instance.canvas, false);           
        }

    }
}
