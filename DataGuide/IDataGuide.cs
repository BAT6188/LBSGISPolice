using System;
namespace LBSDataGuide
{
    interface IDataGuide
    {
        int InData(string dataPath, string tableName);
        bool OutData(string dataPath, string tableName);
    }
}
