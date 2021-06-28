using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

public enum Locomotion {
    Manipulation = 0,
    Teleport = 1,
    S2C = 2
}

public class OrderManager : MonoBehaviour
{
    public string experimentID;
    private Queue<Locomotion> locoQueue;

    public void ShowLocomotionOrder() {
        locoQueue = new Queue<Locomotion>(Utility.sampleWithoutReplacement(3, 0, 3).Select(x => (Locomotion) x)); // IV 1
        string directoryPath = "Assets/Resources/Experiment2_Result";
        string fileName = $"{experimentID}";

        foreach(var locomotion in locoQueue) {
            fileName += $"_{locomotion}";
        }
        fileName += ".txt";

        string filePath = directoryPath + "/" + fileName;

        if(!Directory.Exists(directoryPath)) {
            Directory.CreateDirectory(directoryPath);
        }

        bool contains  = Directory.EnumerateFiles(directoryPath).Any(f => f.Contains($"{experimentID}_"));
        if(!contains) {
            Debug.Log(fileName);
            File.Create(filePath);
        }
        
    }
}
