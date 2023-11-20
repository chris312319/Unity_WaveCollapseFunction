using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Vertex
{
    public Vector3 initialPosition;
    public Vector3 currentPosition;
    public Vector3 offset = Vector3.zero;
    public List<SubQuad> subQuads = new List<SubQuad>();
    public List<Vertex_Y> vertex_Ys = new List<Vertex_Y>();
    public bool isBoundary;
    public int index;
    public void Relax()
    {
        currentPosition = initialPosition + offset;
    }
    public void BoundaryCheck()
    {
        bool isBoundaryHex = this is Vertex_hex && ((Vertex_hex)this).coord.radius == Grid.radius - 1;
        bool isBoundaryMid = this is Vertex_mid && ((Vertex_mid)this).edge.hexes.ToArray()[0].coord.radius == Grid.radius - 1 && ((Vertex_mid)this).edge.hexes.ToArray()[1].coord.radius == Grid.radius - 1;
        isBoundary = isBoundaryHex || isBoundaryMid;
    }
    public Mesh CreateMesh()
    {
        List<Vector3> meshVertices = new List<Vector3>();
        List<int> meshTriangles = new List<int>();
        foreach (SubQuad subQuad in subQuads)
        {
            if(this is Vertex_center)
            {
                meshVertices.Add(currentPosition);
                meshVertices.Add(subQuad.GetMid_cd());
                meshVertices.Add(subQuad.GetCenterPosition());
                meshVertices.Add(subQuad.GetMid_bc());
            }
            else if(this is Vertex_mid)
            {
                if(subQuad.b == this)
                {
                    meshVertices.Add(currentPosition);
                    meshVertices.Add(subQuad.GetMid_bc());
                    meshVertices.Add(subQuad.GetCenterPosition());
                    meshVertices.Add(subQuad.GetMid_ab());
                }
                else
                {
                    meshVertices.Add(currentPosition);
                    meshVertices.Add(subQuad.GetMid_ad());
                    meshVertices.Add(subQuad.GetCenterPosition());
                    meshVertices.Add(subQuad.GetMid_cd());
                }
            }
            else
            {
                meshVertices.Add(currentPosition);
                meshVertices.Add(subQuad.GetMid_ab());
                meshVertices.Add(subQuad.GetCenterPosition());
                meshVertices.Add(subQuad.GetMid_ad());
            }         
        }
        for (int i = 0; i < meshVertices.Count; i++)
        {
            meshVertices[i] -= currentPosition;
        }
        for (int i = 0; i < subQuads.Count; i++)
        {
            meshTriangles.Add(i * 4);
            meshTriangles.Add(i * 4 + 1);
            meshTriangles.Add(i * 4 + 2);
            meshTriangles.Add(i * 4);
            meshTriangles.Add(i * 4 + 2);
            meshTriangles.Add(i * 4 + 3);
        }
        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        return mesh;
    }
}
public class Coord
{
    public readonly int q;
    public readonly int r;
    public readonly int s;
    public readonly int radius;
    public readonly Vector3 worldPosition;
    public Coord(int q,int r,int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
        this.radius = Mathf.Max(Mathf.Abs(q), Mathf.Abs(r), Mathf.Abs(s));
        worldPosition = WorldPosition();
    }
    public Vector3 WorldPosition()
    {
        return new Vector3(q * Mathf.Sqrt(3) / 2, 0, -(float)r - ((float)q / 2)) * 2 * Grid.cellSize;
    }
    static public Coord[] directions = new Coord[]
    {
        new Coord(0,1,-1),
        new Coord(-1,1,0),
        new Coord(-1,0,1),
        new Coord(0,-1,1),
        new Coord(1,-1,0),
        new Coord(1,0,-1)
    };
    static public Coord Direction(int direction)
    {
        return Coord.directions[direction];
    }
    public Coord Add(Coord coord)
    {
        return new Coord(q + coord.q, r + coord.r, s + coord.s);
    }
    public Coord Scale(int k)
    {
        return new Coord(q * k, r * k, s * k);
    }
    public Coord Neighbor(int direction)
    {
        return Add(Direction(direction));
    }
    public static List<Coord> Coord_Ring(int radius)
    {
        List<Coord> result = new List<Coord>();
        if(radius == 0)
        {
            result.Add(new Coord(0, 0, 0));
        }
        else
        {
            Coord coord = Coord.Direction(4).Scale(radius);
            for(int i = 0; i < 6; i++)
            {
                for(int j = 0; j < radius; j++)
                {
                    result.Add(coord);
                    coord = coord.Neighbor(i);
                }              
            }
        }
        return result;
    }
    public static List<Coord> Coord_Hex(int radius)
    {
        List<Coord> result = new List<Coord>();
        for(int i = 0; i < radius; i++)
        {
            result.AddRange(Coord_Ring(i));
        }
        return result;
    }
}
public class Vertex_hex : Vertex
{
    public readonly Coord coord;
    public Vertex_hex(Coord coord)
    {
        this.coord = coord;
        this.initialPosition = coord.worldPosition;
    }
    public static void Hex(List<Vertex_hex> vertices, int radius)
    {
        foreach (Coord coord in Coord.Coord_Hex(radius))
        {
            vertices.Add(new Vertex_hex(coord));
        }
    }
    public static List<Vertex_hex> GrabRing(int radius,List<Vertex_hex> vertices)
    {
        if (radius == 0)
        {
            return vertices.GetRange(0, 1);
        }
        return vertices.GetRange(radius * (radius - 1) * 3 + 1, radius * 6);
    }
    public List<Mesh> CreateSideMesh()
    {
        int n = this.subQuads.Count;
        List<Mesh> meshes = new List<Mesh>();
        for (int i = 0; i < n; i++)
        {
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> meshTriangles = new List<int>();

            meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetMid_ab() + Vector3.up * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetMid_ab() + Vector3.down * Grid.cellHeight / 2);
            foreach(SubQuad subQuad in subQuads)
            {
                if(subQuad.d == subQuads[i].b)
                {
                    meshVertices.Add(subQuad.GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
                    meshVertices.Add(subQuad.GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
                    break;
                }
            }
            for (int j = 0; j < meshVertices.Count; j++)
            {
                meshVertices[j] -= currentPosition;
            }
            meshTriangles.Add(0);
            meshTriangles.Add(2);
            meshTriangles.Add(1);
            meshTriangles.Add(2);
            meshTriangles.Add(3);
            meshTriangles.Add(1);
            meshTriangles.Add(2);
            meshTriangles.Add(4);
            meshTriangles.Add(5);
            meshTriangles.Add(2);
            meshTriangles.Add(5);
            meshTriangles.Add(3);

            Mesh mesh = new Mesh();
            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = meshTriangles.ToArray();
            meshes.Add(mesh);
        }
        return meshes;
    }
    public void NeighborSubQuadCheck()
    {
        foreach(SubQuad subQuad_a in subQuads)
        {
            foreach(SubQuad subQuad_b in subQuads)
            {
                if(subQuad_a.b == subQuad_b.d)
                {
                    subQuad_a.neighbors[0] = subQuad_b;
                    subQuad_a.neighborVertices.Add(subQuad_b, new Vertex[] { subQuad_a.b, subQuad_a.a });
                    break;
                }
            }
            foreach (SubQuad subQuad_b in subQuads)
            {
                if (subQuad_a.d == subQuad_b.b)
                {
                    subQuad_a.neighbors[3] = subQuad_b;
                    subQuad_a.neighborVertices.Add(subQuad_b, new Vertex[] { subQuad_a.d, subQuad_a.a });
                    break;
                }
            }
        }
    }
}
public class Vertex_mid : Vertex
{
    public readonly Edge edge;
    public Vertex_mid(Edge edge,List<Vertex_mid> mids)
    {
        this.edge = edge;
        Vertex_hex a = edge.hexes.ToArray()[0];
        Vertex_hex b = edge.hexes.ToArray()[1];
        mids.Add(this);
        initialPosition = (a.initialPosition + b.initialPosition) / 2;
        currentPosition = initialPosition;
    }
    public List<Mesh> CreateSideMesh()
    {
        List<Mesh> meshes = new List<Mesh>();
        for (int i = 0; i < 4; i++)
        {
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> meshTriangles = new List<int>();

            meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);

            if (subQuads[i].b == this)
            {
                meshVertices.Add(subQuads[i].GetMid_bc() + Vector3.up * Grid.cellHeight / 2);
                meshVertices.Add(subQuads[i].GetMid_bc() + Vector3.down * Grid.cellHeight / 2);
                foreach(SubQuad subQuad in subQuads)
                {
                    if(subQuad.c == subQuads[i].c && subQuad != subQuads[i])
                    {
                        meshVertices.Add(subQuad.GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
                        meshVertices.Add(subQuad.GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
                        break;
                    }
                }
            }
            else
            {
                meshVertices.Add(subQuads[i].GetMid_ad() + Vector3.up * Grid.cellHeight / 2);
                meshVertices.Add(subQuads[i].GetMid_ad() + Vector3.down * Grid.cellHeight / 2);
                foreach (SubQuad subQuad in subQuads)
                {
                    if (subQuad.a == subQuads[i].a && subQuad != subQuads[i])
                    {
                        meshVertices.Add(subQuad.GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
                        meshVertices.Add(subQuad.GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
                        break;
                    }
                }
            }
            for (int j = 0; j < meshVertices.Count; j++)
            {
                meshVertices[j] -= currentPosition;
            }
            meshTriangles.Add(0);
            meshTriangles.Add(2);
            meshTriangles.Add(1);
            meshTriangles.Add(2);
            meshTriangles.Add(3);
            meshTriangles.Add(1);
            meshTriangles.Add(2);
            meshTriangles.Add(4);
            meshTriangles.Add(5);
            meshTriangles.Add(2);
            meshTriangles.Add(5);
            meshTriangles.Add(3);

            Mesh mesh = new Mesh();
            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = meshTriangles.ToArray();
            meshes.Add(mesh);
        }
        return meshes;
    }
}
public class Vertex_center : Vertex 
{
    public List<Mesh> CreateSideMesh()
    {
        int n = this.subQuads.Count;
        List<Mesh> meshes = new List<Mesh>();
        for(int i = 0; i < n; i++)
        {
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> meshTriangles = new List<int>();

            meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetMid_cd() + Vector3.up * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[(i+n-1)%n].GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetMid_cd() + Vector3.down * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[(i + n - 1) % n].GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
            for (int j = 0; j < meshVertices.Count; j++)
            {
                meshVertices[j] -= currentPosition;
            }
            meshTriangles.Add(0);
            meshTriangles.Add(1);
            meshTriangles.Add(3);
            meshTriangles.Add(1);
            meshTriangles.Add(4);
            meshTriangles.Add(3);
            meshTriangles.Add(1);
            meshTriangles.Add(2);
            meshTriangles.Add(5);
            meshTriangles.Add(1);
            meshTriangles.Add(5);
            meshTriangles.Add(4);

            Mesh mesh = new Mesh();
            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = meshTriangles.ToArray();
            meshes.Add(mesh);
        }
        return meshes;
    }
}
public class Vertex_triangleCenter : Vertex_center
{
    public Vertex_triangleCenter(Triangle triangle)
    {
        initialPosition = (triangle.a.initialPosition + triangle.b.initialPosition + triangle.c.initialPosition) / 3;
        currentPosition = initialPosition;
    }
}
public class Vertex_quadCenter : Vertex_center
{
    public Vertex_quadCenter(Quad quad)
    {
        initialPosition = (quad.a.initialPosition + quad.b.initialPosition + quad.c.initialPosition + quad.d.initialPosition) / 4;
        currentPosition = initialPosition;
    }
}
public class Vertex_Y
{
    public readonly Vertex vertex;
    public readonly int y;
    public readonly string name;
    public readonly Vector3 worldPosition;
    public readonly bool isBoundary;
    public bool isActive;
    public List<SubQuad_Cube> subQuad_Cubes = new List<SubQuad_Cube>();
    public Vertex_Y(Vertex vertex,int y)
    {
        this.vertex = vertex;
        this.y = y;
        name = "Vertex_" + vertex.index + "_" + y;
        isBoundary = vertex.isBoundary || y == Grid.height || y == 0;
        worldPosition = vertex.currentPosition + Vector3.up * (y * Grid.cellHeight);
    }
}