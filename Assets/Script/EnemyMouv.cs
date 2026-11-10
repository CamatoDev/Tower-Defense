using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyMouv : MonoBehaviour
{
    private Transform target;
    private int waypointIndex = 0;

    private Enemy enemy;

    [Header("Orientation & Virage")]
    [Tooltip("Vitesse de rotation face au chemin")]
    public float turnSpeed = 10f;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        target = Waypoints.points[0];

        // Regarde immédiatement vers le premier waypoint dès l'apparition
        Vector3 initialDir = target.position - transform.position;
        initialDir.y = 0f;
        if (initialDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(initialDir);
        }
    }

    private void Update()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f; // Neutralise la différence de hauteur (hover Y) pour naviguer strictement sur le plan XZ

        if (dir.sqrMagnitude > 0.0001f)
        {
            // 1. Déplacement vers le waypoint
            transform.Translate(dir.normalized * enemy.speed * Time.deltaTime, Space.World);

            // 2. Orientation fluide face au waypoint (virage progressif)
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        // Distance 2D sur le plan XZ (le flottement Y ne doit pas empêcher d'atteindre le waypoint)
        float distXZ = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(target.position.x, target.position.z)
        );

        if (distXZ <= 0.3f)
        {
            GetNextWaypoint();
        }
        // NOTE: Ne pas reset enemy.speed ici — le slow du laser beamer
        // est géré dans Enemy.slow() et doit persister entre les frames.
        // La vitesse est restaurée naturellement au respawn.
    }

    private void GetNextWaypoint()
    {
        if(waypointIndex >= Waypoints.points.Length - 1)
        {
            EndPath();
            return;
        }

    waypointIndex++;
    target = Waypoints.points[waypointIndex];  
    }

    private void EndPath()
    {
        PlayerStats.lives --;
        WaveSpawner.EnemiesAlive --;
        Destroy(gameObject);
    }
 
}
