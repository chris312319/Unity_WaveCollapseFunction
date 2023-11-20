using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum raycastHitType
{
    ground,
    top,
    bottom,
    side,
    none
}
public class Clicker : MonoBehaviour
{
    // Start is called before the first frame update
    private GridGenerator gridGenerator;
    private ColliderSystem colliderSystem;
    public WaveFunctionCollapse waveFunctionCollapse;
    private SlotColliderSystem slotColliderSystem;
    private PlayerInputActions inputActions;
    private RaycastHit raycastHit;
    private raycastHitType raycastHitType;
    [SerializeField]
    private float raycastRange;
    [SerializeField]
    private LayerMask clickerLayerMask;
    private Vertex_Y vertex_Y_Selected;
    private Vertex_Y vertex_Y_Target;
    private Vertex_Y vertex_Y_pre_Selected;
    private Vertex_Y vertex_Y_pre_Target;
    [SerializeField]
    private Cursor cursor;
    public void Awake()
    {
        gridGenerator = GetComponentInParent<WorldMaster>().gridGenerator;
        colliderSystem = GetComponentInParent<WorldMaster>().colliderSystem;
        waveFunctionCollapse = GetComponentInParent<WorldMaster>().waveFunctionCollapse;
        slotColliderSystem = colliderSystem.GetSlotColliderSystem();
        inputActions = new PlayerInputActions();
        inputActions.Build.Enable();
        inputActions.Build.Add.performed += Add;
        inputActions.Build.Delete.performed += Delete;
    }
    private void Update()
    {
        FindTarget();
        UpdateCursor();
    }
    public void FindTarget()
    {
        vertex_Y_pre_Selected = vertex_Y_Selected;
        vertex_Y_pre_Target = vertex_Y_Target;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray,out raycastHit, raycastRange, clickerLayerMask))
        {
            if (raycastHit.transform.GetComponent<GroundCollider_Quad>())
            {
                vertex_Y_Selected = null;

                Vector3 aim = raycastHit.point;
                SubQuad subQuad = raycastHit.transform.GetComponent<GroundCollider_Quad>().subQuad;
                Vector3 a = subQuad.a.currentPosition;
                Vector3 b = subQuad.b.currentPosition;
                Vector3 c = subQuad.c.currentPosition;
                Vector3 d = subQuad.d.currentPosition;

                Vector3 ab = (a + b) / 2;
                Vector3 cd = (c + d) / 2;
                Vector3 bc = (b + c) / 2;
                Vector3 da = (d + a) / 2;

                float ab_cd = (aim.z - ab.z) * (aim.x - cd.x) - (aim.z - cd.z) * (aim.x - ab.x);
                float bc_da = (aim.z - bc.z) * (aim.x - da.x) - (aim.z - da.z) * (aim.x - bc.x);

                float a_ab_cd = (a.z - ab.z) * (a.x - cd.x) - (a.z - cd.z) * (a.x - ab.x);
                float a_bc_da = (a.z - bc.z) * (a.x - da.x) - (a.z - da.z) * (a.x - bc.x);

                bool on_ad_side = ab_cd * a_ab_cd >= 0;
                bool on_ab_side = bc_da * a_bc_da >= 0;

                if(on_ad_side && on_ab_side)
                {
                    vertex_Y_Target = subQuad.a.vertex_Ys[1];
                }
                else if (on_ad_side && !on_ab_side)
                {
                    vertex_Y_Target = subQuad.d.vertex_Ys[1];
                }
                else if (!on_ad_side && on_ab_side)
                {
                    vertex_Y_Target = subQuad.b.vertex_Ys[1];
                }
                else if (!on_ad_side && !on_ab_side)
                {
                    vertex_Y_Target = subQuad.c.vertex_Ys[1];
                }
                if (vertex_Y_Target.vertex.isBoundary)
                {
                    vertex_Y_Target = null;
                    raycastHitType = raycastHitType.none;
                }
                else
                {
                    raycastHitType = raycastHitType.ground;
                }
            }
            else
            {
                vertex_Y_Selected = raycastHit.transform.parent.GetComponent<SlotCollider>().vertex_Y;
                int y = vertex_Y_Selected.y;
                if (raycastHit.transform.GetComponent<SlotCollider_Top>())
                {
                    if(y < Grid.height - 1)
                    {
                        vertex_Y_Target = vertex_Y_Selected.vertex.vertex_Ys[y +1];
                        raycastHitType = raycastHitType.top;
                    }
                    else
                    {
                        vertex_Y_Target = null;
                        raycastHitType = raycastHitType.none;
                    }
                }
                else if (raycastHit.transform.GetComponent<SlotCollider_Bottom>())
                {
                    if(y > 1)
                    {
                        vertex_Y_Target = vertex_Y_Selected.vertex.vertex_Ys[y - 1];
                        raycastHitType = raycastHitType.bottom;
                    }
                    else
                    {
                        vertex_Y_Target = null;
                        raycastHitType = raycastHitType.none;
                    }
                }
                else
                {
                    vertex_Y_Target = raycastHit.transform.GetComponent<SlotCollider_Side>().neighbor;
                    if (vertex_Y_Target.vertex.isBoundary)
                    {
                        vertex_Y_Target = null;
                        raycastHitType = raycastHitType.none;
                    }
                    else
                    {
                        raycastHitType = raycastHitType.side;
                    }
                }
            }
        }
        else
        {
            vertex_Y_Target = null;
            vertex_Y_Selected = null;
            raycastHitType = raycastHitType.none;
        }
    }
    private void UpdateCursor()
    {
        if(vertex_Y_pre_Target != vertex_Y_Target || vertex_Y_pre_Selected != vertex_Y_Selected)
        {
            cursor.UpdateCursor(raycastHit,raycastHitType,vertex_Y_Selected, vertex_Y_Target);
        }
    }
    private void Add(InputAction.CallbackContext ctx)
    {
        if(vertex_Y_Target != null && !vertex_Y_Target.isActive)
        {
            gridGenerator.ToggleSlot(vertex_Y_Target);
            slotColliderSystem.CreateCollider(vertex_Y_Target);
            waveFunctionCollapse.WFC();
        }
    }
    private void Delete(InputAction.CallbackContext ctx)
    {
        if (vertex_Y_Selected != null && vertex_Y_Selected.isActive)
        {
            gridGenerator.ToggleSlot(vertex_Y_Selected);
            slotColliderSystem.DestroyCollider(vertex_Y_Selected);
            waveFunctionCollapse.WFC();
        }
    }
    private void OnDrawGizmos()
    {
        if (vertex_Y_Target != null)
        {
            //Gizmos.DrawSphere(vertex_Y_Target.worldPosition,0.2f);
        }
    }
}
