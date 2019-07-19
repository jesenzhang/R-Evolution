using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ExcelSheetData
{
    public string sheetName;
    public List<string> fieldNames;
    public List<string> fieldTypes;
    public List<string> fieldValues;

    public ExcelSheetData()
    {
        sheetName = string.Empty;
        fieldNames = new List<string>();
        fieldTypes = new List<string>();
        fieldValues = new List<string>();
    }
}

//基本数据类型
public enum FbsDataType
{
    NONE = 0,
    //标量 类型
    //8 bit: byte (int8), ubyte (uint8), bool
    BYTE = 1,
    UBYTE = 2,
    BOOL = 3,
    //16 bit: short (int16), ushort (uint16)
    SHORT = 4,
    USHORT  = 5,
    //32 bit: int (int32), uint (uint32), float (float32)
    INT = 6,
    UINT = 7,
    FLOAT = 8,
    //64 bit: long (int64), ulong (uint64), double (float64)
    LONG = 9,
    ULONG = 10,
    DOUBLE  = 11,
    //非标量的字段
    STRING = 12,
}

//fbs schema文件的组成元素类型
public enum FbsFileElement
{
    NONE = 0,
    TABLE = 1,
    STRUCT = 2,
    UNION = 3,
    ENUM = 4,
    NAMESPACE = 5,
    ATTRIBUTE = 6,
    ROOT_TYPE = 7,
    FILE_IDENTIFIER = 8,
}

//table 字段类型
public enum FbsFieldType
{
    NONE = 0,
    SCALAR = 1,
    STRUCT = 2,
    ENUM = 3,
    UNION = 4,
    TABLE = 5,
}

//应用属性类型
public enum FbsAttributeType
{
    NONE = 0,
    ID = 1,//(on a field)
    DEPRECATED = 2,//(on a field)
    REQUIRED = 3, //on a non-scalar table field
    FORCE_ALIGN = 4,//(on a struct)
    BIT_FLAGS = 5 ,//(on an enum)
    NESTED_FLATBUFFER = 6,//on a field)
    FLEXBUFFER = 7,// (on a field)
    KEY = 8,// (on a field)
    HASH = 9,// (on a field)
    ORIGINAL_ORDER = 10,// (on a table)
    CUSTOM = 11,// (on a field)
}

//属性类
public class FbsAttribute
{
    //Attributes 字段声明
    public FbsAttributeType attributeType;
    //Attributes 字段声明值
    public string attributeValue;
    //自定义属性名
    public string customName;
}

//table 字段
public class FbsTableField
{
    //字段名
    public string fieldName;
    //字段类型
    public FbsFieldType fieldType;
    //类型是 ENUM STRUCT UNION 时的类型名
    public string fieldTypeName;
    //标量的数据类型
    public FbsDataType dataType;
    //是否是数组
    public bool isArray;
    //默认值
    public string defaultValue;
    //字段属性列表
    public List<FbsAttribute> attributes;
    public FbsTableField()
    {
        fieldName = string.Empty;
        fieldTypeName = string.Empty;
        defaultValue = string.Empty;
        attributes = new List<FbsAttribute>();
    }
}
public class FbsTable
{
    public string tableName;
    public List<FbsTableField> fields;
    public bool isOriginalOrder;
    public FbsTable()
    {
        tableName = string.Empty;
        fields = new List<FbsTableField>();
    }
}
public class FbsEnumField
{
    public string fieldName;
    public string fileValue;

    public FbsEnumField()
    {
        fieldName = string.Empty;
        fileValue = string.Empty;
    }
}
public class FbsEnum
{
    public string enumName;
    public FbsDataType dataType;
    public List<FbsEnumField> enumFields;
    public bool isBitFlags;

