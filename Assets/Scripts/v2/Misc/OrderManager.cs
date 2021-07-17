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
    private Queue<int> orderQueue;

    public void GetOrder() {
        List<int[]> orders = Utility.getPermutations(3);

        // foreach(int[] locomotions in orders) {
        //     string output = "";

        //     foreach(var locomotion in locomotions) {
        //         output += (Locomotion) locomotion;
        //         output += " ";
        //     }

        //     Debug.Log(output);
        // }

        orderQueue = new Queue<int>(Utility.sampleWithoutReplacement(6, 0, 6));

        foreach(int index in orderQueue) {
            string output = "";

            foreach(var locomotion in orders[index]) {
                output += (Locomotion) locomotion;
                output += " ";
            }

            Debug.Log(output);
        }

    }

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
