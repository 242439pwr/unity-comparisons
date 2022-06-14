using UnityEngine;

namespace InfallibleCode.Threads
{
    public class Building : MonoBehaviour
    {
        [SerializeField] private int floors;

        private int _tenants;
        private Unity.Mathematics.Random _random;

        public int PowerUsage { get; private set; }

        private void Awake()
        {
            _tenants = floors * 250;
        }

        public void UpdatePowerUsage()
        {
            var random = new Unity.Mathematics.Random(1);
            for (var i = 0; i < _tenants; i++)
            {
                PowerUsage += random.NextInt(12, 24);
            }
        }
    }
}
