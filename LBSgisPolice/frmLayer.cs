using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MapInfo.Mapping;
using MapInfo.Windows.Controls;

namespace LBSgisPolice
{
    public partial class frmLayer : Form
    {
        private Map mainMap;
        public frmLayer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 读取地图中的图层
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="map">地图</param>
        internal void InitialLayers(Map map)
        {
            try
            {
                mainMap = map;
                for (int i = 0; i < map.Layers.Count; i++)//对地图的每一个图层进行循环
                {
                    IMapLayer layer = map.Layers[i];
                    TreeNode tNode = new TreeNode(layer.Name);
                    tNode.ImageKey = "(无)";
                    tNode.Checked = true;
                    tvLayer.Nodes.Add(tNode);

                    //图层集
                    if (layer.Type == LayerType.Group)
                    {
                        GroupLayer gLayer = (GroupLayer)layer;
                        for (int j = 0; j < gLayer.Count; j++)
                        {
                            IMapLayer layer2 = gLayer[j];
                            TreeNode tChildNode = new TreeNode(layer2.Name);
                            tChildNode.ImageKey = "(无)";
                            if (layer2.VisibleRangeEnabled)//判断一个图层是否具有可视区间
                            {
                                if (layer2.Enabled)
                                {
                                    if (layer2.VisibleRange.Within(map.Zoom))//判断一个图层是否在可视范围内，是则前面选择
                                    {
                                        tChildNode.Checked = true;
                                    }
                                    else
                                    {
                                        tChildNode.Checked = false;
                                        tChildNode.BackColor = Color.FromArgb(210, 210, 210);  //用灰色表示此层在现在的视野范围内不可见，后面会根据这个是此层不开操作
                                        tChildNode.ToolTipText = "此层在当前视野不可见，不能操作";
                                    }
                                }
                                else
                                {
                                    tChildNode.Checked = false;
                                }
                            }
                            else
                            {
                                if (layer2.IsVisible && layer2.Enabled)
                                {
                                    tChildNode.Checked = true;
                                }
                                else
                                {
                                    tChildNode.Checked = false;
                                }
                            }
                            tNode.Nodes.Add(tChildNode);
                        }
                    }

                    //是包含多项的label
                    else if (layer.Type == LayerType.Label)
                    {
                        LabelLayer lLayer = (LabelLayer)layer;
                        if (lLayer.Sources.Count > 1)
                        {
                            for (int j = 0; j < lLayer.Sources.Count; j++)
                            {
                                TreeNode tChildNode = new TreeNode(lLayer.Sources[j].Name);
                                tChildNode.ImageKey = "(无)";
                                if (lLayer.Sources[j].VisibleRangeEnabled)
                                {
                                    if (lLayer.Sources[j].Enabled)
                                    {
                                        if (lLayer.Sources[j].VisibleRange.Within(map.Zoom))
                                        {

                                            tChildNode.Checked = true;
                                        }
                                        else
                                        {
                                            tChildNode.Checked = false;
                                            tChildNode.BackColor = Color.FromArgb(210, 210, 210);  //用灰色表示此层在现在的视野范围内不可见，后面会根据这个是此层不开操作
                                            tChildNode.ToolTipText = "此层在当前视野不可见，不能操作";
                                        }
                                    }
                                    else
                                    {
                                        tChildNode.Checked = false;
                                    }
                                }
                                else
                                {
                                    if (lLayer.Enabled)
                                    {
                                        tChildNode.Checked = lLayer.Sources[j].Visible;
                                    }
                                }
                                tNode.Nodes.Add(tChildNode);
                            }
                        }
                    }
                    else if (layer.Type == LayerType.ObjectTheme)
                    {

                    }
                    else if (layer.Type == LayerType.Normal)
                    {
                        if (layer.VisibleRangeEnabled)
                        {
                            if (layer.Enabled)
                            {
                                if (layer.VisibleRange.Within(map.Zoom))
                                {
                                    tNode.Checked = true;
                                }
                                else
                                {
                                    tNode.Checked = false;
                                    tNode.BackColor = Color.FromArgb(210, 210, 210);  //用灰色表示此层在现在的视野范围内不可见，后面会根据这个是此层不开操作
                                    tNode.ToolTipText = "此层在当前视野不可见，不能操作";
                                }
                            }
                            else
                            {
                                tNode.Checked = false;
                            }
                        }
                        else
                        {
                            if (layer.IsVisible && layer.Enabled)
                            {
                                tNode.Checked = true;
                            }
                            else
                            {
                                tNode.Checked = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExToLog(ex, "显示图层");
            }
        }

        /// <summary>
        /// 关闭窗体按钮
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (this.tvLayer != null)
            {
                this.tvLayer.Nodes.Clear();
                this.tvLayer.Dispose();
            }
            this.Dispose();         
        }

        /// <summary>
        /// 确定按钮点击事件
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i <mainMap.Layers.Count; i++)
                {
                    IMapLayer layer =mainMap.Layers[i];
                    //for (int j = 0; j < this.tvLayer.Nodes.Count; j++)
                    //{

                        if (layer.Type== LayerType.Normal)
                        {
                            if (this.tvLayer.Nodes[i].BackColor != Color.FromArgb(210, 210, 210))
                            {
                                if (layer.Name == this.tvLayer.Nodes[i].Text)
                                {
                                    layer.Enabled = this.tvLayer.Nodes[i].Checked;
                                }
                            }
                        }

                        else if (layer.Type== LayerType.Label)
                        {
                            LabelLayer lLayer = (LabelLayer)layer;
                            for (int m = 0; m < lLayer.Sources.Count; m++)
                            {

                                for (int k = 0; k < this.tvLayer.Nodes[i].Nodes.Count; k++)
                                {
                                    if (this.tvLayer.Nodes[i].Nodes[k].BackColor != Color.FromArgb(210, 210, 210))
                                    {
                                        if (lLayer.Sources[m].Name == this.tvLayer.Nodes[i].Nodes[k].Text)
                                        {
                                            lLayer.Sources[m].Enabled = this.tvLayer.Nodes[i].Nodes[k].Checked;
                                        }
                                    }
                                   
                                }
                            }
                         }
                         else if (layer is ObjectThemeLayer)
                         {

                         }
                         else if (layer.Type== LayerType.Group)
                         {
                             GroupLayer gLayer = (GroupLayer)layer;
                             for (int m = 0; m < gLayer.Count; m++)
                             {
                                 IMapLayer iLayer = gLayer[m];
                                 for (int k = 0; k < this.tvLayer.Nodes[i].Nodes.Count; k++)
                                 {
                                     if (this.tvLayer.Nodes[i].Nodes[k].BackColor != Color.FromArgb(210, 210, 210))
                                     {
                                         if (iLayer.Alias == this.tvLayer.Nodes[i].Nodes[k].Text)
                                         {
                                             //if (iLayer.Type == LayerType.Raster)
                                             //{
                                                 
                                             //}
                                             //else
                                             //{
                                                 iLayer.Enabled = this.tvLayer.Nodes[i].Nodes[k].Checked;
                                             //}
                                         }
                                     }
                                 }
                             }
                         }
                    //}
                }
                this.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in buttonOK_Click in frmLayer\n"+ex.Message);
            }
         } 

