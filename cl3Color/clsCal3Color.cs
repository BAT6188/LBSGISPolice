using System;
using System.Collections.Generic;
using System.Text;

namespace cl3Color
{
    class clsCal3Color
    {
        public double  a1, a2, a3, a0;

        public double  A1
        {
            get { return a1; }
            set { a1 = value; }
        }

        public double A2
        {
            get { return a2; }
            set { a2 = value; }
        }

        public double A3
        {
            get { return a3; }
            set { a3 = value; }
        }

        public double A0
        {
            get { return a0; }
            set { a0 = value; }
        }

        public void CalYujingValue(DateTime timeFrom,DateTime timeTo)
        {
            TimeSpan ts = timeFrom -timeTo;
            TimeSpan ts2 = new TimeSpan(ts.Days * 3, 0, 0, 0);
            DateTime dt1 = timeFrom.Add(ts2);      //三个周期前的时间

            string strExp1 = "select count(*) from 案件信息 where 发案时间初值>=to_date('"+dt1.ToString()+"','yyyy-mm-dd hh24:mi:ss') and 发案时间初值<to_date('"+timeFrom.ToString()+"','yyyy-mm-dd hh24:mi:ss')";
        }

    }
}