    public FbsEnum()
    {
        enumName = string.Empty;
        enumFields = new List<FbsEnumField>();
    }
}
public class FbsStructField
{
    public string fieldName;
    public FbsDataType dataType;
    public FbsStructField()
    {
        fieldName = string.Empty;
    }
}
public class FbsStruct
{
    public string structName;
    public List<FbsStructField> structFields;
    public bool isForceAlign;
    public int forceAlignSize;
    public FbsStruct()
    {
        structName = string.Empty;
        structFields = new List<FbsStructField>();
    }
}
public class FbsUnion
{
    public string unionName;
    public List<string> unionTables;

    public FbsUnion()
    {
        unionName = string.Empty;
        unionTables = new List<string>();
    }
}
public class FbsFile
{
    public string fileName;
    public string namespaceName;
    public List<FbsStruct> structs;
    public List<FbsEnum> enums;
    public List<FbsUnion> unions;
    public List<FbsTable> tables;
    public List<string> customAttributes;
    public string root_type;
    public string file_identifier;

    public FbsFile()
    {
        fileName = string.Empty;
        namespaceName = string.Empty;
        root_type = string.Empty;
        file_identifier = string.Empty;
        structs = new List<FbsStruct>();
        enums = new List<FbsEnum>();
        unions = new List<FbsUnion>();
        tables = new List<FbsTable>();
        customAttributes = new List<string>();
    }
}

public class FileGenerater
{
    private static StringBuilder sb;
    private static StringBuilder StrBuilder
    {
        get {
            if (sb == null)
                sb = new StringBuilder();
            return sb;
        }
    }

    /// <summary>
    /// Determines a text file's encoding by analyzing its byte order mark (BOM).
    /// Defaults to ASCII when detection of the text file's endianness fails.
    /// </summary>
    /// <param name="filename">The text file to analyze.</param>
    /// <returns>The detected encoding.</returns>
    public static Encoding GetEncoding(string filename)
    {
        // Read the BOM
        var bom = new byte[4];
        using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
        {
            file.Read(bom, 0, 4);
        }

        // Analyze the BOM
        if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
        if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
        if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
        if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
        if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
        return Encoding.ASCII;
    }

    #region  table文本生成
    //生成基础类型文本
    public static string GetFbsDataTypeString(FbsDataType dataType, bool isArray)
    {
        string name = Enum.GetName(typeof(FbsDataType), dataType).ToLower();
        return isArray ? string.Format("[{0}]", name) : name;
        /*switch (dataType)
        {
            case FbsDataType.BOOL:
                return isArray ? "[bool]" : "bool";
            case FbsDataType.BYTE:
                return isArray ? "[byte]" : "byte";
            case FbsDataType.DOUBLE:
                return isArray ? "[double]" : "double";
            case FbsDataType.FLOAT:
                return isArray ? "[float]" : "float";
            case FbsDataType.INT:
                return isArray ? "[int]" : "int";
            case FbsDataType.LONG:
                return isArray ? "[long]" : "long"; 
            case FbsDataType.SHORT:
                return isArray ? "[short]" : "short";
            case FbsDataType.STRING:
                return isArray ? "[string]" : "string";
            case FbsDataType.UBYTE:
                return isArray ? "[ubyte]" : "ubyte";
            case FbsDataType.UINT:
                return isArray ? "[uint]" : "uint";
            case FbsDataType.ULONG:
                return isArray ? "[ulong]" : "ulong";
            case FbsDataType.USHORT:
                return isArray ? "[ushort]" : "ushort";
            case FbsDataType.NONE:
                return "null";
            default:
                return "null";
        }*/
    }

