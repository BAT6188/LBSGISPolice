using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using MapInfo.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using MapInfo.Windows.Controls;

namespace clKaKou
{
    public partial class FrmInfo : Form
    {
        Boolean UseDll = true;

        public string[] StrCon;         // 数据库连接字符串数组
        public string UserName;         // 登陆用户姓名
        public string photoserver;      // 图片服务器地址
        public MapControl mapControl;   // 地图控件
        public string getFromNamePath;  // GetFromNameConfig.ini的地址
        private string mysql;           // 连接字符串
        public string layerName;        // 图层名称

        GDW_GIS_Interface.communication gdwcom = new GDW_GIS_Interface.communication(); //高德威接口

        public FrmInfo()
        {
            InitializeComponent();
        }
       
        public void setInfo(DataRow dRow,Point pt,string[] Constr,string un)
        {
            try
            {
                StrCon = Constr;
                CLC.DatabaseRelated.OracleDriver.CreateConstring(Constr[0], Constr[1], Constr[2]);
                mysql = CLC.DatabaseRelated.OracleDriver.GetConString;
                UserName = un;

                this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                this.dataGridView1.Rows.Clear();

                foreach (DataColumn col in dRow.Table.Columns)
                {
                    if (col.Caption.IndexOf("备用字段") < 0)
                    {
                        if (col.Caption.IndexOf("照片") < 0 && col.Caption !="车辆号牌" )
                        {
                            this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);
                        }
                        else
                        {
                            if ((col.Caption == "车辆号牌" && dRow[col].ToString().Length > 0)|| (col.Caption == "照片1" && dRow[col].ToString().Length > 0) 
                                                                                              || (col.Caption == "照片2" && dRow[col].ToString().Length > 0)
                                                                                              || (col.Caption == "照片3" && dRow[col].ToString().Length > 0))
                            {
                                DataGridViewLinkCell dglc1 = new DataGridViewLinkCell();

                                dglc1.Value = "查看 " + col.Caption + " 的信息";

                                if (col.Caption == "车辆号牌")
                                    dglc1.Value = dRow[col].ToString();

                                dglc1.ToolTipText =dRow[col].ToString();

                                this.dataGridView1.Rows.Add(col.Caption + ":", dRow[col]);

                                this.dataGridView1.Rows[this.dataGridView1.Rows.Count - 1].Cells[1] = dglc1;
                            }                           
                        }
                    }
                }

                this.setSize();

                this.setLocation(this.Width, this.Height, pt.X, pt.Y);           //设置位置

                int ki=0;
                for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                {
                    if (this.dataGridView1.Columns[i].ToString().IndexOf("处理状态") > 0)
                    {
                        ki = i;
                    }
                }

                if (dRow.Table.Columns[0].Caption == "报警编号" && this.dataGridView1.Rows[ki].Cells[1].Value.ToString() != "已处理")
                {
                    this.panel3.Visible = true;
                }
                else
                {
                    this.panel3.Visible = false;
                }

                this.TopMost = true;

                this.Visible = true;

                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                }
            }
            catch(Exception ex) {
                writeToLog(ex, "clKaKou-frmInfo-01-setInfo");
            }
        }

        private void setSize()
        {
            try
            {
                for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                {
                    if (this.dataGridView1.Columns[i].Width > 300)
                    {
                        this.dataGridView1.Columns[1].Width = 300;
                        this.dataGridView1.Columns[1].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
                    }
                }

                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    double width = this.dataGridView1.Columns[1].DefaultCellStyle.Font.Size + 2;

                    int n = this.dataGridView1.Rows[i].Cells[1].Value.ToString().Length;
                    if (width * n > 195)
                    {
                        n = (int)(width * n);
                        double d = n / 300.0;
                        n = (int)Math.Ceiling(d) + 1;

                        this.dataGridView1.Rows[i].Height = (this.dataGridView1.Rows[i].Height - 6) * n;
                    }
                }

                int cMessageWidth = 0;

                cMessageWidth = this.dataGridView1.Columns[0].Width + this.dataGridView1.Columns[1].Width + 30;

                if (this.dataGridView1.Columns[1].Width == 300)
                {
                    cMessageWidth = this.dataGridView1.Columns[0].Width + 330;
                }

                int cMessageHeight = 0;
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    cMessageHeight += this.dataGridView1.Rows[i].Height;
                }
                cMessageHeight += 100;
                this.Size = new Size(cMessageWidth, cMessageHeight);  //设置大小
            }
            catch(Exception ex)
            {
                writeToLog(ex, "clKaKou-frmInfo-02-setSize");
            }
        }


        private void setLocation(int iWidth,int iHeight,int x,int y) {
            try
            {
                if (x + iWidth > Screen.PrimaryScreen.WorkingArea.Width)
                {
                    x = x - iWidth - 10;
                }
                if (y + iHeight > Screen.PrimaryScreen.WorkingArea.Height)
                {
                    y = y - iHeight - 10;
                    if (y < 0) y = 0;
                }
                this.SetDesktopLocation(x, y);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmInfo-03-setLocation");
            }
        }


        /// <summary>
        /// 查看图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "照片1:") || (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "照片2:") || (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "照片3:"))
            {
                string photoip = string.Empty;

                if (this.dataGridView1.Rows[0].Cells[0].Value.ToString() == "报警编号:")
                {
                    if (this.dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString().IndexOf("照片") > -1)
                    {
                        photoip = this.dataGridView1.Rows[e.RowIndex].Cells[1].ToolTipText;
                    }
                }
                else if (dataGridView1.Rows[0].Cells[0].Value.ToString() == "通行车辆编号:")
                {
                    if (this.dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString().IndexOf("照片") > -1)
                    {
                        photoip = photoserver + this.dataGridView1.Rows[e.RowIndex].Cells[1].ToolTipText;
                    }
                }

                //为了防止以前的数据中存在不规则字符或者ftp服务器地址或者http服务器地址出现错误，在此进行错误字符Replace。
                if (photoip.IndexOf("\\") > 0)
                {
                    photoip = photoip.Replace("\\", "/");
                }
               // writeToLog("photoip: " + photoip); 

                try
                {
                    FrmImage fimage = new FrmImage();
                    fimage.pictureBox1.Image = Image.FromFile(Application.StartupPath + "\\wait.gif");
                   // writelog("打开图片前的图片服务器地址 photoip: " + photoip);
                    fimage.pictureBox1.Image = Image.FromStream(System.Net.WebRequest.Create(photoip).GetResponse().GetResponseStream());
                    fimage.TopMost = true;
                    fimage.username = this.UserName;
                    fimage.SQLCON = this.StrCon;
                    fimage.ShowDialog();

                    WriteEditLog("查看照片 " + photoip, "查看照片", "详细信息");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("打开图片时发生错误" + photoip, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    writeToLog(ex, "clKaKou-frmInfo-04-查看图片："+photoip);
                    return;
                }
            }
            else if (e.ColumnIndex == 1 && this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() == "车辆号牌:")
            {
                System.Data.OracleClient.OracleConnection oraconn = new System.Data.OracleClient.OracleConnection("User ID=sdgis_cx;Password=!giscx123;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.47.227.29)(PORT=1521)))(CONNECT_DATA=(SID=sdgazyk)))");
                //System.Data.OracleClient.OracleConnection oraconn = new System.Data.OracleClient.OracleConnection(mysql);
                if (oraconn.State == ConnectionState.Open)
                    oraconn.Close();
                try
                {

                   //                                                                                         暂住所详细地址	暂住邮政编码	所有权	初次登记日期	最近定检日期	检验有效期止	强制报废期止	发证机关	管理部门	发牌日期	发行驶证日期	发登记证书日期	发合证日期	保险止日期	补领号牌次数	补领行驶证次数	补_换领证书次数	登记证书编号	制登记证书行数	档案编号	管理辖区	机动车状态	自定义状态	抵押标志	经办人	车辆来源	注册流水号	发动机型号	燃料种类	排量	功率	转向形式	车廓	车外廓宽	车外廓高	货箱内部长度	货箱内部宽度	货箱内部高度	钢板弹簧片数	轴数	轴距	前轮距	后轮距	轮胎规格	轮胎数	总质量	整备质量	核定载质量	核定载客人数	准牵引质量	驾驶室前排载客人数	驾驶室后排载客人数	环保达标情况	出厂日期	获得方式	来历凭证1	凭证编号1	来历凭证2	凭证编号2	销售单位	销售价格	销售日期	进口凭证	进凭证编号	合格证编号	纳税证明	纳税证明编号																											
                   //XH as 机动车序号,HPZL as 号牌种类,	HPHM,号牌号码,	CLPP1 as 中文品牌,	CLXH as 车辆型号,	CLPP2 as 英文品牌,	GCJK as 过程_进口,	ZZG as 制造国,,	ZZCMC as 制造厂名称 ,	CLSBDH as 车辆识别代号 ,	FDJH as 发动机号,
                   // CLLX 车辆类型,	CSYS 车身颜色,	SYXZ 使用性质,	SFZMHM 身份证明号码,	SFZMMC身份证明种类名称,	SYR机动车所有人,	ZSXZQH住所行政区划,	ZSXXDZ登记住所详细地址,	YZBM1住所邮政编码,	LXDH联系电话,	ZZZ居住暂住证明,
                   //ZZXZQH暂住行政区划,	ZZXXDZ	YZBM2	SYQ	CCDJRQ	DJRQ	YXQZ	QZBFQZ	FZJG	GLBM	FPRQ	FZRQ	FDJRQ	FHGZRQ	BXZZRQ	BPCS	BZCS	BDJCS	DJZSBH	ZDJZSHS	DABH	XZQH	ZT	ZDYZT	DYBJ	JBR	CLLY	LSH	FDJXH	RLZL	PL	GL	ZXXS	CWKC	CWKK	CWKG	HXNBCD	HXNBKD	HXNBGD	GBTHPS	ZS	ZJ	QLJ	HLJ	LTGG	LTS	ZZL	ZBZL	HDZZL	HDZK	ZQYZL	QPZK	HPZK	HBDBQK	CCRQ	HDFS	LLPZ1	PZBH1	LLPZ2	PZBH2	XSDW	XSJG	XSRQ	JKPZ	JKPZHM	HGZBH	NSZM	NSZMBH	GXRQ	XGZL	QMBH	HMBH	BZ	JYW	WFCS	LJJF	SGCS	LJJJSS	CLYT	YTSX	DZYX	XSZBH	SJHM	JYHGBZBH	CYRY	DPHGZBH	SQDM	YXH	TYBIP	AZDM	ZDRUSJ	BJRUSJ	CSIP	CSBS	RMJYW

                    string fields = "XH as 机动车序号,HPZL as 号牌种类,	HPHM as 号牌号码,	CLPP1 as 中文品牌,	CLXH as 车辆型号,	CLPP2 as 英文品牌,	GCJK as 过程_进口,	ZZG as 制造国,	ZZCMC as 制造厂名称 ,	CLSBDH as 车辆识别代号 ,	FDJH as 发动机号," +
                    "CLLX as 车辆类型,	CSYS as  车身颜色,	SYXZ as 使用性质,	SFZMHM as 身份证明号码,	SFZMMC as 身份证明种类名称,	SYR as 机动车所有人,	ZSXZQH as 住所行政区划,	ZSXXDZ as 登记住所详细地址,	YZBM1 as 住所邮政编码,	LXDH as 联系电话,	ZZZ as 居住暂住证明 ";
                    oraconn.Open();
                    OracleCommand cmd = new OracleCommand("Select "+ fields +" from gis_vehicle where HPHM='" + this.dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString() + "'", oraconn);
                    OracleDataAdapter Adp = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    Adp.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        if (dt.Rows.Count >= 2)
                        {
                            frmCarNum frmnum = new frmCarNum(this.mapControl,this.StrCon,this.getFromNamePath,this.layerName,this.UserName);
                            frmnum.setfrmInfo(dt);
                            frmnum.Visible = false;
                            frmnum.ShowDialog();
                            return;
                        }
                        frmCarInfo frmcar = new frmCarInfo();
                        frmcar.setInfo(dt.Rows[0], this.StrCon, this.UserName);
                        frmcar.mapControl = this.mapControl;
                        frmcar.mysql = this.mysql;
                        frmcar.layerName = this.layerName;
                        frmcar.getFromNamePath = this.getFromNamePath;
                        frmcar.TopMost = true;
                        frmcar.Visible = false;
                        frmcar.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("外地车辆暂未登记！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                   
                }
                catch(Exception ex)
                {
                    writeToLog(ex, "clKaKou-frmInfo-dataGridView1_CellContentClick-查看车辆");
                }
                finally
                {
                    if (oraconn.State == ConnectionState.Open)
                        oraconn.Close();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

       /// <summary>
       /// 处理情况
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {          
            try
            {
                string exedtl = string.Empty;

                frmExcut frmexec = new frmExcut();
                frmexec.ShowDialog(this);
                if (frmexec.DialogResult == DialogResult.OK)
                {
                    if (frmexec.ExDetail.Length > 50)
                    {
                        MessageBox.Show("处理情况概述超过50字符,系统自动取前50个字符作为概述信息进行保存", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        exedtl = frmexec.ExDetail.Substring(0, 50);
                    }
                    else
                    {
                        exedtl = frmexec.ExDetail;
                    }
                }
                else
                {
                    MessageBox.Show("没有写入处理情况概述,系统自动取空值作为概述信息进行保存", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    exedtl = frmexec.ExDetail;
                }
               
                string sqlstr = "update V_ALARM set 处理状态 = '已处理',处理人='" + this.UserName + "',处理时间= to_date('" + System.DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),处理情况='" + exedtl + "' where 报警编号 ='" + this.dataGridView1.Rows[0].Cells[1].Value.ToString() + "'";
                RunCommand(sqlstr);
                ////调用dll
                if (UseDll == true)
                {  //xxxxxx 3
                    Boolean exc = gdwcom.AlarmWite(this.dataGridView1.Rows[0].Cells[1].Value.ToString(), this.UserName, System.DateTime.Now); 

                    if (exc == false)
                    {
                       // MessageBox.Show("向治安卡口服务器中写入处理数据时发生错误。", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                WriteEditLog(sqlstr, "处理报警", "V_ALARM");
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmInfo-05-处理情况");
            }           
        }

        /// <summary>
        /// 查询SQL
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            DataTable dt = CLC.DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sql">SQL语句</param>
        private void RunCommand(string sql)
        {
            CLC.DatabaseRelated.OracleDriver.CreateConstring(StrCon[0], StrCon[1], StrCon[2]);
            CLC.DatabaseRelated.OracleDriver.OracleComRun(sql);
        }
       
        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="sFunc"></param>
        private void writeToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, sFunc);
        }

        //记录操作记录
        private void WriteEditLog(string sql, string method, string tablename)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + UserName + "',to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'),'治安卡口','" + tablename + ":" + sql.Replace('\'', '"') + "','" + method + "')";
                RunCommand(strExe);
            }
            catch (Exception ex)
            {
                writeToLog(ex, "clKaKou-frmInfo-06-记录操作记录");
            }
        }
    }
}

