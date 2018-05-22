using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;

using System.IO;
using Newtonsoft.Json;
using System.Web;
using System.Data;

namespace MyLineBot.Controllers
{
    public class LineBotWebHookController : isRock.LineBot.LineWebHookControllerBase
    {
        string channelAccessToken = System.Configuration.ConfigurationManager.AppSettings["LineChannelAccessToken"];
        string AdminUserId = System.Configuration.ConfigurationManager.AppSettings["LineAdminUserId"];

        [Route("api/LineWebHookSample")]
        [HttpPost]
        public IHttpActionResult POST()
        {
            try
            {
                //設定ChannelAccessToken(或抓取Web.Config)
                this.ChannelAccessToken = channelAccessToken;
                //取得Line Event(範例，只取第一個)
                var LineEvent = this.ReceivedMessage.events.FirstOrDefault();
                //配合Line verify 
                if (LineEvent.replyToken == "00000000000000000000000000000000") return Ok();
                //回覆訊息
                if (LineEvent.type == "message")
                {
                    //收到文字
                    if (LineEvent.message.type == "text")
                    {
                        if (LineEvent.message.text.Substring(0, 1) == "!")
                        {

                            try
                            {

                                if (LineEvent.message.text.Split('!').Length == 2)
                                {

                                    string sReplyString = OneCommand(LineEvent.message.text);
                                    this.ReplyMessage(LineEvent.replyToken, sReplyString);

                                }
                                else if(LineEvent.message.text.Split('!').Length == 3)
                                {

                                    string sReplyString = twoCommand(LineEvent.message.text);
                                    this.ReplyMessage(LineEvent.replyToken, sReplyString);

                                }
                                else
                                {

                                    this.ReplyMessage(LineEvent.replyToken, "指令不正確");
                                }


                                
                            }
                            catch (Exception ex)
                            {

                                this.ReplyMessage(LineEvent.replyToken, "執行指令發生錯誤：" + ex.Message);
                            }

                        }
                    }

                    //收到貼圖    
                    //if (LineEvent.message.type == "sticker")
                    //{
                    //    this.ReplyMessage(LineEvent.replyToken, 1, 2);
                    //}

                }
                //response OK
                return Ok();
            }
            catch (Exception ex)
            {
                //如果發生錯誤，傳訊息給Admin
                this.PushMessage(AdminUserId, "發生錯誤:\n" + ex.Message);
                //response OK
                return Ok();
            }
        }


        private string OneCommand(string Command)
        {
            string ResultString = "";
            switch (Command)
            {
                //換行符號 %0D%0A %0D%0A 
                case "!本月遊戲清單":
                    ResultString = getGameList("");
                    
                    break;
                case "!一哥廢物":

                    ResultString = "說得好^_^";
                    break;

                default:
                    ResultString = "安安你說了:" + Command + "，窩抗不懂^_^";
                    break;
            }

            return ResultString;
        }


        private string twoCommand(string Command)
        {

            string ResultString = "";
            //0是沒有資料的 1是指令 2是值
            string[] arrCommond = Command.Split('!');

            switch (arrCommond[1])
            {
                //換行符號 %0D%0A %0D%0A 
                case "本月遊戲清單":
                    int iMonth = 0;
                    if (int.TryParse(arrCommond[2],out iMonth))
                    {
                        if (iMonth >=1 && iMonth <= 12)
                        {
                            ResultString = getGameList(iMonth.ToString("00"));
                        }
                        else
                        {
                            ResultString = "指令格式輸入錯誤,月份只能輸入1~12月份";
                        }

                    }
                    else
                    {
                        ResultString = "指令格式輸入錯誤,指令為：\n !本月遊戲清單!NN \n NN為1~12月份" ;
                    }


                    break;


                default:
                    ResultString = "安安你說了:" + Command + "，窩抗不懂^_^";
                    break;
            }


            return ResultString;
        }


        private string getGameList(string AssignMonth)
        {
            if(AssignMonth == "")
            {
                AssignMonth = DateTime.Now.ToString("yyyy/MM");
            }
            else
            {
                AssignMonth = DateTime.Now.ToString("yyyy/") + AssignMonth;
            }


            string LastUpdateTime = "最後更新日期：2018/05/22 ";
            string sMyReply = "本月遊戲清單\n";

            RPaWorkLibrary.MSSQL RM = new RPaWorkLibrary.MSSQL();
            RPaWorkLibrary.Encryption RE = new RPaWorkLibrary.Encryption();
            DataTable dtGameList;

            try
            {
   
                RM.ServerName = RE.EnCodeString(System.Configuration.ConfigurationManager.AppSettings["DBServer"]);
                RM.DataBaseName = RE.EnCodeString(System.Configuration.ConfigurationManager.AppSettings["DBDataBase"]);
                RM.DB_Id = RE.EnCodeString(System.Configuration.ConfigurationManager.AppSettings["DBId"]);
                RM.DB_Psw = RE.EnCodeString(System.Configuration.ConfigurationManager.AppSettings["DBPws"]);

                try
                {
                    
                    DateTime LastDay = Convert.ToDateTime(AssignMonth + "/01").AddMonths(1).AddDays(-Convert.ToDateTime(AssignMonth + "/01").AddMonths(1).Day);

                    string WhereString = " AND dSaleDate >= '" + AssignMonth + "/01' ";
                    WhereString += " AND dSaleDate <= '" + AssignMonth + "/" + LastDay.ToString("dd") + "' ";
                    dtGameList = RM.Get_DataTable("SELECT * FROM GameList WHERE 1 = 1 " + WhereString);

                    if (dtGameList.Rows.Count == 0)
                    {
                        return "此月無發售遊戲(" + LastUpdateTime + ")";

                    }
                }
                catch (Exception ex)
                {

                    throw new Exception("【撈資料發生問題】" + ex.Message);
                }


                try
                {

                    foreach (DataRow dr in dtGameList.Rows)
                    {
                        string sLanguage = "";
                        string sSaleDate = dr["dSaleDate"].ToString();

                        if(dr["sLanguage"].ToString().Trim() != "中")
                        {
                            sLanguage = "(" + dr["sLanguage"].ToString().Trim() + ")";
                        }
                       
                        sMyReply += Convert.ToDateTime(sSaleDate).ToString("yyyy/MM/dd") +  " " + dr["sName"].ToString().TrimEnd()+ sLanguage + " \n";

                    }

                    return sMyReply + LastUpdateTime ;
                }
                catch (Exception ex)
                {

                    throw new Exception("【組合文字訊息發生錯誤】" + ex.Message);
                }


            }
            catch (Exception ex)
            {

                throw new Exception("本月遊戲清單執行錯誤 - " + ex.Message);
            }


            
        }




    }
}
