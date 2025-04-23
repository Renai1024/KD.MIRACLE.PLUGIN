using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Metadata.EntityElement;
using System.Data;

using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;

namespace KD.MIRACLE.PLUGIN
{
    [Description("动态表单赋值")]
    [Kingdee.BOS.Util.HotUpdate]

    public class DyListtBill : AbstractDynamicFormPlugIn
    {   //接收传回来的数据
        string entityName = "F_SCS_Entity";
        DataTable dt;
        public override void OnLoad(EventArgs e)
        {//OnLoad加载事件
            base.OnLoad(e);

            //接收传回来的数据
            var fidList = this.View.OpenParameter.GetCustomParameter("FIDList") as List<string>;
            var fentryIdList = this.View.OpenParameter.GetCustomParameter("FEntryIDList") as List<string>;

            if (fidList == null || fentryIdList == null || fidList.Count == 0)
            {
                this.View.ShowMessage("未接收到有效参数");
                return;
            }

            try
            {
                // 2. 构建参数化SQL
                string selectSql = @"SELECT 
                            S1.FBILLNO AS F_BILLNO,
                            S2.FSEQ AS F_LINENUM,
                            M1.FNUMBER AS F_MATERIALNUM,
                            S2.F_SHENGGUANHUIFUJIAOQI1 AS F_Date1,
                            S2.F_SHENGGUANHUIFUJIAOQI2 AS F_Date2,
                            S2.F_SHENGGUANHUIFUJIAOQI3 AS F_Date3,
                            S2.F_SHENGGUANHUIFUYUANYOU1 AS F_REASON1,
                            S2.F_SHENGGUANHUIFUYUANYOU2 AS F_REASON2,
                            S2.F_SHENGGUANHUIFUYUANYOU3 AS F_REASON3,
                            S1.FID AS F_FID,
                            S2.FEntryID AS F_ENTRYID
                            FROM T_SAL_ORDER S1
                            INNER JOIN T_SAL_ORDERENTRY S2 ON S1.FID = S2.FID
                            INNER JOIN T_BD_MATERIAL M1 ON M1.FMATERIALID = S2.FMATERIALID
                            WHERE
                            S1.FID IN ({0}) 
                            AND S2.FEntryID IN ({1})
                    ";

                // 3. 参数处理（防止SQL注入）
                string fidParams = string.Join(",", fidList);
                string entryParams = string.Join(",", fentryIdList);

                // 4. 执行SQL查询
                dt = DBUtils.ExecuteDataSet(this.Context, string.Format(selectSql, fidParams, entryParams)).Tables[0];

                if (dt.Rows.Count > 0)
                {
                    //this.View.ShowMessage(dt.Rows.Count.ToString());
                    // 5. 转换查询结果
                    Entity entity = this.View.BillBusinessInfo.GetEntity(entityName);
                    DynamicObjectCollection entryData = this.Model.GetEntityDataObject(entity);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DynamicObject row = new DynamicObject(entity.DynamicObjectType);
                        entity.SeqDynamicProperty.SetValue(row, i + 1);
                        // 字段映射
                        row["F_BILLNO"] = dt.Rows[i]["F_BILLNO"].ToString();
                        row["F_LINENUM"] = dt.Rows[i]["F_LINENUM"].ToString();
                        row["F_MATERIALNUM"] = dt.Rows[i]["F_MATERIALNUM"].ToString();
                        row["F_Date1"] = convert(dt.Rows[i]["F_Date1"].ToString());
                        row["F_Date2"] = convert(dt.Rows[i]["F_Date2"].ToString());
                        row["F_Date3"] = convert(dt.Rows[i]["F_Date3"].ToString());
                        row["F_REASON1"] = dt.Rows[i]["F_REASON1"]?.ToString();
                        row["F_REASON2"] = dt.Rows[i]["F_REASON2"]?.ToString();
                        row["F_REASON3"] = dt.Rows[i]["F_REASON3"]?.ToString();
                        row["F_FID"] = dt.Rows[i]["F_FID"]?.ToString();
                        row["F_ENTRYID"] = dt.Rows[i]["F_ENTRYID"]?.ToString();
                        entryData.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                this.View.ShowMessage($"数据加载失败：{ex.Message}");
            }
        }
        public object convert(string str)
        {
            if (str != "") return Convert.ToDateTime(str);
            else return null;
        }
    }
}