    public static void GetFbsDataTypeByString(string orignaldataTypeString,ref FbsFieldType fieldType,ref string fieldTypeName, ref FbsDataType fbsDataType,ref bool isArray)
    {
        string dataTypeString = orignaldataTypeString.ToLower();
        //数组检查
        int indexleft = dataTypeString.IndexOf("[");
        int indexright = dataTypeString.IndexOf("]");
        if (indexleft == 0 && (indexright == dataTypeString.Length - 1))
        {
            isArray = true;
            orignaldataTypeString = orignaldataTypeString.Substring(1, orignaldataTypeString.Length - 2);
            dataTypeString = dataTypeString.Substring(1, dataTypeString.Length - 2);
        }
        else
        {
            isArray = false;
        }

        //检查标量
        string[] names = Enum.GetNames(typeof(FbsDataType));
        for (int i = 0; i < names.Length; i++)
        {
            string name = names[i].ToLower();
            int index = dataTypeString.IndexOf(name);
            if (index == 0 && name.Length == dataTypeString.Length)
            {
                fieldType = FbsFieldType.SCALAR;
                fbsDataType = (FbsDataType)Enum.Parse(typeof(FbsDataType), name.ToUpper());
                fieldTypeName = string.Empty;
                return;
            }
        }
        //检查Enum 
        int enumindex = dataTypeString.IndexOf("enum:");
        if (enumindex == 0)
        {
            fieldType = FbsFieldType.ENUM;
            fieldTypeName = orignaldataTypeString.Substring(5, orignaldataTypeString.Length-5);
            fbsDataType = FbsDataType.NONE;
            return;
        }
     
        //检查Struct
        int structindex = dataTypeString.IndexOf("struct:");
        if (structindex == 0)
        {
            fieldType = FbsFieldType.STRUCT;
            fieldTypeName = orignaldataTypeString.Substring(7, orignaldataTypeString.Length - 7);
            fbsDataType = FbsDataType.NONE;
            return;
        }

        //检查Union
        int unionindex = dataTypeString.IndexOf("union:");
        if (unionindex == 0)
        {
            fieldType = FbsFieldType.UNION;
            fieldTypeName = orignaldataTypeString.Substring(6, orignaldataTypeString.Length - 6);
            fbsDataType = FbsDataType.NONE;
            return;
        }

        //检查Table
        int tableindex = dataTypeString.IndexOf("table:");
        if (tableindex == 0)
        {
            fieldType = FbsFieldType.TABLE;
            fieldTypeName = orignaldataTypeString.Substring(6, orignaldataTypeString.Length - 6);
            fbsDataType = FbsDataType.NONE;
            return;
        }

        fieldType = FbsFieldType.NONE;
        fbsDataType = FbsDataType.NONE;
        fieldTypeName = string.Empty;
        isArray = false;
        /*
        switch (dataTypeString)
        {
            case "bool":
                {
                    fbsDataType = FbsDataType.BOOL;
                    isArray = false;
                    break;
                }
            case "[bool]":
                {
                    fbsDataType = FbsDataType.BOOL;
                    isArray = true;
                    break;
                }
            case "byte":
                {
                    fbsDataType = FbsDataType.BYTE;
                    isArray = false;
                    break;
                }
            case "[byte]":
                {
                    fbsDataType = FbsDataType.BYTE;
                    isArray = true;
                    break;
                }
            case "double":
                {
                    fbsDataType = FbsDataType.DOUBLE;
                    isArray = false;
                    break;
                };
            case "[double]":
                {
                    fbsDataType = FbsDataType.DOUBLE;
                    isArray = true;
                    break;
                };
            case "float":
                {
                    fbsDataType = FbsDataType.FLOAT;
                    isArray = false;
                    break;
                };
            case "[float]":
                {
                    fbsDataType = FbsDataType.FLOAT;
                    isArray = true;
                    break;
                };
            case "int":
                {
                    fbsDataType = FbsDataType.INT;
                    isArray = false;
                    break;
                };
            case "[int]":
                {
                    fbsDataType = FbsDataType.INT;
                    isArray = true;
                    break;
                };
            case "long":
                {
                    fbsDataType = FbsDataType.LONG;
                    isArray = false;
                    break;
                };
            case "[long]":
                {
                    fbsDataType = FbsDataType.LONG;
                    isArray = true;
                    break;
                };
            case "short":
                {
                    fbsDataType = FbsDataType.SHORT;
                    isArray = false;
                    break;
                };
            case "[short]":
                {
                    fbsDataType = FbsDataType.SHORT;
                    isArray = true;
                    break;
                };
            case "string":
                {
                    fbsDataType = FbsDataType.STRING;
                    isArray = false;
                    break;
                };
            case "[string]":
                {
                    fbsDataType = FbsDataType.STRING;
                    isArray = true;
                    break;
                };
            case "ubyte":
                {
                    fbsDataType = FbsDataType.UBYTE;
                    isArray = false;
                    break;
                };
            case "[ubyte]":
                {
                    fbsDataType = FbsDataType.UBYTE;
                    isArray = true;
                    break;
                };
            case "uint":
                {
                    fbsDataType = FbsDataType.UINT;
                    isArray = false;
                    break;
                };
            case "[uint]":
                {
                    fbsDataType = FbsDataType.UINT;
                    isArray = true;
                    break;
                };
            case "ulong":
                {
                    fbsDataType = FbsDataType.ULONG;
                    isArray = false;
                    break;
                };
            case "[ulong]":
                {
                    fbsDataType = FbsDataType.ULONG;
                    isArray = true;
                    break;
                };
            case "ushort":
                {
                    fbsDataType = FbsDataType.USHORT;
                    isArray = false;
                    break;
                };
            case "[ushort]":
                {
                    fbsDataType = FbsDataType.USHORT;
                    isArray = true;
                    break;
                };
            case "":
                {
                    fbsDataType = FbsDataType.NONE;
                    isArray = false;
                    break;
                }
            default:
                {
                    fbsDataType = FbsDataType.NONE;
                    isArray = false;
                    break;
                }
        }
        */
    }


