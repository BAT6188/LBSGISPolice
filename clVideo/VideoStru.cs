using System;
using System.Collections.Generic;
using System.Text;

namespace clVideo
{
    struct VideoStru
    {
        public string _taskname ;
        public string _taskid ;
        public string _filepath ;
        public string _starttime;
        public string _endtime;
        public string _filesize ;
        public string _said ;

        public VideoStru(string taskname, string taskid, string filepath, string starttime, string endtime, string filesize, string said)
        {
            this._taskname = taskname;
            this._taskid = taskid;
            this._filepath = filepath;
            this._starttime = starttime;
            this._endtime = endtime;
            this._filesize = filesize;
            this._said = said;
        }
    }
}
