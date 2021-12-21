using System.Collections;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField]
    Transform scaledScreen;
    float margin = .1f;
    [SerializeField]
    float speed = .5f;

    [SerializeField]
    Projectile projectileTemplate;
    [SerializeField]
    float fireRate = 1;


    private void Start()
    {
        FlyLoop();
        FireLoop();
    }


    void FlyLoop()
    {
        StartCoroutine(GoToPositionThen(FlyLoop));
    }

    IEnumerator GoToPositionThen(System.Action EndAction)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(Random.value.ToMin1_1() * scaledScreen.lossyScale.x * .5f, Random.value.ToMin1_1() * scaledScreen.lossyScale.y * .5f, transform.position.z);
        while (Vector3.Distance(transform.position, targetPosition) > .01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        EndAction();
    }

    void FireLoop()
    {
        Projectile newProjectile = Instantiate(projectileTemplate);
        newProjectile.transform.position = transform.position;
        this.Timer(1 / fireRate, delegate { FireLoop(); });
    }
}
