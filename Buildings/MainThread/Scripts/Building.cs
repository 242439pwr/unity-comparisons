using UnityEngine;
using Random = UnityEngine.Random;

namespace InfallibleCode.Start_Here
{
    public class Building : MonoBehaviour
    {
        [SerializeField] private int floors;

        private int _tenants;

        public int PowerUsage { get; private set; }

        private void Awake()
        {
            _tenants = floors * Random.Range(250, 250);
        }

        private void Update()
        {
            for (var i = 0; i < _tenants; i++)
            {
                PowerUsage += Random.Range(12, 24);
            }
        }
    }
}