using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Set Dynamically")]
    public Rigidbody rigidbody;

    private Neighborhood _neighborhood;

    // ����������� ���� ����� ��� �������������
    private void Awake()
    {
        _neighborhood = GetComponent<Neighborhood>();
        rigidbody = GetComponent<Rigidbody>();

        // ������� ��������� �������
        pos = Random.insideUnitSphere * Spawner.S.spawnRadius;

        // ������� ��������� ��������� ��������
        Vector3 vel = Random.onUnitSphere * Spawner.S.velocity;
        rigidbody.velocity = vel;

        LookAhead();

        // �������� ����� � ��������� ����, �� �� ������� �����
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
        // ������������� ����� ������ � ����������� �����
        transform.LookAt(pos + rigidbody.velocity);
    }

    public Vector3 pos
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    // FixedUpdate ���������� ��� ������ ��������� ������ (50 ��� � �������)
    private void FixedUpdate()
    {
        Vector3 vel = rigidbody.velocity;
        Spawner spn = Spawner.S;

        // �������������� ������������ - �������� ������� �������
        Vector3 velAvoid = Vector3.zero;
        Vector3 tooClosePos = _neighborhood.avgClosePos;
        // ���� ������� Vector3.zero, ������  ������������� �� ����
        if (tooClosePos != Vector3.zero)
        {
            velAvoid = pos - tooClosePos;
            velAvoid.Normalize();
            velAvoid *= spn.velocity;
        }

        // ������������ �������� - ����������� ����������� �������� � ��������
        Vector3 velAlign = _neighborhood.avgVel;
        // ������������ ���������, ������ ���� velAlign �� ����� Vector3.zero
        if (velAlign != Vector3.zero)
        {
            // ��� ���������� ������ �����������, ������� ����������� ��������
            velAlign.Normalize();
            // � ����� ����������� � ��������� ��������
            velAlign *= spn.velocity;
        }

        // ������������ ������� - �������� � ������� ������ ������ �������
        Vector3 velCenter = _neighborhood.avgPos;
        if (velCenter != Vector3.zero)
        {
            velCenter -= transform.position;
            velCenter.Normalize();
            velCenter *= spn.velocity;
        }

        // ���������� - ������������ �������� � ������� ������� Attractor
        Vector3 delta = Attractor.POS - pos;
        // ���������, ���� ���������, � ������� Attractor ��� �� ����
        bool attracted = (delta.magnitude > spn.attractPushDist);
        Vector3 velAttract = delta.normalized * spn.velocity;

        // ��������� ��� ��������
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

        // ���������� vel � ������������ � velocity � �������-�������� Spawner
        vel = vel.normalized * spn.velocity;
        // � ���������� ��������� �������� ���������� Rigidbody
        rigidbody.velocity = vel;
        // ��������� ����� ������ � ������� ������ ����������� ��������
        LookAhead();
    }
}
