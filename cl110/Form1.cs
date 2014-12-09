using System;
using System.Data;
using System.Windows.Forms;
using CLC;

namespace cl110
{
    public partial class frmBus : Form
    {
        public string[] ConStr;
        public string UserName = string.Empty;

        private string fields = string.Empty;
        private string fieldx = string.Empty;
        private string fieldy = string.Empty;
        private string tablename = string.Empty;
        public double x;
        public double y;

        private string GetFromNamePath = string.Empty;
        public frmBus()
        {
            InitializeComponent();
        }

        private void frmBus_Load(object sender, EventArgs e)
        {
            try
            {
                cmbTableName.Items.Clear();
                cmbTableName.Items.Add("案件信息");
                cmbTableName.Items.Add("人口系统");
                cmbTableName.Items.Add("出租屋房屋系统");
                cmbTableName.Items.Add("治安卡口系统");
                cmbTableName.Items.Add("基层派出所");
                cmbTableName.Items.Add("基层民警中队");
                cmbTableName.Items.Add("公共场所");
                cmbTableName.Items.Add("安全防护单位");
                cmbTableName.Items.Add("视频位置");
                cmbTableName.Items.Add("特种行业");
                cmbTableName.Items.Add("网吧");
                cmbTableName.Items.Add("消防栓");
                cmbTableName.Items.Add("消防重点单位");


                cmbTableName.Text = cmbTableName.Items[0].ToString();

                GetFromNamePath = Application.StartupPath + "\\GetFromNameConfig.ini";
            }
            catch (Exception ex)
            {
                ExToLog(ex, "01-初始化窗体");
            }
        }

