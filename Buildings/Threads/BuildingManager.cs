using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace InfallibleCode.Threads
{
    public class BuildingManager : MonoBehaviour
    {
        [SerializeField] private List<Building> buildings;
        private List<Task> tasks;

        private void Awake()
        {
            tasks = new List<Task>(buildings.Count);
        }

        private void Update()
        {
            foreach(var building in buildings)
            {
                tasks.Add(Task.Run(() => building.UpdatePowerUsage()));
            }
            Task.WaitAll(tasks.ToArray());
        }

    }
}