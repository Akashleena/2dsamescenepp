using System.Diagnostics;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Debug=UnityEngine.Debug;


/* author :https://www.youtube.com/watch?v=sU_Y2g1Nidk */

public class WriteToCSVFile : MonoBehaviour
{
    string filename = "";
    [System.Serializable]

    public class WritingtoSS
    {
    public string nameofPP;
    public float totalTime;
    public float averageTime;
    public float totalCost;
    public float totalSimTime;

    public int totalNodes;
    }
    [System.Serializable]
    public class WritingtoSSList
    {
        public WritingtoSS[] obj;
    }
    public WritingtoSSList myWritingtoSSList = new WritingtoSSList();
    void Start() 
    {
        filename =  Application.dataPath + "/testpp.csv";
         //string m_Path = Application.dataPath;

        //Output the Game data path to the console
        Debug.Log("dataPath : " + filename);
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     WriteCSV();
        // }
    }
    public void WriteCSV(string nameofPP, float totalTime, float totalCost, int totalNodes)
    {

        Debug.Log("obj length" + myWritingtoSSList.obj.Length);
        
       
            
        TextWriter tw = new StreamWriter(filename, false);
        // with open("WriteToCSVFile.csv", "a", newline="") as file:
        tw.WriteLine("NameofPP, TotalTime, TotalCost, TotalNodes");
        tw.Close();

        tw = new StreamWriter(filename, append: true);
        

        for(int i=0; i<myWritingtoSSList.obj.Length; i++)
        {
            myWritingtoSSList.obj[i].nameofPP=nameofPP;
            myWritingtoSSList.obj[i].totalTime=totalTime;
            myWritingtoSSList.obj[i].totalCost=totalCost;
            myWritingtoSSList.obj[i].totalNodes=totalNodes;

        tw.WriteLine(myWritingtoSSList.obj[i].nameofPP + "," + myWritingtoSSList.obj[i].totalTime + ","+ myWritingtoSSList.obj[i].totalCost + "," + myWritingtoSSList.obj[i].totalNodes);
        }
        tw.Close();
        
    }
}
