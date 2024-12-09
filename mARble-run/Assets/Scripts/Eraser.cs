using UnityEngine;
using MagicLeap.OpenXR.Features;
using System.Collections;

namespace MagicLeap.Examples
    {
    public class Eraser : MonoBehaviour
    {
        private MagicLeapController controller;
        void Start()
        {
            controller = MagicLeapController.Instance;
        }
        public IEnumerator canErase()
        {
            while (true)
            {
                // Continuously check for bumper press
                if (controller.BumperIsPressed)
                {
                    Destroy(gameObject);
                    yield break; // Exit the coroutine after destruction
                }
                // Wait for the next frame
                yield return null;
            }
        }
    }
}