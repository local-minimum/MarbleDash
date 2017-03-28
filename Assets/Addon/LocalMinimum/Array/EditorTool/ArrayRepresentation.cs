using UnityEngine;

namespace LocalMinimum.Boolean.Editor {

    public class ArrayRepresentation : MonoBehaviour {

        public Color colorZero = Color.black;
        public Color colorOne = Color.white;
        public Color colorMinusOne = Color.magenta;

        public int width = 20;
        public int height = 20;

        public float spacing = 1f;
        public Vector3 itemScale = Vector3.one;

        int[,] intRepresentation;
        bool[,] boolRepresentation;
        ArrayItemRepresentation[,] itemRepresentation;
        bool boolIsActive = true;

        private void Awake()
        {
            Generate();
        }

        public void Clicked(int x, int y)
        {
            if (!boolIsActive)
            {
                boolRepresentation[x, y] = !boolRepresentation[x, y];
                boolIsActive = true;
                UpdateEveryone();
            }
            else           
            {
                boolRepresentation[x, y] = !boolRepresentation[x, y];
                itemRepresentation[x, y].SetColor(Color.Lerp(colorZero, colorOne, boolRepresentation[x, y] ? 1 : 0));
            }
        }

        public void Generate()
        {
            itemRepresentation = new ArrayItemRepresentation[width, height];
            boolRepresentation = new bool[width, height];
            intRepresentation = new int[width, height];


            ArrayItemRepresentation[] existing = GetComponentsInChildren<ArrayItemRepresentation>();
            int nExisting = existing.Length;
            int index = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++, index++)
                {
                    ArrayItemRepresentation current;
                    if (index < nExisting)
                    {
                        current = existing[index];
                    } else
                    {
                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube); //new GameObject("", typeof(MeshRenderer), typeof(MeshFilter), typeof(BoxCollider));
                        go.transform.SetParent(transform);
                        current = go.AddComponent<ArrayItemRepresentation>();
                        current.arrayRep = this;
                    }

                    current.transform.localScale = itemScale;
                    current.transform.localPosition = new Vector3((x - width / 2f) * spacing, 0, (y - height / 2f) * spacing);
                    current.name = string.Format("Element ({0}, {1})", x, y);
                    current.SetPosition(x, y);
                    itemRepresentation[x, y] = current;
                }
            }

            while (index < nExisting)
            {
                Destroy(existing[index]);
                index++;
            }

            UpdateEveryone();
        }

        public void Fill(int value)
        {
            intRepresentation = intRepresentation.Fill(boolRepresentation, value);
            boolIsActive = false;
            UpdateEveryone();
        }

        public void Edge(bool borderAsEdge)
        {
            boolRepresentation = boolRepresentation.Edge(borderAsEdge);
            boolIsActive = true;
            UpdateEveryone();
        }

        public void DistanceToEdge(bool borderAsEdge)
        {
            intRepresentation = boolRepresentation.DistanceToEgde(borderAsEdge);
            boolIsActive = false;
            UpdateEveryone();
        }

        public void Invert()
        {
            boolRepresentation = boolRepresentation.Invert();
            boolIsActive = true;
            UpdateEveryone();
        }

        public void Label()
        {
            int labels;
            intRepresentation = boolRepresentation.Label(out labels);
            boolIsActive = false;
            UpdateEveryone();
        }

        void UpdateEveryone()
        {
            int max = intRepresentation.Max();
            int min = Mathf.Max(0, intRepresentation.Min());
            float span = Mathf.Max(1, max - min);

            for (int x=0; x<width; x++)
            {
                for (int y=0; y<height; y++)
                {
                    itemRepresentation[x, y].SetColor(boolIsActive ? 
                        Color.Lerp(colorZero, colorOne, boolRepresentation[x, y] ? 1f : 0f) :
                        intRepresentation[x, y] < 0 ? colorMinusOne : Color.Lerp(colorZero, colorOne, (intRepresentation[x, y] - min) / span));
                }
            }

        }
    }
}