        private void cmbTableName_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                cmdTableFields.Items.Clear();
                cmdTableFields.Items.Add("请选择字段");
                switch (cmbTableName.Text.Trim())
                {
                    case "案件信息":
                        cmdTableFields.Items.Add("案件编号");
                        cmdTableFields.Items.Add("案件名称");
                        cmdTableFields.Items.Add("案件类型");
                        cmdTableFields.Items.Add("案发状态");
                        cmdTableFields.Items.Add("案别_案由");
                        cmdTableFields.Items.Add("所属派出所");
                        break;
                    case "人口系统":
                        cmdTableFields.Items.Add("姓名");
                        cmdTableFields.Items.Add("身份证号码");
                        cmdTableFields.Items.Add("民族");
                        cmdTableFields.Items.Add("全住址");
                        cmdTableFields.Items.Add("人口性质");
                        cmdTableFields.Items.Add("所属派出所");
                        break;
                    case "出租屋房屋系统":
                        cmdTableFields.Items.Add("房屋编号");
                        cmdTableFields.Items.Add("屋主姓名");
                        cmdTableFields.Items.Add("产权证号");
                        cmdTableFields.Items.Add("全住址");
                        cmdTableFields.Items.Add("身份证号码");
                        cmdTableFields.Items.Add("所属派出所");
                        break;
                    case "治安卡口系统":
                        cmdTableFields.Items.Add("卡口名称");
                        cmdTableFields.Items.Add("卡口编号");
                        cmdTableFields.Items.Add("安装地点");
                        cmdTableFields.Items.Add("监控点接壤地区");
                        cmdTableFields.Items.Add("卡口对应车道数");
                        cmdTableFields.Items.Add("所属派出所");
                        cmdTableFields.Items.Add("监控方向");
                        cmdTableFields.Items.Add("联系人");
                        cmdTableFields.Items.Add("联系方式");
                        break;
                    case "基层派出所":
                        cmdTableFields.Items.Add("派出所名");
                        cmdTableFields.Items.Add("派出所代码");
                        cmdTableFields.Items.Add("详址");
                        cmdTableFields.Items.Add("派出所类别");
                        cmdTableFields.Items.Add("所长姓名");
                        cmdTableFields.Items.Add("所长电话");
                        cmdTableFields.Items.Add("教指导员姓名");
                        break;
                    case "基层民警中队":
                        cmdTableFields.Items.Add("中队名");
                        cmdTableFields.Items.Add("中队代码");
                        cmdTableFields.Items.Add("详址");
                        cmdTableFields.Items.Add("中队类别");
                        cmdTableFields.Items.Add("队长姓名");
                        cmdTableFields.Items.Add("指导员姓名");
                        break;
                    case "公共场所":
                        cmdTableFields.Items.Add("编号");
                        cmdTableFields.Items.Add("企业注册名称");
                        cmdTableFields.Items.Add("企业实际经营名称");
                        cmdTableFields.Items.Add("当前等级");
                        cmdTableFields.Items.Add("行业类别");
                        cmdTableFields.Items.Add("详细地址");
                        cmdTableFields.Items.Add("法定代表人");
                        cmdTableFields.Items.Add("实际经营人姓名");
                        break;
                    case "安全防护单位":
                        cmdTableFields.Items.Add("编号");
                        cmdTableFields.Items.Add("单位名称");
                        cmdTableFields.Items.Add("单位编号");
                        cmdTableFields.Items.Add("单位性质");
                        cmdTableFields.Items.Add("单位地址");
                        break;
                    case "视频位置":
                        cmdTableFields.Items.Add("设备名称");
                        cmdTableFields.Items.Add("设备编号");
                        cmdTableFields.Items.Add("所属派出所");
                        cmdTableFields.Items.Add("日常管理人");
                        cmdTableFields.Items.Add("设备ID");
                        break;
                    case "特种行业":
                        cmdTableFields.Items.Add("编号");
                        cmdTableFields.Items.Add("企业注册名称");
                        cmdTableFields.Items.Add("企业实际经营名称");
                        cmdTableFields.Items.Add("当前等级");
                        cmdTableFields.Items.Add("行业类别");
                        cmdTableFields.Items.Add("详细地址");
                        cmdTableFields.Items.Add("法定代表人");
                        cmdTableFields.Items.Add("实际经营人姓名");
                        break;
                    case "网吧":
                        cmdTableFields.Items.Add("编号");
                        cmdTableFields.Items.Add("网吧名称");
                        cmdTableFields.Items.Add("网吧编号");
                        cmdTableFields.Items.Add("地址");
                        cmdTableFields.Items.Add("法人代表");
                        cmdTableFields.Items.Add("负责人");
                        break;
                    case "消防栓":
                        cmdTableFields.Items.Add("编号");
                        cmdTableFields.Items.Add("消防内部编号");
                        cmdTableFields.Items.Add("所属派出所");
                        cmdTableFields.Items.Add("位置描述");
                        cmdTableFields.Items.Add("消防管理单位");
                        break;
                    case "消防重点单位":
                        cmdTableFields.Items.Add("编号");
                        cmdTableFields.Items.Add("消防安全重点单位");
                        cmdTableFields.Items.Add("地址");
                        cmdTableFields.Items.Add("所属派出所");
                        cmdTableFields.Items.Add("消防安全责任人");
                        cmdTableFields.Items.Add("消防安全管理人");
                        cmdTableFields.Items.Add("联系电话");
                        cmdTableFields.Items.Add("单位类别");
                        break;
                    default:
                        break;
                }
                cmdTableFields.Text = cmdTableFields.Items[0].ToString();
            }
            catch (Exception ex)
            {
                ExToLog(ex, "02-切换选项");
            }
        }


        private void btnSearch_Click(object sender, EventArgs e)
        {
            Search();
        }

        /// <summary>
        /// 查询
        /// </summary>
        private void Search()
        {
            try
            {
                switch (cmbTableName.Text.Trim())
                {
                    case "案件信息":
                        fields = "案件编号,案件名称,案发状态,案件类型,案别_案由,专案标识,发案地点_区县,发案地点_街道,发案地点详址,所属社区,简要案情,作案手段特点,发案场所,案件来源,所属派出所,所属中队,所属警务室 ";
                        tablename = cmbTableName.Text.Trim();
                        fieldx = "X";
                        fieldy = "Y";
                        break;
                    case "人口系统":
                        fields = "身份证号码,姓名,性别,民族,出生日期,人口性质,全住址,住址街路巷,户籍地址,婚姻状态,结婚时间,配偶姓名,联系电话,所属派出所,所属中队,所属警务室 ";
                        tablename = cmbTableName.Text.Trim();
                        fieldx = "X";
                        fieldy = "Y";
                        break;
                    case "出租屋房屋系统":
                        fields = "房屋编号,屋主姓名,屋主联系电话,房屋类型,全住址,地址街路巷,产权证类别,产权证号,当前居住人数,暂住证有效期内人数,所属派出所,所属中队,所属警务室 ";
                        tablename = cmbTableName.Text.Trim();
                        fieldx = "X";
                        fieldy = "Y";
                        break;
                    case "治安卡口系统":
                        fields = "编号,卡口名称,卡口编号,安装地点,监控点接壤地区,卡口对应车道数,所属派出所,监控方向,联系人,联系方式 ";
                        tablename = cmbTableName.Text.Trim();
                        fieldx = "X";
                        fieldy = "Y";
                        break;
                    case "基层派出所":
                        fields =
                            " 派出所名,派出所代码,详址,派出所类别,值班电话,现有民警数,辖区面积,常住人口总户数,实有人口,常住人口数,暂住人口数,已建警务室数,所长姓名,所长电话,教指导员姓名,已建中队数量 ";
                        tablename = cmbTableName.Text.Trim();
                        fieldx = "X";
                        fieldy = "Y";
                        break;
                    case "基层民警中队":
                        fields =
                            " 中队名,中队代码,详址,中队类别,值班电话,现有民警数,辖区面积,常住人口总户数,实有人口,常住人口数,暂住人口数,已建警务室数,所属派出所代码,队长姓名,队长电话,指导员姓名 ";
                        tablename = cmbTableName.Text.Trim();
                        fieldx = "X";
                        fieldy = "Y";
                        break;
                    case "公共场所":
                        fields =
                            "编号,企业注册名称,企业实际经营名称,联系电话,行业类别,详细地址,法定代表人,实际经营人姓名,所属社区民警中队,所属警务室,责任民警姓名,民警联系电话,主营,兼营 ";
                        tablename = cmbTableName.Text.Trim();
                        fieldx = "X";
                        fieldy = "Y";
                        break;
                    case "安全防护单位":
                        fields = "编号,单位名称,单位性质,单位地址,主管保卫工作负责人,手机号码 ";
                        tablename = cmbTableName.Text.Trim();
                        fieldx = "X";
                        fieldy = "Y";
                        break;
                    case "视频位置":
                        fields = "设备名称,设备编号,所属派出所,日常管理人,设备ID ";
                        tablename = cmbTableName.Text.Trim();
                        fieldx = "X";
                        fieldy = "Y";
                        break;
                    case "特种行业":
                        fields =
                            "编号,企业注册名称,企业实际经营名称,当前等级,联系电话,行业类别,详细地址,法定代表人,实际经营人姓名,保安组织联系电话,保安组织负责人姓名,保安组织负责人联系电话,责任民警姓名,民警联系电话,主营,兼营 ";
                        tablename = cmbTableName.Text.Trim();
                        fieldx = "X";
                        fieldy = "Y";
                        break;
                    case "网吧":
                        fields = "编号,网吧名称,地址,联系电话,所属派出所,电脑数目,法人代表,负责人 ";
                        tablename = cmbTableName.Text.Trim();
                        fieldx = "X";
                        fieldy = "Y";
                        break;
                    case "消防栓":
                        fields = "编号,消防内部编号,所属派出所,位置描述,消防管理单位 ";
                        tablename = cmbTableName.Text.Trim();
                        fieldx = "X";
                        fieldy = "Y";
                        break;
                    case "消防重点单位":
                        fields = "编号,消防安全重点单位,地址,所属派出所,消防安全责任人,消防安全管理人,联系电话,单位类别 ";
                        tablename = cmbTableName.Text.Trim();
                        fieldx = "X";
                        fieldy = "Y";
                        break;
                    default:
                        break;
                }

                if (cmbTableName.Text.Trim() != "")
                {
                    string Sqlstr = string.Empty;

                    if (cmdTableFields.Text.Trim() != "请选择字段")
                    {
                        if (txtKey.Text.Trim() == "")
                        {
                            MessageBox.Show(@"关键字不能为空值");
                        }
                        if (txtKey.Text != "" && checkBox1.Checked)
                        {
                            Sqlstr = "Select " + fields + " from " + tablename + " where " + cmdTableFields.Text.Trim() +
                                     " like '%" + txtKey.Text.Trim() + "%'";
                        }
                        else if (txtKey.Text != "" && checkBox1.Checked == false)
                        {
                            Sqlstr = "Select " + fields + " from " + tablename + " where " + cmdTableFields.Text.Trim() +
                                     " = '" + txtKey.Text.Trim() + "'";
                        }
                    }
                    else if (cmdTableFields.Text.Trim() == "请选择字段")
                    {
                        Sqlstr = "Select " + fields + " from " + tablename;
                    }

                    DataTable dt = GetTable(Sqlstr);

                    dataGridView1.DataSource = dt;
                    dataGridView1.Refresh();

                    //Width = dataGridView1.Width;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "03-查询");
            }
        }

        /// <summary>
        /// 双击关联
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (
                    MessageBox.Show(@"是否进行数据关联?  注意，此操作将改变当前数据的坐标值", @"系统提示", MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    if (dataGridView1.CurrentRow != null)
                    {
                        string Sqlstr = string.Empty;
                        if(cmbTableName.Text.Trim()=="案件信息"||cmbTableName.Text.Trim()=="人口系统"||cmbTableName.Text.Trim()=="出租屋房屋系统")
                            Sqlstr = "update " + tablename + " set GEOLOC = MDSYS.SDO_GEOMETRY(2001, 8307, MDSYS.SDO_POINT_TYPE(" + x.ToString() + "," + y.ToString() + ", NULL),NULL, NULL) where "+ dataGridView1.Columns[0].HeaderText + " ='" +
                                      dataGridView1.CurrentRow.Cells[0].Value.ToString() + "'"; 
                        else
                        {
                            Sqlstr = "update " + tablename + " set x=" + x.ToString() + ",y=" + y.ToString() + " where " +
                                      dataGridView1.Columns[0].HeaderText + " ='" +
                                      dataGridView1.CurrentRow.Cells[0].Value.ToString() + "'";
                        }
                      
                        RunCommand(Sqlstr);
                    }

                    MessageBox.Show(@"数据关联成功", @"系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "04-双击关联");
            }
        }

        private void frmBus_FormClosing(object sender, FormClosingEventArgs e)
        {
            Visible = false;
            e.Cancel = true;
        }

        private void frmBus_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode.ToString())
                {
                    case "Return":
                        Search();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "04-frmBus_KeyDown");
            }
        }


        /// <summary>
        /// 查询SQL
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataTable</returns>
        private DataTable GetTable(string sql)
        {
            DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            DataTable dt = DatabaseRelated.OracleDriver.OracleComSelected(sql);
            return dt;
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sql">SQL语句</param>
        private void RunCommand(string sql)
        {
            DatabaseRelated.OracleDriver.CreateConstring(ConStr[0], ConStr[1], ConStr[2]);
            DatabaseRelated.OracleDriver.OracleComRun(sql);
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="sFunc"></param>
        private void ExToLog(Exception ex, string sFunc)
        {
            BugRelated.ExceptionWrite(ex, "cl110-Form1-" + sFunc);
        }

        private void RecToLog(string s)
        {
            BugRelated.LogWrite(s, Application.StartupPath + "\rec.log");
        }

        //记录操作记录
        private void WriteEditLog(string sql, string method)
        {
            try
            {
                string strExe = "insert into 操作记录 values('" + UserName + "',to_date('" + DateTime.Now.ToString() +
                                "','yyyy-mm-dd hh24:mi:ss'),'110接处警','GPS110.案件信息110:" + sql.Replace('\'', '"') + "','" +
                                method + "')";
                RunCommand(strExe);
            }
            catch(Exception ex)
            {
                ExToLog(ex, "WriteEditLog");
            }
        }
    }
}