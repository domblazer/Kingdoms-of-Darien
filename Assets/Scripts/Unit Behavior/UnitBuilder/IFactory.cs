using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFactory
{
    Transform spawnPoint { get; set; }
    Transform rallyPoint { get; set; }
}
