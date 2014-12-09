using System;
using System.Collections.Generic;
using System.Text;

namespace clGPSPolice
{
    class TaskForce     // 重大任务执行组对象
    {
        private string taskForceNum;   // 任务组编号

        public string TaskForceNum
        {
            get { return taskForceNum; }
            set { taskForceNum = value; }
        }

        private string taskForceName;  // 任务组名称

        public string TaskForceName
        {
            get { return taskForceName; }
            set { taskForceName = value; }
        }

        private string majorName;      // 执行任务名称

        public string MajorName
        {
            get { return majorName; }
            set { majorName = value; }
        }

        private string majorUntis;     // 执行任务单位

        public string MajorUntis
        {
            get { return majorUntis; }
            set { majorUntis = value; }
        }

        private string commanderID;   // 指挥员肩咪ID

        public string CommanderID
        {
            get { return commanderID; }
            set { commanderID = value; }
        }

        private string standCommID;   // 备用肩咪ID

        public string StandCommID
        {
            get { return standCommID; }
            set { standCommID = value; }
        }

        private string executiveRen;  // 参加任务人数

        public string ExecutiveRen
        {
            get { return executiveRen; }
            set { executiveRen = value; }
        }
    }
}
