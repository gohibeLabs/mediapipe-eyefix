using UnityEngine.UI;
using UnityEngine;

public class EyeShapeSetter : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(()=>EyeShapeManager.Instance.SetEyeShape(gameObject.name));
    }
}