    public static string GetTableFieldTypeString(FbsTableField field)
    {
        return field.fieldType == FbsFieldType.SCALAR ? GetFbsDataTypeString(field.dataType, field.isArray) : field.isArray ? string.Format("[{0}]", field.fieldTypeName) : field.fieldTypeName;
    }
    //生成table 字段属性文本
    public static void GenTableFieldAttributes(FbsTableField field, ref string attributeString)
    {
        if (field.attributes != null && field.attributes.Count > 0)
        {
            attributeString = "(";
            for (int i = 0; i < field.attributes.Count; i++)
            {
                FbsAttribute fieldAttribute = field.attributes[i];
                if (fieldAttribute.attributeType == FbsAttributeType.ID && fieldAttribute.attributeValue != string.Empty)
                {
                    attributeString += string.Format("id:{0},", fieldAttribute.attributeValue);
                }
                else if (fieldAttribute.attributeType == FbsAttributeType.DEPRECATED)
                {
                    attributeString += "deprecated,";
                }
                else if (fieldAttribute.attributeType == FbsAttributeType.REQUIRED && field.fieldType != FbsFieldType.SCALAR)
                {
                    attributeString += "required,";
                }
                else if (fieldAttribute.attributeType == FbsAttributeType.NESTED_FLATBUFFER && fieldAttribute.attributeValue != string.Empty)
                {
                    attributeString += string.Format("nested_flatbuffer: \"{0}\",", fieldAttribute.attributeValue);
                }
                else if (fieldAttribute.attributeType == FbsAttributeType.FLEXBUFFER && field.dataType == FbsDataType.UBYTE && field.isArray)
                {
                    attributeString += "flexbuffer,";
                }
                else if (fieldAttribute.attributeType == FbsAttributeType.KEY)
                {
                    attributeString += "key,";
                }
                else if (fieldAttribute.attributeType == FbsAttributeType.HASH && (field.dataType == FbsDataType.UINT || field.dataType == FbsDataType.ULONG))
                {
                    attributeString += "hash,";
                }
                else if (fieldAttribute.attributeType == FbsAttributeType.CUSTOM )
                {
                    attributeString += fieldAttribute.attributeValue== string.Empty ? string.Format("{0},", fieldAttribute.customName) : string.Format("{0}:{1},",fieldAttribute.customName,fieldAttribute.attributeValue);
                }
            }
            attributeString = attributeString.Substring(0, attributeString.Length - 1);
            attributeString += ")";
        }
    }
    //写入table的field
    public static void GenFbsSchemaField(FbsTableField field,ref StringBuilder stringBuilder)
    {
        string typestring = field.fieldType == FbsFieldType.SCALAR ? GetFbsDataTypeString(field.dataType, field.isArray) : field.isArray ? string.Format("[{0}]", field.fieldTypeName) : field.fieldTypeName;
        string defaultvalue = string.Empty;
        if (field.defaultValue != string.Empty && !field.isArray && field.fieldType != FbsFieldType.UNION)
        {
            defaultvalue = " = " + field.defaultValue;
        }
        string attributeString = string.Empty;
        GenTableFieldAttributes(field, ref attributeString);
        stringBuilder.AppendFormat("  {0}:{1}{2}{3};\n", field.fieldName, typestring, defaultvalue, attributeString);
    }
    public static void GenFbsTable(FbsTable fbsTable ,ref StringBuilder stringBuilder)
    {
        //表结构
        if(fbsTable.isOriginalOrder)
            stringBuilder.AppendFormat("table {0} (original_order) {1}\n", fbsTable.tableName, "{");
        else
            stringBuilder.AppendFormat("table {0} {1}\n", fbsTable.tableName,"{");
       
        if (fbsTable.fields != null && fbsTable.fields.Count > 0)
        {
            int count = fbsTable.fields.Count;
            for (int i = 0; i < count; i++)
            {
                FbsTableField field = fbsTable.fields[i];
                GenFbsSchemaField(field, ref stringBuilder);
            }
        }
        stringBuilder.Append("}\n\n");
    }
    #endregion

