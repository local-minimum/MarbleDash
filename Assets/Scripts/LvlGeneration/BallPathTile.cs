using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using LocalMinimum.Grid;

public class BallPathTile : MonoBehaviour {

    MeshFilter mf;
    Mesh m;

    [SerializeField]
    Material startMat;

    [SerializeField]
    Material endMat;

    [SerializeField]
    Material pathMat;

    ParticleSystem partSys;

    private void Awake()
    {
        mf = GetComponent<MeshFilter>();
        m = new Mesh();
        m.name = "ProcGen path tile";
        mf.sharedMesh = m;
        partSys = GetComponentInChildren<ParticleSystem>();
    }

    GridPos pos;
    Vector2 size;
    bool playerVisited = false;

    [SerializeField, Range(0, 1)]
    float middleWidth = 0.5f;

    public void SetPosition(BoardGrid board, GridPos pos)
    {
        foreshadowed = false;
        playerVisited = false;
        this.pos = pos;
        size = board.TileShape;
        transform.localPosition = board.GetLocalPosition(pos);
    }

    BallPathTile nextTile;
    BallPathTile previousTile;

    public void SetNextTile(BallPathTile next)
    {
        nextTile = next;
        next.SetPreviousTile(this);
    }

    void SetPreviousTile(BallPathTile prev)
    {
        previousTile = prev;
    }

    Material activeMat;
    Color refColor;
    [SerializeField]
    Color offColor;

    public void GenerateMesh()
    {
        float halfX = size.x / 2f;
        float halfZ = size.y / 2f;

        Direction entry = Direction.None;
        Direction exit = Direction.None;
        if (nextTile) {
            exit = (nextTile.pos - pos).AsMajorDirection();
        }
        if (previousTile)
        {
            entry = (previousTile.pos - pos).AsMajorDirection();
        }

        activeMat = Instantiate( (previousTile == null) ? startMat : exit == Direction.None ? endMat : pathMat);
        GetComponent<MeshRenderer>().material = activeMat;
        refColor = activeMat.color;
        m.Clear();

        if (previousTile == null)
        {            
            GenerateStart(halfX, halfZ, exit);
        } else if (nextTile == null)
        {
            GenerateEnd(halfX, halfZ, entry);
            activeMat.color = offColor;
        } else
        {
            GenerateMiddle(halfX, halfZ, entry, exit);
            activeMat.color = offColor;
        }
        m.RecalculateNormals();
        m.RecalculateBounds();
    }

    void GenerateEnd(float halfX, float halfZ, Direction entry)
    {
        GenerateStart(halfX, halfZ, entry);
    }

    void GenerateMiddle(float halfX, float halfZ, Direction entry, Direction exit)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();


        Vector2 halfSizeInner = size * middleWidth / 2f;
        Vector2 halfSizeOuter = size / 2f;
        Vector2 uvCenter = Vector2.one * middleWidth;
        Vector2 uvEdges = Vector2.one - uvCenter;

        verts.Add(new Vector3(-halfSizeInner.x, halfSizeInner.y, 0));       // 0
        verts.Add(new Vector3(halfSizeInner.x, halfSizeInner.y, 0));        // 1
        verts.Add(new Vector3(halfSizeInner.x, -halfSizeInner.y, 0));       // 2
        verts.Add(new Vector3(-halfSizeInner.x, -halfSizeInner.y, 0));      // 3

        uvs.Add(new Vector2(uvEdges.x, uvEdges.y + uvCenter.y));
        uvs.Add(new Vector2(uvEdges.x + uvCenter.x, uvEdges.y + uvCenter.y));
        uvs.Add(new Vector2(uvEdges.x + uvCenter.x, uvEdges.y));
        uvs.Add(new Vector2(uvEdges.x, uvEdges.y));

        tris.AddRange(new int[]
        {
                0, 3, 1,
                1, 3, 2,
        });

        int off = 4;

        if (entry == Direction.West || exit == Direction.West)
        {
            //Debug.Log(name + " West");

            verts.Add(new Vector3(-halfSizeOuter.x, -halfSizeInner.y));     // off + 0
            verts.Add(new Vector3(-halfSizeOuter.x, halfSizeInner.y));      // off + 1

            uvs.Add(new Vector2(0, uvEdges.y));
            uvs.Add(new Vector2(0, uvEdges.y + uvCenter.y));
            
            
            tris.AddRange(new int[]
            {
                3, 0, off + 1,
                3, off + 1, off + 0
            });
            
            off += 2;
        }

        if (entry == Direction.East || exit == Direction.East)
        {
            //Debug.Log(name + " East");
            verts.Add(new Vector3(+halfSizeOuter.x, -halfSizeInner.y));     // off + 0
            verts.Add(new Vector3(+halfSizeOuter.x, halfSizeInner.y));      // off + 1

            uvs.Add(new Vector2(1, uvEdges.y));
            uvs.Add(new Vector2(1, uvEdges.y + uvCenter.y));
            
            tris.AddRange(new int[]
            {
                1, 2, off + 1,
                2, off + 0, off + 1
            });

            off += 2;
        }

        if (entry == Direction.North || exit == Direction.North)
        {
            //Debug.Log(name + " North");
            verts.Add(new Vector3(-halfSizeInner.x, halfSizeOuter.y));     // off + 0
            verts.Add(new Vector3(halfSizeInner.x, halfSizeOuter.y));      // off + 1

            uvs.Add(new Vector2(uvEdges.x, 1));
            uvs.Add(new Vector2(uvEdges.x + uvCenter.x, 1));
            
            tris.AddRange(new int[]
            {
                0, 1, off + 1,
                0, off + 1, off + 0
            });
            
            off += 2;
        }

