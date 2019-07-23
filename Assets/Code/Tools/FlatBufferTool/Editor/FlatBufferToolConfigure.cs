using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class FlatBufferToolConfigure : BaseConfigure<FlatBufferToolConfigure>
{
    //flatbuffer flatc.exe 路径
    public string FlactcPath = string.Empty;
    //excel表格目录
    public string ExcelDir = string.Empty;
    //fbs schema生成目录
    public string SchemaDir = string.Empty;
    //json数据生成目录
    public string JsonDir = string.Empty;
    //class文件生成目录
    public string ClassDir = string.Empty;
    //二进制文件生成目录
    public string BinDir = string.Empty;
    //表格读取时 属性名行号
    public int fieldNameRow = 0;
    //表格读取时 属性类型行号
    public int fieldTypeRow = 1;
    //表格读取时 属性类型行号
    public int fieldValueRow = 3;
}
