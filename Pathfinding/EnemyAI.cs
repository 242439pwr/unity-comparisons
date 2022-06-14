using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Threading;
using Unity.Profiling;
using UnityEngine.Profiling;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Collections;

public class EnemyAI : MonoBehaviour
{

    [SerializeField]
    private float proximityCheckFrequency = 0.5f;

    static readonly ProfilerMarker pathfindingCoroutineMarker = new ProfilerMarker("Mark.CoroutinePathfinding");
    static Thread mainThread = Thread.CurrentThread;
    public Transform target;
    public float speed = 200f;
    public float nextWaypointDistance = 3f;


    public Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;

    public Seeker seeker;
    public Rigidbody2D rb;
    private IEnumerator coroutine;
    void Start()
    {
        Physics2D.autoSimulation = false;
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    public void OnInstantiate()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    public void UpdatePath(Vector2 rb_pos, Vector3 tar_pos)
    {
        if (seeker.IsDone())
            seeker.StartPath(rb_pos,
                tar_pos
                );
    }
    public void UpdatePath()
    {
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }
    public void RunCoroutine(float loopTime) => StartCoroutine(UpdatePathCoroutine(loopTime));
    public void RunThread() => UpdatePath();
    public void RunJob() => UpdatePath();


    public void RunTask()
    {
        var _seeker = seeker;
        var _rbPos = rb.position;
        var _tarPos = target.position;
        Task<Path> t = Task.Factory.StartNew(() =>
        {
            return _seeker.StartPath(_rbPos, _tarPos);
        });
        OnPathComplete(t.Result);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    IEnumerator UpdatePathCoroutine(float time)
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(time);
            UpdatePath();
        }
    }

    void FixedUpdate()
    {
        if (path == null) return;
        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;
        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }

}
