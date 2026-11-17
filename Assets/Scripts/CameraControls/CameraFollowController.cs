using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CameraControls
{
public class CameraFollowController : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 5f;
    [SerializeField] private List <Transform> Transforms = new List<Transform>();

    // Update is called once per frame
    private void Update()
    {
        if (Transforms.Count == 0) return;
        var bounds = new Bounds(Transforms.First().position, Vector3.zero);
        foreach (var target in Transforms)
            bounds.Encapsulate(target.position);

        this.transform.position = Vector3.Slerp(this.transform.position, bounds.center, Time.deltaTime * _movementSpeed);
    }

   public void addTarget(Transform transform)
   {
       Transforms.Add(transform);
   }
}
}