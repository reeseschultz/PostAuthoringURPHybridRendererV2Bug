using TMPro;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

public class SpawnSystem : SystemBase
{
    const float RAYCAST_DISTANCE = 1000;

    bool spawnEntity = true; // GameObjects are spawned if false, entities if true.

    Camera cam = default;

    Entity cubePrefab = default;

    PhysicsWorld physicsWorld => World.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld;

    TextMeshProUGUI text = default;

    protected override void OnCreate()
    {
        cam = Camera.main;
        text = GameObject.Find("Text").GetComponent<TextMeshProUGUI>();
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            spawnEntity = !spawnEntity;

            if (spawnEntity)
            {
                text.text = text.text.Replace("Spawning GameObjects", "Spawning entities");
                text.text = text.text.Replace("to entities", "to GameObjects");
            }
            else
            {
                text.text = text.text.Replace("Spawning entities", "Spawning GameObjects");
                text.text = text.text.Replace("to GameObjects", "to entities");
            }
        }

        if (!Input.GetMouseButtonUp(0)) return;

        // Below line is needed here since the prefab isn't available in OnCreate:
        if (cubePrefab == Entity.Null) cubePrefab = EntityManager.CreateEntityQuery(typeof(Cube)).GetSingleton<Cube>().Value;

        var screenPointToRay = cam.ScreenPointToRay(Input.mousePosition);

        var rayInput = new RaycastInput
        {
            Start = screenPointToRay.origin,
            End = screenPointToRay.GetPoint(RAYCAST_DISTANCE),
            Filter = CollisionFilter.Default
        };

        if (!physicsWorld.CastRay(rayInput, out RaycastHit hit)) return;

        var spawnLocation = hit.Position + hit.SurfaceNormal;

        if (spawnEntity)
        {
            var entity = EntityManager.Instantiate(cubePrefab);

            EntityManager.AddComponentData(entity, new Translation
            {
                Value = spawnLocation
            });
        }
        else
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = (Vector3)spawnLocation;
        }
    }
}
