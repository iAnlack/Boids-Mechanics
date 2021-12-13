using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    // Ётот класс реализует шаблон проектировани€ "ќдиночка". —уществовать может
    // только один экземл€р Spawner, поэтому сохраним его в статической переменной S
    static public Spawner S;
    static public List<Boid> boids;

    // Ёти пол€ позвол€ют настраивать пор€док создани€ объктов Boid
    [Header("Set in Inspector: Spawning")]
    public GameObject boidPrefab;
    public Transform boidAnchor;
    public int numBoids = 100;
    public float spawnRadius = 100f;
    public float spawnDelay = 0.1f;

    // Ёти пол€ позвол€ют настраивать стайное поведение объектов Boid
    [Header("Set in Inspector: Boids")]
    public float velocity = 30f;
    public float neighborDist = 30f;
    public float collDist = 4f;
    public float velMatching = 0.25f;
    public float flockCentering = 0.2f;
    public float collAvoid = 2f;
    public float attractPull = 2f;
    public float attractPush = 2f;
    public float attractPushDist = 5f;

    private void Awake()
    {
        // —охранить этот экземпл€р Spawner в S
        S = this;
        // «апустить создание объектов Boid
        boids = new List<Boid>();
        InstantiateBoid();
    }

    public void InstantiateBoid()
    {
        GameObject gameObject = Instantiate(boidPrefab);
        Boid boid = gameObject.GetComponent<Boid>();
        boid.transform.SetParent(boidAnchor);
        boids.Add(boid);
        if (boids.Count < numBoids)
        {
            Invoke("InstantiateBoid", spawnDelay);
        }
    }
}
