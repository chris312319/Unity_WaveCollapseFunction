using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Cursor : MonoBehaviour
{
    public void Awake()
    {
        transform.localScale = Vector3.one * 1.1f;
        GetComponent<MeshRenderer>().material.color = new Color(255,243,0);
    }
    public void UpdateCursor(RaycastHit raycastHit,raycastHitType raycastHitType,Vertex_Y selected,Vertex_Y target)
    {
        GetComponent<MeshFilter>().mesh.Clear();
        if(raycastHitType == raycastHitType.ground)
        {
            Vertex vertex = target.vertex;
            GetComponent<MeshFilter>().mesh = vertex.CreateMesh();
            transform.position = target.worldPosition + Vector3.down * Grid.cellHeight * 0.5f;
        }
        else if(raycastHitType == raycastHitType.top)
        {
            Vertex vertex = selected.vertex;
            GetComponent<MeshFilter>().mesh = vertex.CreateMesh();
            transform.position = selected.worldPosition + Vector3.up * Grid.cellHeight * 0.51f;
        }
        else if (raycastHitType == raycastHitType.bottom)
        {
            Vertex vertex = selected.vertex;
            GetComponent<MeshFilter>().mesh = vertex.CreateMesh();
            GetComponent<MeshFilter>().mesh.triangles = GetComponent<MeshFilter>().mesh.triangles.Reverse().ToArray();
            transform.position = selected.worldPosition + Vector3.down * Grid.cellHeight * 0.51f;
        }
        else if (raycastHitType == raycastHitType.side)
        {
            Vertex vertex = selected.vertex;
            GetComponent<MeshFilter>().mesh = raycastHit.transform.GetComponent<MeshCollider>().sharedMesh;
            transform.position = selected.worldPosition;
        }
    }
}