    #region Enum文本生成
    public static void GenFbsEnum(FbsEnum fbsEnum, ref StringBuilder stringBuilder)
    {
        if (fbsEnum.isBitFlags)
            stringBuilder.AppendFormat("enum {0} : {1} (bit_flags) {2}\n", fbsEnum.enumName, GetFbsDataTypeString(fbsEnum.dataType,false),"{");
        else
            stringBuilder.AppendFormat("enum {0} : {1}{2}\n", fbsEnum.enumName, GetFbsDataTypeString(fbsEnum.dataType, false), "{");

        if (fbsEnum.enumFields != null && (fbsEnum.enumFields.Count > 0))
        {
            int Count = fbsEnum.enumFields.Count;
            for (int i = 0; i < Count; i++)
            {
                FbsEnumField field = fbsEnum.enumFields[i];
                if (field.fileValue != string.Empty)
                    stringBuilder.AppendFormat("  {0} : {1},\n", field.fieldName,field.fileValue);
                else
                    stringBuilder.AppendFormat("  {0},\n", field.fieldName);
            }
        }
       // stringBuilder.Remove(stringBuilder.Length - 1, 1);
        stringBuilder.Append("}\n\n");
    }
    #endregion

    #region union文本生成
    public static void GenFbsUnion(FbsUnion fbsUnion, ref StringBuilder stringBuilder)
    {
        stringBuilder.AppendFormat("union {0} {1}\n", fbsUnion.unionName, "{");

        if (fbsUnion.unionTables != null && (fbsUnion.unionTables.Count > 0))
        {
            int Count = fbsUnion.unionTables.Count;
            for (int i = 0; i < Count; i++)
            {
                string field = fbsUnion.unionTables[i];
                stringBuilder.AppendFormat("  {0},\n", field);
            }
        }
        // stringBuilder.Remove(stringBuilder.Length - 1, 1);
        stringBuilder.Append("}\n\n");
    }
    #endregion

