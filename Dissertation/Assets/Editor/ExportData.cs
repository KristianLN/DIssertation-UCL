using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ExportData
{
    // Exporting draw
    //private string path;
    //public string nameOfFile;
    //private string contentToWrite;
    [MenuItem("Tools/Write file")]
    public void exportData(string pathToFile, string contentToWrite)//string pathToFile, string contentToWrite
    {
      // StreamWriter sw = File.AppendAllText(path,contentToWrite);
      // sw.Close();
      File.AppendAllText(pathToFile,contentToWrite);
    }

    // [MenuItem("Tools/Write file")]
    // static void WriteString()
    // {
    //     string path = "Assets/Resources/test.txt";
    //
    //     //Write some text to the test.txt file
    //     StreamWriter writer = new StreamWriter(path, true);
    //     writer.WriteLine("Test");
    //     writer.Close();
    //
    //     // //Re-import the file to update the reference in the editor
    //     // AssetDatabase.ImportAsset(path);
    //     // TextAsset asset = Resources.Load("test");
    //     //
    //     // //Print the text from the file
    //     // Debug.Log(asset.text);
    // }

}
