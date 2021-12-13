using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Set Dynamically")]
    public Rigidbody rigidbody;

    private Neighborhood _neighborhood;

    // Используйте этот метод для инициализации
    private void Awake()
    {
        _neighborhood = GetComponent<Neighborhood>();
        rigidbody = GetComponent<Rigidbody>();

        // Выбрать случайную позицию
        pos = Random.insideUnitSphere * Spawner.S.spawnRadius;

        // Выбрать случайную начальную скорость
        Vector3 vel = Random.onUnitSphere * Spawner.S.velocity;
        rigidbody.velocity = vel;

        LookAhead();

        // Окрасить птицу в случайный цвет, но не слишком тёмный
        Color randomColor = Color.black;
        while (randomColor.r + randomColor.g + randomColor.b < 1.0f)
        {
            randomColor = new Color(Random.value, Random.value, Random.value);
        }
        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
        {
            r.material.color = randomColor;
        }
        TrailRenderer trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.material.SetColor("_TintColor", randomColor);
    }

    private void LookAhead()
    {
        // Ориентировать птицу клювом в направлении полёта
        transform.LookAt(pos + rigidbody.velocity);
    }

    public Vector3 pos
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    // FixedUpdate вызывается при каждом пересчёте физики (50 раз в секунду)
    private void FixedUpdate()
    {
        Vector3 vel = rigidbody.velocity;
        Spawner spn = Spawner.S;

        // ПРЕДОВТРАЩЕНИЕ СТОЛКНОВЕНИЙ - избегать близких соседей
        Vector3 velAvoid = Vector3.zero;
        Vector3 tooClosePos = _neighborhood.avgClosePos;
        // Если получен Vector3.zero, ничего  предпринимать не надо
        if (tooClosePos != Vector3.zero)
        {
            velAvoid = pos - tooClosePos;
            velAvoid.Normalize();
            velAvoid *= spn.velocity;
        }

        // СОГЛАСОВАНИЕ СКОРОСТИ - попробовать согласовать скорость с соседями
        Vector3 velAlign = _neighborhood.avgVel;
        // Согласование требуется, только если velAlign не равно Vector3.zero
        if (velAlign != Vector3.zero)
        {
            // Нас интересует только направление, поэтому нормализуем скорость
            velAlign.Normalize();
            // и затем преобразуем в выбранную скорость
            velAlign *= spn.velocity;
        }

        // КОНЦЕНТРАЦИЯ СОСЕДЕЙ - движения в сторону центра группы соседей
        Vector3 velCenter = _neighborhood.avgPos;
        if (velCenter != Vector3.zero)
        {
            velCenter -= transform.position;
            velCenter.Normalize();
            velCenter *= spn.velocity;
        }

        // ПРИТЯЖЕНИЕ - организовать движение в сторону объекта Attractor
        Vector3 delta = Attractor.POS - pos;
        // Проверить, куда двигаться, в сторону Attractor или от него
        bool attracted = (delta.magnitude > spn.attractPushDist);
        Vector3 velAttract = delta.normalized * spn.velocity;

        // Применить все скорости
        float fdt = Time.fixedDeltaTime;
        if (velAvoid != Vector3.zero)
        {
            vel = Vector3.Lerp(vel, velAvoid, spn.collAvoid * fdt);
        }
        else
        {
            if (velAlign != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAvoid, spn.velMatching * fdt);
            }
            if (velCenter != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAvoid, spn.flockCentering * fdt);
            }
            if (velAttract != Vector3.zero)
            {
                if (attracted)
                {
                    vel = Vector3.Lerp(vel, velAttract, spn.attractPull * fdt);
                }
                else
                {
                    vel = Vector3.Lerp(vel, -velAttract, spn.attractPush * fdt);
                }
            }
        }

        // Установить vel в соответствии с velocity в объекте-одиночке Spawner
        vel = vel.normalized * spn.velocity;
        // В заключение присвоить скорость компоненту Rigidbody
        rigidbody.velocity = vel;
        // Повернуть птицу клювом в сторону нового направления движения
        LookAhead();
    }
}