    #region struct文本生成
    public static void GenFbsStruct(FbsStruct fbsStruct, ref StringBuilder stringBuilder)
    {
        stringBuilder.AppendFormat("struct {0} {1}\n", fbsStruct.structName, "{");
        if (fbsStruct.isForceAlign)
            stringBuilder.AppendFormat("struct {0} (force_align : {1}) {2}\n", fbsStruct.structName, fbsStruct.forceAlignSize, "{");
        else
            stringBuilder.AppendFormat("struct {0} {1}\n", fbsStruct.structName, "{");

        if (fbsStruct.structFields != null && (fbsStruct.structFields.Count > 0))
        {
            int Count = fbsStruct.structFields.Count;
            for (int i = 0; i < Count; i++)
            {
                FbsStructField field = fbsStruct.structFields[i];
                stringBuilder.AppendFormat("  {0} : {1};\n", field.fieldName, GetFbsDataTypeString(field.dataType, false));
            }
        }
        // stringBuilder.Remove(stringBuilder.Length - 1, 1);
        stringBuilder.Append("}\n\n");
    }
    #endregion

    //flatbuff schema文件生成
    public static void GenFbsSchemaFile(FbsFile fbsFile,string fileOutDir,ref string fileFullpath)
    {
        //---------生成FBS文件---------
        StringBuilder stringBuilder = StrBuilder;
        stringBuilder.Clear();
        //spacename
        if (fbsFile.namespaceName != string.Empty)
            stringBuilder.AppendFormat("namespace {0};\n\n", fbsFile.namespaceName);

        //customAttribute
        if (fbsFile.customAttributes !=null && fbsFile.customAttributes.Count > 0)
        {
            for (int i = 0; i < fbsFile.customAttributes.Count; i++)
            {
                string field = fbsFile.customAttributes[i];
                stringBuilder.AppendFormat("attribute \"{0}\";\n", field);
            }
        }
        //Enums
        if (fbsFile.enums != null && fbsFile.enums.Count > 0)
        {
            for (int i = 0; i < fbsFile.enums.Count; i++)
            {
                FbsEnum fbsenum = fbsFile.enums[i];
                GenFbsEnum(fbsenum, ref stringBuilder);
            }
        }
        //Structs
        if (fbsFile.structs != null && fbsFile.structs.Count > 0)
        {
            for (int i = 0; i < fbsFile.structs.Count; i++)
            {
                FbsStruct fbsStruct = fbsFile.structs[i];
                GenFbsStruct(fbsStruct, ref stringBuilder);
            }
        }

        //Unions
        if (fbsFile.unions != null && fbsFile.unions.Count > 0)
        {
            for (int i = 0; i < fbsFile.unions.Count; i++)
            {
                FbsUnion fbsUnion = fbsFile.unions[i];
                GenFbsUnion(fbsUnion, ref stringBuilder);
            }
        }

        //tables
        if (fbsFile.tables != null && fbsFile.tables.Count > 0)
        {
            for (int i = 0; i < fbsFile.tables.Count; i++)
            {
                FbsTable fbstable = fbsFile.tables[i];
                GenFbsTable(fbstable, ref stringBuilder);
            }
        }
        //根类型
        if (fbsFile.root_type != string.Empty)
            stringBuilder.AppendFormat("root_type {0};\n", fbsFile.root_type);
        else
            Debug.LogError(fbsFile.fileName + " root_type is Empty!!!!");
        // 文件标识 
        if (fbsFile.file_identifier != string.Empty)
            stringBuilder.AppendFormat("file_identifier \"{0}\";\n", fbsFile.file_identifier);

        fileFullpath = fileOutDir + "/"+fbsFile.fileName+".fbs";
        if (!Directory.Exists(fileOutDir))
        {
            Directory.CreateDirectory(fileOutDir);
        }
        if (File.Exists(fileFullpath))
        {
            File.Delete(fileFullpath);
        }
        FileStream fs = new FileStream(fileFullpath, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
        sw.Write(stringBuilder.ToString());
        sw.Close();
        fs.Close();
        stringBuilder.Clear();
    }

    public static FbsTable GetFbsRootTableDataTypeObj(FbsFile fbsfile)
    {
        FbsTable fbsTable = null;
        foreach (var table in fbsfile.tables)
        {
            if (table.tableName == fbsfile.fileName)
            {
                fbsTable = table;
                break;
            }
        }
        return fbsTable;
    }

    //生成给定类型table的数组组成的json 
    public static void GenJsonFile(FbsFile fbsfile, List<string> fieldValues, string fileOutDir,ref string jsonPath)
    {
        //生成json文件
        StringBuilder jsonBuilder =  StrBuilder;
        jsonBuilder.Clear();
        jsonBuilder.Append("{\n  \"data\":[\n");
        FbsTable fbsTable = GetFbsRootTableDataTypeObj(fbsfile);
        if (fbsTable == null)
        {
            Debug.LogErrorFormat("can not find table named ", fbsfile.fileName);
            return;
        }
        int fieldCount = fbsTable.fields.Count;
        int dataCount = Mathf.FloorToInt(fieldValues.Count / fieldCount);
        for (int k = 0; k < fieldValues.Count; k++)
        {
            string value = fieldValues[k];
            int fieldIndex = k % fieldCount;
            int dataIndex = Mathf.FloorToInt(k / fieldCount);
            FbsTableField tableField = fbsTable.fields[fieldIndex];
            string fieldName = tableField.fieldName;
            if (fieldIndex == 0)
            {
                jsonBuilder.Append("    {\n");
            }
            bool isstring = !(tableField.fieldType == FbsFieldType.SCALAR && tableField.dataType != FbsDataType.STRING);
            string valuecolon = isstring && !tableField.isArray ? "\"" : "";
            string valuearrayL = tableField.isArray ? "[" : "";
            string valuearrayR = tableField.isArray ? "]" : "";
            string jsonValue = value;
            if (isstring && tableField.isArray)
            {
                string[] condition = { "[/]" };
                string[] vs = value.Split(condition, StringSplitOptions.RemoveEmptyEntries);
                jsonValue = "";
                for (int m = 0; m < vs.Length; m++)
                {
                    jsonValue += "\"" + vs[m] + "\",";
                }
                jsonValue = jsonValue.Substring(0, jsonValue.Length - 1);
            }
            string lastCommon = (fieldIndex == (fieldCount - 1)) ? "" : ",";
            jsonBuilder.AppendFormat("		\"{0}\":{1}{2}{3}{4}{5}{6}\n", fieldName, valuearrayL, valuecolon, jsonValue, valuecolon, valuearrayR, lastCommon);

            if (fieldIndex == fieldCount - 1)
            {
                if (dataIndex == dataCount - 1)
                {
                    jsonBuilder.Append("	}\n");
                }
                else
                {
                    jsonBuilder.Append("	},\n");
                }
            }
        }
        jsonBuilder.Append("	]\n}");
        jsonPath = fileOutDir+"/" + fbsTable.tableName + ".json";
        if (!Directory.Exists(fileOutDir))
        {
            Directory.CreateDirectory(fileOutDir);
        }
        if (File.Exists(jsonPath))
        {
            File.Delete(jsonPath);
        }
        FileStream fs = new FileStream(jsonPath, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
        sw.Write(jsonBuilder.ToString());
        sw.Close();
        fs.Close();
        jsonBuilder.Clear();
    }

    public static void ReadExcel(string excelFilePath, ref List<ExcelSheetData> excelSheetDatas)
    {
        using (var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream))
            {
                do
                {
                    string tableName = reader.Name;
                    tableName = tableName.Trim();
                    int isValid = tableName.IndexOf("=");
                    if (isValid < 0)
                        continue;
                    if (reader.RowCount < 2)
                        continue;
                    ExcelSheetData excelSheetData = new ExcelSheetData();
                    List<string> fieldNames = excelSheetData.fieldNames;
                    List<string> fieldTypes = excelSheetData.fieldTypes;
                    List<string> fieldValues = excelSheetData.fieldValues;
                    excelSheetData.sheetName = tableName;
                    for (int row = 0; row < reader.RowCount; row++)
                    {
                        reader.Read();
                        for (int col = 0; col < reader.FieldCount; col++)
                        {
                            //前两行是字段定义
                            if (row == 0)
                            {
                                fieldNames.Add(("" + (string)reader.GetValue(col)).Trim());
                            }
                            else
                            if (row == 1)
                            {
                                fieldTypes.Add(("" + (string)reader.GetValue(col)).Trim());
                            }
                            else
                            if (row >= 3)
                            {
                                fieldValues.Add(("" + reader.GetValue(col)).Trim());
                            }
                            object str = (object)reader.GetValue(col);
                        }
                    }
                    excelSheetDatas.Add(excelSheetData);
                } while (reader.NextResult());
            }
        }
    }

