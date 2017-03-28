using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LocalMinimum.Arrays.Editor
{


    [ExecuteInEditMode]
    public class ArrayItemRepresentation : MonoBehaviour
    {

        int x;
        int y;
            
        Renderer r;

        [HideInInspector]
        public ArrayRepresentation arrayRep;

        private void Awake()
        {
            r = GetComponent<Renderer>();
        }

        public void SetPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void SetColor(Color c)
        {
            if (r == null)
            {
                Awake();
            }
            r.material.color = c;
        }

        bool hovered = false;

        private void OnMouseEnter()
        {
            hovered = true;
        }

        private void OnMouseExit()
        {
            hovered = false;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && hovered)
            {
                arrayRep.Clicked(x, y);
            }
        }
    }

    
}