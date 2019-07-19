using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ExcelSheetForEditor
{
    public ExcelSheetData SheetData;
    public bool FoldOpen = false;
    public FbsFile FbsObject;
}

public class ExcelDataForEditor
{
    public string ExcelPath;
    public List<ExcelSheetForEditor> ExcelSheetDatas;
    public bool FoldOpen = false;

    public ExcelDataForEditor()
    {
        ExcelPath = string.Empty;
        ExcelSheetDatas = new List<ExcelSheetForEditor>();
    }
}

public class FlatBufferToolWindow : EditorWindow
{
    private static string[] ExcelExts = new string[] { ".xlsx", ".xls" };

    public static Dictionary<string, List<ExcelSheetData>> ExcelDic;

    public static List<ExcelDataForEditor> ExcelList;

    //左侧shader列表ScrollView位置
    Vector2 scrollViewPos;

    public static bool ShowExcelInfo;

    private static FlatBufferToolWindow m_window;
    public static FlatBufferToolWindow Window
    {
        get
        {
            if (m_window == null)
            {
                m_window = EditorWindow.GetWindow<FlatBufferToolWindow>("FlatBufferToolWindow");
                m_window.minSize = new Vector2(200, 200);
            }
            return m_window;
        }
    }

    private void ReadAllExcel()
    {
        if (ExcelList == null)
            ExcelList = new List<ExcelDataForEditor>();
        else
            ExcelList.Clear();
        string[] excels = FileHelper.GetFiles(FlatBufferToolConfigure.Configure.ExcelDir, ExcelExts);
        foreach (string excelFilePath in excels)
        {
            List<ExcelSheetData> excelSheetDatas = new List<ExcelSheetData>();
            FileGenerater.ReadExcel(excelFilePath,ref excelSheetDatas);
            ExcelDataForEditor excel = new ExcelDataForEditor
            {
                ExcelPath = excelFilePath
            };
            foreach (var exd in excelSheetDatas)
            {
                excel.ExcelSheetDatas.Add(new ExcelSheetForEditor() {
                    SheetData = exd,
                    FoldOpen = false,
                    FbsObject = FileGenerater.GenFbsFileObject(exd) 
                });
              
            }
            ExcelList.Add(excel);
        }
 
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Setting");
        GUIHelper.DrawFilePick("FlatcPath:", ref FlatBufferToolConfigure.Configure.FlactcPath);
        EditorGUILayout.Separator();
        GUIHelper.DrawFolderPick("ExcelDir:", ref FlatBufferToolConfigure.Configure.ExcelDir);
        EditorGUILayout.Separator();
        GUIHelper.DrawFolderPick("SchemaDir:", ref FlatBufferToolConfigure.Configure.SchemaDir);
        EditorGUILayout.Separator();
        GUIHelper.DrawFolderPick("JsonDir:", ref FlatBufferToolConfigure.Configure.JsonDir);
        EditorGUILayout.Separator();
        GUIHelper.DrawFolderPick("ClassDir:", ref FlatBufferToolConfigure.Configure.ClassDir);
        EditorGUILayout.Separator();
        GUIHelper.DrawFolderPick("BinDir", ref FlatBufferToolConfigure.Configure.BinDir);
        EditorGUILayout.Separator();
        
        EditorGUILayout.BeginHorizontal();
        string btnShowFilter = "显示Excel列表";
        Color preColor = GUI.color;
        GUI.color = ShowExcelInfo ? Color.white : Color.gray;
        if (GUILayout.Button(btnShowFilter, GUILayout.MaxWidth(100)))
        {
            ShowExcelInfo = !ShowExcelInfo;
        }
        GUI.color = preColor;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        if (ShowExcelInfo)
        {
            scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);
            if (ExcelList == null)
            {
                ReadAllExcel();
            }
            foreach (var excel in ExcelList)
            {
                GUIStyle style = !excel.FoldOpen ? GUIHelper.GetStyle(GUIStyleEnum.FOLDOUTNORMAL) : GUIHelper.GetStyle(GUIStyleEnum.FOLDOUTDIM);
                excel.FoldOpen = EditorGUILayout.Foldout(excel.FoldOpen, excel.ExcelPath,true, style);
                if (excel.FoldOpen)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(15));
                    GUILayout.BeginVertical();
                    foreach (var sheet in excel.ExcelSheetDatas)
                    {
                        GUIStyle sheetstyle = !sheet.FoldOpen ? GUIHelper.GetStyle(GUIStyleEnum.FOLDOUTNORMAL) : GUIHelper.GetStyle(GUIStyleEnum.FOLDOUTDIM);
                        sheet.FoldOpen = EditorGUILayout.Foldout(sheet.FoldOpen, sheet.SheetData.sheetName, true, sheetstyle);
                        if (sheet.FoldOpen)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("", GUILayout.Width(15));
                            GUILayout.BeginVertical();
                            int fieldCount = sheet.SheetData.fieldNames.Count;
                            float itemWidth = (Window.position.size.x - 30) / fieldCount;
                            FbsTable fbsTable = FileGenerater.GetFbsRootTableDataTypeObj(sheet.FbsObject);
                            if (fbsTable == null)
                            {
                                Debug.LogError("FbsTable is Null!");
                                return;
                            }
                            GUILayout.BeginHorizontal(GUIHelper.GetStyle(GUIStyleEnum.BLACKSTYLE));
                            for (int findex = 0; findex < fieldCount; findex++)
                            {
                                GUILayout.Label(sheet.SheetData.fieldNames[findex], GUIHelper.GetStyle(GUIStyleEnum.MIDDLETITLE), GUILayout.Width(itemWidth));
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal(GUIHelper.GetStyle(GUIStyleEnum.BLACKSTYLE));
                          
                            for (int findex = 0; findex < fieldCount; findex++)
                            {
                                string typename = FileGenerater.GetTableFieldTypeString(fbsTable.fields[findex]);
                                GUILayout.Label(typename, GUIHelper.GetStyle(GUIStyleEnum.MIDDLETITLE), GUILayout.Width(itemWidth));
                            }
                            GUILayout.EndHorizontal();

                            for (int vindex = 0; vindex < sheet.SheetData.fieldValues.Count; vindex++)
                            {
                                int dataIndex = Mathf.FloorToInt(vindex % fieldCount);
                                if(dataIndex==0)
                                    GUILayout.BeginHorizontal();
                                GUILayout.TextArea(sheet.SheetData.fieldValues[vindex], GUIHelper.GetStyle(GUIStyleEnum.MIDDLETITLE),GUILayout.Width(itemWidth));
                                if (dataIndex == fieldCount-1)
                                    GUILayout.EndHorizontal();
                            }

                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                        }
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();
        }
        if (GUILayout.Button("Save"))
        {
            FlatBufferToolConfigure.Save();
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("build"))
        {
            string[] excels = FileHelper.GetFiles(FlatBufferToolConfigure.Configure.ExcelDir,ExcelExts);
            string outSchemaPath = FlatBufferToolConfigure.Configure.SchemaDir;
            string outJsonPath = FlatBufferToolConfigure.Configure.JsonDir;
            string outClassPath = FlatBufferToolConfigure.Configure.ClassDir;
            string outBinPath = FlatBufferToolConfigure.Configure.BinDir;
            foreach (string excelFilePath in excels) {
                FileGenerater.BuildAllFromExcel(FlatBufferToolConfigure.Configure.FlactcPath,excelFilePath, outSchemaPath, outJsonPath, outClassPath, outBinPath);
            }
        }
        GUILayout.EndVertical();
    }

}