    public static FbsFile GenFbsFileObject(ExcelSheetData excelSheetData)
    {
        //一个sheet 一个table
        FbsFile fbsFile = new FbsFile
        {
            fileName = excelSheetData.sheetName.Replace("=", "")
        };
        FbsTable fbsTable = new FbsTable
        {
            tableName = fbsFile.fileName
        };
        //处理table字段
        for (int j = 0; j < excelSheetData.fieldNames.Count; j++)
        {
            FbsTableField fbsTableField = new FbsTableField();
            fbsTableField.fieldName = excelSheetData.fieldNames[j];
            string dataTypeStr = excelSheetData.fieldTypes[j];
            FileGenerater.GetFbsDataTypeByString(dataTypeStr, ref fbsTableField.fieldType, ref fbsTableField.fieldTypeName, ref fbsTableField.dataType, ref fbsTableField.isArray);
            fbsTable.fields.Add(fbsTableField);
        }
        fbsFile.tables.Add(fbsTable);
        //添加root_type table
        FbsTable fbsRootTable = new FbsTable();
        fbsRootTable.tableName = "Root_" + fbsFile.fileName;
        FbsTableField dataField = new FbsTableField();
        dataField.fieldName = "data";
        dataField.fieldType = FbsFieldType.TABLE;
        dataField.isArray = true;
        dataField.fieldTypeName = fbsTable.tableName;
        fbsRootTable.fields.Add(dataField);
        fbsFile.tables.Add(fbsRootTable);
        //root_type
        fbsFile.root_type = fbsRootTable.tableName;
        fbsFile.namespaceName = "GameDataTables";
        return fbsFile;
    }
 
    public static void BuildAllFromExcel(string flatcPath,string excelFilePath, string outSchemaPath, string outJsonPath, string outClassPath, string outBinPath)
    {
        List<ExcelSheetData> excelSheetDatas = new List<ExcelSheetData>();
        ReadExcel(excelFilePath, ref excelSheetDatas);

        foreach (var sheet in excelSheetDatas)
        {
            FbsFile fbsFile = GenFbsFileObject(sheet);
            string schemaPath = string.Empty;
            string jsonPath = string.Empty;
            FileGenerater.GenFbsSchemaFile(fbsFile, outSchemaPath, ref schemaPath);

            FileGenerater.GenJsonFile(fbsFile, sheet.fieldValues, outJsonPath, ref jsonPath);

            if (!Directory.Exists(outClassPath))
            {
                Directory.CreateDirectory(outClassPath);
            }
            if (!Directory.Exists(outBinPath))
            {
                Directory.CreateDirectory(outBinPath);
            }
            CmdHelper.RunFlatC(flatcPath, schemaPath, jsonPath, outClassPath, outBinPath);
        }
    }
}