        /// <summary>
        /// 当选中或取消树结点的复选框是发生
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        private void tvLayer_AfterCheck(object sender, TreeViewEventArgs e)
        {       
            try
            {
                if(e.Node.BackColor== Color.FromArgb(210, 210, 210))
                {
                    if (e.Node.Checked)
                    {
                        e.Node.Checked =false;
                    }
                }

                if (e.Node.Nodes != null)
                {
                    for (int i = 0; i < e.Node.Nodes.Count; i++)
                    {
                        if (e.Node.Nodes[i].BackColor == Color.FromArgb(210, 210, 210))
                        {
                        }
                        else
                        {
                            e.Node.Nodes[i].Checked = e.Node.Checked;
                        }
                    }
                }

                if (e.Node.Parent != null)
                {
                    if (e.Node.Checked)
                    {
                        if (e.Node.Parent.Checked == false)
                        {
                            e.Node.Parent.Checked = true;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in tvLayer_AfterSelect in frmLayer\n" + ex.Message);
                ExToLog(ex, "确定图层");
            }
        }

        /// <summary>
        /// 异常日志
        /// 最后更新人 李立
        /// 最后更新时间 2011-1-24
        /// </summary>
        /// <param name="ex">异常源</param>
        /// <param name="sFunc">函数名</param>
        private void ExToLog(Exception ex, string sFunc)
        {
            CLC.BugRelated.ExceptionWrite(ex, "Main-frmLayer-" + sFunc);
        }
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////        
    }
}