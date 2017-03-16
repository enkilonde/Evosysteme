using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    Transform origine;
    float speed = 10;
    public void Create(Transform orig, float _speed)
    {
        origine = orig;
        speed = _speed;
    }

    private void Update()
    {

        transform.position += transform.forward * Time.deltaTime * 10;
    }


    private void OnTriggerEnter(Collider other)
    {

        if (!other.gameObject.name.Contains("Monster") || other.transform == origine) return;

        Destroy(gameObject);

    }

    public static void Shoot(Transform origin, Vector3 dir, float speed)
    {
        GameObject bul = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bul.transform.localScale /= 5;
        bul.AddComponent<Bullet>().Create(origin, speed);
        bul.transform.position = origin.position;
        bul.transform.LookAt(dir);
        bul.GetComponent<Collider>().isTrigger = true;
        bul.AddComponent<Rigidbody>().isKinematic = true;
        Destroy(bul, 2);

    }

}
