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
    public static int objCount = 0;
    void Start() 
    {
        filename =  Application.dataPath + "/testpp.csv";
         //string m_Path = Application.dataPath;

        //Output the Game data path to the console
        Debug.Log("dataPath : " + filename);
        Debug.Log(File.Exists(filename));
        if(!File.Exists(filename)) {
            TextWriter tw = new StreamWriter(filename, false);
            // with open("WriteToCSVFile.csv", "a", newline="") as file:
            tw.WriteLine("NameofPP,TotalTime,TotalCost,TotalNodes,TotalSimTime");
            tw.Close();
        }
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     WriteCSV();
        // }
    }
    public void WriteCSV(string nameofPP, float totalTime, float totalCost, int totalNodes, float totalSimTime)
    {

        Debug.Log("obj length" + myWritingtoSSList.obj.Length);
        
        TextWriter tw = new StreamWriter(filename, append: true);
        
        myWritingtoSSList.obj[objCount].nameofPP=nameofPP;
        myWritingtoSSList.obj[objCount].totalTime=totalTime;
        myWritingtoSSList.obj[objCount].totalCost=totalCost;
        myWritingtoSSList.obj[objCount].totalNodes=totalNodes;
        myWritingtoSSList.obj[objCount].totalSimTime=totalSimTime;

        tw.WriteLine(myWritingtoSSList.obj[objCount].nameofPP + "," + myWritingtoSSList.obj[objCount].totalTime + ","+ myWritingtoSSList.obj[objCount].totalCost + "," + myWritingtoSSList.obj[objCount].totalNodes+ "," + myWritingtoSSList.obj[objCount].totalSimTime);
        
        tw.Close();
        
    }
}
