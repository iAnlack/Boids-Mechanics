using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neighborhood : MonoBehaviour
{
    [Header("Set Dynamically")]
    public List<Boid> neighbors;
    private SphereCollider _collider;

    private void Start()
    {
        neighbors = new List<Boid>();
        _collider = GetComponent<SphereCollider>();
        _collider.radius = Spawner.S.neighborDist / 2;
    }

    private void FixedUpdate()
    {
        if (_collider.radius != Spawner.S.neighborDist/2)
        {
            _collider.radius = Spawner.S.neighborDist / 2;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Boid boid = other.GetComponent<Boid>();
        if (boid != null)
        {
            if (neighbors.IndexOf(boid) == -1)
            {
                neighbors.Add(boid);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Boid boid = other.GetComponent<Boid>();
        if (boid != null)
        {
            if (neighbors.IndexOf(boid) != -1)
            {
                neighbors.Remove(boid);
            }
        }
    }

    public Vector3 avgPos
    {
        get
        {
            Vector3 avg = Vector3.zero;
            if (neighbors.Count == 0)
            {
                return avg;
            }

            for (int i = 0; i < neighbors.Count; i++)
            {
                avg += neighbors[i].pos;
            }
            avg /= neighbors.Count;

            return avg;
        }
    }

    public Vector3 avgVel
    {
        get
        {
            Vector3 avg = Vector3.zero;
            if (neighbors.Count == 0)
            {
                return avg;
            }

            for (int i = 0; i < neighbors.Count; i++)
            {
                avg += neighbors[i].rigidbody.velocity;
            }
            avg /= neighbors.Count;

            return avg;
        }
    }

    public Vector3 avgClosePos
    {
        get
        {
            Vector3 avg = Vector3.zero;
            Vector3 delta;
            int nearCount = 0;
            for (int i = 0; i < neighbors.Count; i++)
            {
                delta = neighbors[i].pos - transform.position;
                if (delta.magnitude <= Spawner.S.collDist)
                {
                    avg += neighbors[i].pos;
                    nearCount++;
                }
            }
            // ≈сди нет соседей, лет€щих слишком близко, вернуть Vector3.zero
            if (neighbors.Count == 0)
            {
                return avg;
            }

            // »наче координаты центральной точки
            avg /= neighbors.Count;

            return avg;
        }
    }
}
