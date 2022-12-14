using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum PortalColor
{
    Blue,
    Orange,
    Pink,
    Green
}

public class BulletPortal : MonoBehaviour
{
    public PortalColor portalColor;

    public GameObject bulletPrefab;

    public SpriteRenderer sprite;

    public ParticleSystem pSystem;

    public GameObject guts;
    public GameObject chunk;

    public void TeleportBullet(Vector2 bulletVelocity, Vector3 bulletForward, Vector3 portalUp, Vector3 hitOffset)
    {
        //Angle between portals
        var vectorAngle = CalculateAngle(portalUp, transform.up);

        //Calculate rotated velocity for bullet
        var newVelocity = Quaternion.AngleAxis(vectorAngle, Vector3.forward) * Vector3.Reflect(bulletVelocity, portalUp);

        //Calculate where on the portal to spawn bullet
        hitOffset = Quaternion.AngleAxis(vectorAngle, Vector3.forward) * hitOffset;
        hitOffset *= transform.localScale.x;

        //Spawn bullet
        var bullet = Instantiate(bulletPrefab, transform.position + transform.up / 3f - hitOffset, Quaternion.Euler(bulletForward));

        //Set bullet velocity
        bullet.GetComponentInChildren<BulletMovement>().SetVelocity(newVelocity);
    }

    private float CalculateAngle(Vector3 from, Vector3 to)
    {
        var angle = Vector3.SignedAngle(from, to, Vector3.forward);

        return angle;
    }

    private void Start()
    {
        SetVisualColor();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            foreach (var portal in GrabPortals())
            {
                portal.TriggerGore();
            }

            TriggerGore(false);

            collision.GetComponent<PlayerCollision>().TouchHazard();
            collision.GetComponent<PlayerCollision>().HidePlayer();
        }

        if (collision.CompareTag("Bullet"))
        {
            HandleBullet(collision);
        }
    }

    private void HandleBullet(Collider2D bulletCollider)
    {
        var bullet = bulletCollider.transform.root.gameObject;

        var velocity = bullet.GetComponentInChildren<Rigidbody2D>().velocity;
        var bulletUp = bullet.transform.up;

        var targetPortals = GrabPortals();

        var point = bulletCollider.ClosestPoint(transform.position);
        var offset = transform.position - new Vector3(point.x, point.y, 0);
        offset /= transform.localScale.x;

        bullet.GetComponentInChildren<BulletCollision>().KillBullet();

        foreach (var portal in targetPortals)
        {
            portal.TeleportBullet(velocity, bulletUp, transform.up, offset);
        }
    }

    private List<BulletPortal> GrabPortals()
    {
        PortalColor searchColor = PortalColor.Blue;

        switch (portalColor)
        {
            case PortalColor.Blue:
                searchColor = PortalColor.Orange;
                break;
            case PortalColor.Orange:
                searchColor = PortalColor.Blue;
                break;
            case PortalColor.Pink:
                searchColor = PortalColor.Green;
                break;
            case PortalColor.Green:
                searchColor = PortalColor.Pink;
                break;
        }

        var portals = FindObjectsOfType<BulletPortal>().Where(p => p.portalColor == searchColor).ToList();

        print(portals.Count);

        return portals;
    }

    private void TriggerGore(bool triggerGuts = true)
    {
        //GUTS
        var gutCount = Random.Range(1, 3);
        for (int i = 0; i < gutCount; i++)
        {
            if (!triggerGuts)
            {
                break;
            }

            var g = Instantiate(guts, transform.position, Quaternion.identity);
            g.transform.right = transform.right;

            var rbodies = g.GetComponentsInChildren<Rigidbody2D>();
            var forceDir = transform.up;
            forceDir += transform.right * Random.Range(-1f, 1f);

            rbodies[Random.Range(0, rbodies.Length)].AddForce(forceDir.normalized * 10, ForceMode2D.Impulse);
        }

        //CHUNKS
        var chunkCount = Random.Range(2, 5);
        for (int i = 0; i < chunkCount; i++)
        {
            var g = Instantiate(chunk, transform.position, Quaternion.identity);
            g.transform.right = transform.right;

            var rbodies = g.GetComponentsInChildren<Rigidbody2D>();
            var forceDir = transform.up;
            forceDir += transform.right * Random.Range(-1f, 1f);

            rbodies[Random.Range(0, rbodies.Length)].AddForce(forceDir.normalized * 5, ForceMode2D.Impulse);
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;

        switch (portalColor)
        {
            case PortalColor.Blue:
                Gizmos.color = Color.blue;
                break;
            case PortalColor.Orange:
                Gizmos.color = new Color(1, 0.6f, 0);
                break;
            case PortalColor.Pink:
                Gizmos.color = Color.magenta;
                break;
            case PortalColor.Green:
                Gizmos.color = Color.green;
                break;
        }

        Gizmos.DrawIcon(transform.position, "P.png", false, Gizmos.color);
        Gizmos.DrawLine(transform.position + transform.right * 0.2f, transform.position + transform.up);
        Gizmos.DrawLine(transform.position - transform.right * 0.2f, transform.position + transform.up);
    }

    private void OnValidate()
    {
        SetVisualColor();
    }

    private void SetVisualColor()
    {
        Color vfxColor = Color.white;

        switch (portalColor)
        {
            case PortalColor.Blue:
                vfxColor = Color.blue;
                break;
            case PortalColor.Orange:
                vfxColor = new Color(1, 0.6f, 0);
                break;
            case PortalColor.Pink:
                vfxColor = Color.magenta;
                break;
            case PortalColor.Green:
                vfxColor = Color.green;
                break;
        }

        vfxColor.a = pSystem.startColor.a;
        pSystem.startColor = vfxColor;
    }
}
