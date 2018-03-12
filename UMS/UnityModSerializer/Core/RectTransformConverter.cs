using UnityEngine;

#if !EDITOR
namespace UMS.Core
{
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformConverter : MonoBehaviour { }
}
#endif