        if (entry == Direction.South || exit == Direction.South)
        {
            //Debug.Log(name + " South");

            verts.Add(new Vector3(-halfSizeInner.x, -halfSizeOuter.y));     // off + 0
            verts.Add(new Vector3(halfSizeInner.x, -halfSizeOuter.y));      // off + 1

            uvs.Add(new Vector2(uvEdges.x, 0));
            uvs.Add(new Vector2(uvEdges.x + uvCenter.x, 0));

            tris.AddRange(new int[]
            {
                2, 3, off + 0,
                2, off + 0, off + 1
            });

            off += 2;
        }

        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m.SetUVs(0, uvs);
    }

    void GenerateStart(float halfX, float halfZ, Direction exit)
    {
        m.vertices = new Vector3[]
        {
                new Vector3(-halfX, halfZ),
                new Vector3(halfX, halfZ),
                new Vector3(halfX, -halfZ),
                new Vector3(-halfX, -halfZ),

        };
        m.triangles = new int[]
        {
                0, 3, 1,
                1, 3, 2,
        };

        if (exit == Direction.South)
        {
            m.uv = new Vector2[]
            {
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                    new Vector2(0, 0)
            };
        }
        else if (exit == Direction.North)
        {
            m.uv = new Vector2[]
            {
                    new Vector2(1, 0),
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
            };
        }
        else if (exit == Direction.West)
        {
            m.uv = new Vector2[]
            {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0)

            };
        }
        else if (exit == Direction.East || true)
        {
            m.uv = new Vector2[]
            {
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                    new Vector2(0, 0),
                    new Vector2(0, 1)

            };
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!playerVisited && other.tag == "Player")
        {
            PlayerController pCtrl = other.GetComponent<PlayerController>();
            if (pCtrl != null && (previousTile == null || previousTile.foreshadowed))
            {
                foreshadowed = true;
                activeMat.color = refColor;
                PlayerRunData.stats.Coin += 1;

                if (nextTile)
                {
                    nextTile.Foreshadow(1, foreshadowDecay);
                } else
                {
                    LevelCompleted(other.transform);
                }

                playerVisited = true;
            }
        }
    }

    void LevelCompleted(Transform playerT)
    {
        if (playerVisited)
        {
            return;
        }

        playerVisited = true;
        StartCoroutine(NewLevel(playerT));
    }

    [SerializeField]
    int levelCompleteBonus = 60;

    IEnumerator<WaitForSeconds> NewLevel(Transform playerT)
    {
        float progress = 0f;
        float delta = 0.016f;
        Vector3 localStart = transform.InverseTransformPoint(playerT.position);
        Vector3 firstTarget = new Vector3(0, 0, 0.5f);
        float centerTime = (localStart - firstTarget).magnitude * 0.5f;
        CoinFountains.instance.ShowerMe(transform);
        PlayerRunData.stats.Coin += levelCompleteBonus;
        while (progress < centerTime)
        {
            progress += delta;
            playerT.position = transform.TransformPoint(Vector3.Lerp(localStart, firstTarget, progress / centerTime));
            yield return new WaitForSeconds(delta);
        }


        playerT.position = transform.TransformPoint(firstTarget);

        partSys.Play();
        yield return new WaitForSeconds(0.15f);

        progress = 0;
        float launchTime = 1.5f;
        Vector3 secondTarget = new Vector3(0, 0, 30);
        while (progress < launchTime)
        {
            progress += delta;
            playerT.position = transform.TransformPoint(Vector3.Lerp(firstTarget, secondTarget, progress / launchTime));
            yield return new WaitForSeconds(delta);
        }
        MeshRenderer playerRend = playerT.GetComponent<MeshRenderer>();
        ShadowCastingMode onShadows = playerRend.shadowCastingMode;
        playerRend.shadowCastingMode = ShadowCastingMode.Off;
        yield return new WaitForSeconds(delta);
        Level.instance.StopTheMotion();
        PlayerRunData.stats.NextLevel();
        Level.instance.Generate();
        StoreSwapper.instance.HideAllStores();
        Level.instance.Implement();
        yield return new WaitForSeconds(0.5f);
        playerRend.shadowCastingMode = onShadows;
        yield return new WaitForSeconds(delta);
        Level.instance.StartTheMotion();
    }

    [SerializeField, Range(0, 1)]
    float foreshadowDecay = 0.125f;

    [SerializeField, Range(0, 1)]
    float foreshadowFactor = 0.4f;

    [SerializeField, Range(0, 1)]
    float foreshadowTrunctation = 0.25f;

    bool foreshadowed = false;

    void Foreshadow(float progress, float step)
    {
        if (activeMat == null || playerVisited)
        {
            if (!playerVisited) {
                Debug.Log(string.Format("There's no active material here ({0}) so can't apply color change", name));
            }
            return;
        }

        progress = Mathf.Clamp01(progress - step);
        if (progress > foreshadowTrunctation)
        {
            activeMat.color = Color.Lerp(offColor, refColor, progress * foreshadowFactor);

            foreshadowed = true;

            if (progress > 0 && nextTile)
            {
                nextTile.Foreshadow(progress, step);
            }
            else if (progress > 0)
            {
                Debug.Log("Reached end of path at " + name);
            }
        }
    }
}
