using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;

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
                            switch (LineEvent.message.text.Replace("!", ""))
                            {
                                //換行符號 %0D%0A %0D%0A 
                                case "本月遊戲清單":

                                    string MyReply = "本月遊戲清單:  \n PS4 - 拉比哩比 (2018-05-08) ";
                                    MyReply += " \n PS4 - H1Z1 屍流感 (2018-05-22) ";
                                    MyReply += " \n PS4 - 女神異聞錄 5 星夜熱舞  (2018-05-25) ";

                                    this.ReplyMessage(LineEvent.replyToken, MyReply);

                                    break;
                                case "一哥廢物":
                                    this.ReplyMessage(LineEvent.replyToken, "說得好R!");

                                    break;

                                default:
                                    this.ReplyMessage(LineEvent.replyToken, "安安你說了:" + LineEvent.message.text + "，窩抗不懂^_^");
                                    break;
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
    }
}
