using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Orm.DataEntity;

namespace KD.MIRACLE.PLUGIN
{
    [Description("获取动态表单当前数据并更新")]
    [Kingdee.BOS.Util.HotUpdate]
    public class GetDyUpdateData : AbstractDynamicFormPlugIn
    {
        public string entity = "F_SCS_Entity";
        int count = 0;
        int index = 1;

        public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterBarItemClick(e);
            if (e.BarItemKey.Equals("SCS_MODIFY_DY_DATA"))
            {
                //获取单据体行数
                //int lineCount = this.View.Model.GetEntryRowCount(entity);
                DynamicObjectCollection collection = this.View.Model.DataObject[entity] as DynamicObjectCollection;


                //判断是否获取到行信息
                if (collection.Count == 0)
                {
                    this.View.ShowMessage("未获取到动态表单数据！");
                    return;
                }
                try
                {
                    foreach (DynamicObject row in collection)
                    {
                        string FID = row["F_FID"].ToString();
                        string FENTRYID = row["F_ENTRYID"].ToString();
                        string FDATE1 = Convert.ToString(row["F_Date1"]);
                        string FDATE2 = Convert.ToString(row["F_Date2"]);
                        string FDATE3 = Convert.ToString(row["F_Date3"]);
                        string FREASON1 = row["F_REASON1"]?.ToString();
                        string FREASON2 = row["F_REASON2"]?.ToString();
                        string FREASON3 = row["F_REASON3"]?.ToString();


                        Dictionary<string, string> updateDate = new Dictionary<string, string>();
                        updateDate.Add("F_Date1", FDATE1);
                        updateDate.Add("F_Date2", FDATE2);
                        updateDate.Add("F_Date3", FDATE3);

                        string updateSql = @"/*dialect*/
                            UPDATE S2 SET 
                            S2.{2} = '{3}',
                            S2.F_SHENGGUANHUIFUYUANYOU1 = '{4}',
                            S2.F_SHENGGUANHUIFUYUANYOU2 = '{5}',
                            S2.F_SHENGGUANHUIFUYUANYOU3 = '{6}',
                            S2.F_KEFUJIAOQI = '{3}'
                            FROM T_SAL_ORDER S1
                            INNER JOIN T_SAL_ORDERENTRY S2 ON S1.FID = S2.FID
                            WHERE S1.FID = '{0}' AND S2.FENTRYID = '{1}' 
                        ";

                        string updateSql2 = @"/*dialect*/
                            UPDATE S3 SET 
                            S3.FDELIVERYDATE = '{2}',
                            S3.FPLANDELIVERYDATE = '{2}'
                            FROM T_SAL_ORDER S1
                            INNER JOIN T_SAL_ORDERENTRY S2 ON S1.FID = S2.FID
                            INNER JOIN T_SAL_ORDERENTRYDELIPLAN S3 ON S2.FENTRYID = S3.FENTRYID
                            WHERE S1.FID = '{0}' AND S2.FENTRYID = '{1}' 
                        ";

                        string updateSql3 = @"/*dialect*/
                            UPDATE S3 SET 
							S3.FDELIVERYDATE = '{2}'                            					
                            FROM T_SAL_ORDER S1
                            INNER JOIN T_SAL_ORDERENTRY S2 ON S1.FID = S2.FID
							INNER JOIN T_SAL_ORDERENTRY_D S3 on S2.FENTRYID = S3.FENTRYID
                            WHERE S1.FID = '{0}' AND S2.FENTRYID = '{1}' 
                        ";
                        //若获取到行日期为空
                        if (judge(updateDate).ContainsKey("ERROR"))
                        {
                            this.View.ShowErrMessage("第" + index + "行" + judge(updateDate).Values.First().ToString());
                            return;
                        }
                        var result = DBUtils.Execute(this.Context,
                            string.Format(updateSql,
                            FID, FENTRYID,
                            judge(updateDate).Keys.First().ToString(), judge(updateDate).Values.First().ToString(),
                            FREASON1, FREASON2, FREASON3));

                        var result2 = DBUtils.Execute(this.Context,
                            string.Format(updateSql2,
                            FID, FENTRYID,
                            judge(updateDate).Values.First().ToString()));

                        var result3 = DBUtils.Execute(this.Context,
                            string.Format(updateSql3,
                            FID, FENTRYID,
                            judge(updateDate).Values.First().ToString()));
                        if (result == 0 || result2 == 0 || result3 == 0)
                        {
                            this.View.ShowErrMessage("第" + index + "行修改失败！");
                            return;
                        }
                        index++;
                        count++;
                    }
                    if (count == collection.Count)
                    {
                        this.View.ShowMessage("修改成功，总计" + count + "行数据成功保存");
                    }
                }
                catch (Exception ex)
                {
                    this.View.ShowErrMessage($"出错了：{ex.Message}");
                    return;
                }
            }
        }


        public Dictionary<string, string> judge(Dictionary<string, string> dic)
        {
            Dictionary<string, string> tmp = new Dictionary<string, string>();
            if (dic["F_Date3"] != null && dic["F_Date3"] != "")
            {
                tmp.Add("F_SHENGGUANHUIFUJIAOQI3", dic["F_Date3"]);
            }
            else if (dic["F_Date2"] != null && dic["F_Date2"] != "")
            {
                tmp.Add("F_SHENGGUANHUIFUJIAOQI2", dic["F_Date2"]);
            }
            else if (dic["F_Date1"] != null && dic["F_Date1"] != "")
            {
                tmp.Add("F_SHENGGUANHUIFUJIAOQI1", dic["F_Date1"]);
            }
            else tmp.Add("ERROR", "回复交期均为空，请填写数据后提交！");
            return tmp;
        }


    }
}
