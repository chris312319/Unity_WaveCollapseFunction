using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GridGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject p;
    [SerializeField]
    private int radius;
    [SerializeField]
    private int height;
    [SerializeField]
    private float cellSize;
    [SerializeField]
    private float cellHeight;
    [SerializeField]
    private int relaxTimes;
    public ModuleLibrary moduleLibrary;
    [SerializeField]
    public  Material moduleMaterial;
    private Grid grid;
    public List<Slot> slots;
    private WorldMaster worldMaster;
    private WaveFunctionCollapse waveFunctionCollapse;
    public Transform ActiveBall;
    public Transform DeActiveBall;
    public float activeDis;
    public void Awake()
    {
        worldMaster = GetComponentInParent<WorldMaster>();
        waveFunctionCollapse = worldMaster.waveFunctionCollapse;
        grid = new Grid(radius,height,cellSize,cellHeight,relaxTimes);
        moduleLibrary = Instantiate(moduleLibrary);
    }
    public void Update()
    {
        foreach(Vertex vertex in grid.vertices)
        {
            foreach(Vertex_Y vertex_Y in vertex.vertex_Ys)
            {
                if (!vertex_Y.isActive && Vector3.Distance(vertex_Y.worldPosition, ActiveBall.position) <= activeDis && !vertex_Y.isBoundary) vertex_Y.isActive = true;
                if (vertex_Y.isActive && Vector3.Distance(vertex_Y.worldPosition, DeActiveBall.position) <= activeDis) vertex_Y.isActive = false;
            }
        }
        foreach(SubQuad subQuad in grid.subQuads)
        {
            foreach(SubQuad_Cube subQuad_Cube in subQuad.subQuad_Cubes)
            {
                subQuad_Cube.UpdateBit();
                if(subQuad_Cube.pre_bit != subQuad_Cube.bit)
                {
                    UpdateSlot(subQuad_Cube);
                }
            }
        }
    }
    private void UpdateSlot(SubQuad_Cube subQuad_Cube)
    {
        string name = "Slot_" + grid.subQuads.IndexOf(subQuad_Cube.subQuad) + "_" + subQuad_Cube.y;
        GameObject slot_GameObject;
        if (transform.Find(name))
        {
            slot_GameObject = transform.Find(name).gameObject;
        }
        else
        {
            slot_GameObject = null;
        }

        if(slot_GameObject == null)
        {
            if(subQuad_Cube.bit != "00000000" && subQuad_Cube.bit != "11111111")
            {
                slot_GameObject = new GameObject(name, typeof(Slot));
                slot_GameObject.transform.SetParent(transform);
                slot_GameObject.transform.localPosition = subQuad_Cube.centerPosition;
                Slot slot = slot_GameObject.GetComponent<Slot>();
                slot.Initialized(moduleLibrary, subQuad_Cube, moduleMaterial);
                slots.Add(slot);
                slot.UpdateModule(slot.possibleModules[0]);
                waveFunctionCollapse.resetSlots.Add(slot);
                waveFunctionCollapse.cur_collapseSlots.Add(slot);
            }
        }
        else
        {
            Slot slot = slot_GameObject.GetComponent<Slot>();
            if(subQuad_Cube.bit == "00000000" || subQuad_Cube.bit == "11111111")
            {
                slots.Remove(slot);
                if(waveFunctionCollapse.resetSlots.Contains(slot))
                {
                    waveFunctionCollapse.resetSlots.Remove(slot);
                }
                if (waveFunctionCollapse.cur_collapseSlots.Contains(slot))
                {
                    waveFunctionCollapse.cur_collapseSlots.Remove(slot);
                }
                Destroy(slot_GameObject);
                Resources.UnloadUnusedAssets();
            }
            else
            {
                slot.ResetSlot(moduleLibrary);
                if (slot.possibleModules.Count > 0) 
                {
                    slot.UpdateModule(slot.possibleModules[0]);
                    if (!waveFunctionCollapse.resetSlots.Contains(slot))
                    {
                        waveFunctionCollapse.resetSlots.Add(slot);
                    }
                    if (!waveFunctionCollapse.cur_collapseSlots.Contains(slot))
                    {
                        waveFunctionCollapse.cur_collapseSlots.Add(slot);
                    }
                }
                else
                {
                    Debug.Log(slot.subQuad_Cube.bit + " has no module");
                }
            }
        }
    }
    public void ToggleSlot(Vertex_Y vertex_Y)
    {
        vertex_Y.isActive = !vertex_Y.isActive;
        foreach(SubQuad_Cube subQuad_Cube in vertex_Y.subQuad_Cubes)
        {
            subQuad_Cube.UpdateBit();
            UpdateSlot(subQuad_Cube);
        }
    }
    /*private void OnDrawGizmos()
    {
        if(grid != null)
        {
            /*foreach (Vertex_hex vertex in grid.hexes)
            {
                Gizmos.DrawSphere(vertex.coord.worldPosition, 0.3f);             
            }
            Gizmos.color = Color.yellow;
            foreach(Triangle triangle in grid.triangles)
            {
                Gizmos.DrawLine(triangle.a.currentPosition, triangle.b.currentPosition);
                Gizmos.DrawLine(triangle.b.currentPosition, triangle.c.currentPosition);
                Gizmos.DrawLine(triangle.c.currentPosition, triangle.a.currentPosition);
            }
            Gizmos.color = Color.green;
            foreach(Quad quad in grid.quads)
            {
                Gizmos.DrawLine(quad.a.currentPosition, quad.b.currentPosition);
                Gizmos.DrawLine(quad.b.currentPosition, quad.c.currentPosition);
                Gizmos.DrawLine(quad.c.currentPosition, quad.d.currentPosition);
                Gizmos.DrawLine(quad.d.currentPosition, quad.a.currentPosition);
            }
            Gizmos.color = Color.red;          
            foreach (Vertex_mid mid in grid.mids)
            {
                Gizmos.DrawSphere(mid.currentPosition, 0.1f);
            }
            Gizmos.color = Color.blue;
            foreach (Vertex_center center in grid.centers)
            {
                Gizmos.DrawSphere(center.currentPosition, 0.1f);
            }
            Gizmos.color = Color.white;
            foreach (SubQuad subquad in grid.subQuads)
            {
                Gizmos.DrawLine(subquad.a.currentPosition, subquad.b.currentPosition);
                Gizmos.DrawLine(subquad.b.currentPosition, subquad.c.currentPosition);
                Gizmos.DrawLine(subquad.c.currentPosition, subquad.d.currentPosition);
                Gizmos.DrawLine(subquad.d.currentPosition, subquad.a.currentPosition);
            }
            
            foreach (Vertex vertex in grid.vertices)
            {
                foreach(Vertex_Y vertex_y in vertex.vertex_Ys)
                {
                    if (vertex_y.isActive) Gizmos.color = Color.red;
                    else Gizmos.color = Color.gray;
                    Gizmos.DrawSphere(vertex_y.worldPosition, 0.01f);
                }
            }
            foreach(SubQuad subQuad in grid.subQuads)
            {
                foreach(SubQuad_Cube subQuad_Cube in subQuad.subQuad_Cubes)
                {
                    Gizmos.color = Color.gray;
                    Gizmos.DrawLine(subQuad_Cube.vertex_Ys[0].worldPosition, subQuad_Cube.vertex_Ys[1].worldPosition);
                    Gizmos.DrawLine(subQuad_Cube.vertex_Ys[1].worldPosition, subQuad_Cube.vertex_Ys[2].worldPosition);
                    Gizmos.DrawLine(subQuad_Cube.vertex_Ys[2].worldPosition, subQuad_Cube.vertex_Ys[3].worldPosition);
                    Gizmos.DrawLine(subQuad_Cube.vertex_Ys[3].worldPosition, subQuad_Cube.vertex_Ys[0].worldPosition);
                    Gizmos.DrawLine(subQuad_Cube.vertex_Ys[4].worldPosition, subQuad_Cube.vertex_Ys[5].worldPosition);
                    Gizmos.DrawLine(subQuad_Cube.vertex_Ys[5].worldPosition, subQuad_Cube.vertex_Ys[6].worldPosition);
                    Gizmos.DrawLine(subQuad_Cube.vertex_Ys[6].worldPosition, subQuad_Cube.vertex_Ys[7].worldPosition);
                    Gizmos.DrawLine(subQuad_Cube.vertex_Ys[7].worldPosition, subQuad_Cube.vertex_Ys[4].worldPosition);
                    Gizmos.DrawLine(subQuad_Cube.vertex_Ys[0].worldPosition, subQuad_Cube.vertex_Ys[4].worldPosition);
                    Gizmos.DrawLine(subQuad_Cube.vertex_Ys[1].worldPosition, subQuad_Cube.vertex_Ys[5].worldPosition);
                    Gizmos.DrawLine(subQuad_Cube.vertex_Ys[2].worldPosition, subQuad_Cube.vertex_Ys[6].worldPosition);
                    Gizmos.DrawLine(subQuad_Cube.vertex_Ys[3].worldPosition, subQuad_Cube.vertex_Ys[7].worldPosition);
                    GUI.color = Color.yellow;
                    if (subQuad_Cube.bit != "00000000") 
                    {
                        Handles.Label(subQuad_Cube.centerPosition, subQuad_Cube.bit);
                        Handles.Label(subQuad_Cube.vertex_Ys[0].worldPosition, subQuad_Cube.bit[0] + " a");
                        Handles.Label(subQuad_Cube.vertex_Ys[1].worldPosition, subQuad_Cube.bit[1] + " b");
                        Handles.Label(subQuad_Cube.vertex_Ys[2].worldPosition, subQuad_Cube.bit[2] + " c");
                        Handles.Label(subQuad_Cube.vertex_Ys[3].worldPosition, subQuad_Cube.bit[3] + " d");
                        Handles.Label(subQuad_Cube.vertex_Ys[4].worldPosition, subQuad_Cube.bit[4] + " a");
                        Handles.Label(subQuad_Cube.vertex_Ys[5].worldPosition, subQuad_Cube.bit[5] + " b");
                        Handles.Label(subQuad_Cube.vertex_Ys[6].worldPosition, subQuad_Cube.bit[6] + " c");
                        Handles.Label(subQuad_Cube.vertex_Ys[7].worldPosition, subQuad_Cube.bit[7] + " d");
                    }
                }
            }
        }
    }*/
    public Grid GetGrid()
    {
        return grid;
    }
    public IEnumerator Create()
    {
        foreach (Vertex_hex vertex in grid.hexes)
        {
            GameObject t = Instantiate(p);
            t.transform.position = vertex.coord.worldPosition;
            yield return new WaitForSeconds(0.3f);
        }
    }
}
