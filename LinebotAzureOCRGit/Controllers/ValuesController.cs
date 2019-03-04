using isRock.LineBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace LinebotAzureOCRGit.Controllers
{
    public class ValuesController : LineWebHookControllerBase
    {
        const string channelAccessToken = "!!!!! 改成自己的ChannelAccessToken !!!!!";
        const string AdminUserId = "!!!改成你的AdminUserId!!!";

        [Route("api/AzureOCR")]
        [HttpPost]
        public async Task<IHttpActionResult> OCR()
        {

            string replyToken = "";
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
                    replyToken = LineEvent.replyToken;
                    if (LineEvent.message.type == "text") //收到文字
                        this.ReplyMessage(LineEvent.replyToken, "廢話不多說，快傳圖來辨識看看。");
                    if (LineEvent.message.type == "sticker") //收到貼圖
                        this.ReplyMessage(LineEvent.replyToken, "愛您唷 <3");
                    if (LineEvent.message.type.Trim().ToLower() == "image") //收到圖片
                    {
                        var ocrService = new AzureOCRService();
                        //取得contentid
                        var LineContentID = ReceivedMessage.events.FirstOrDefault().message.id.ToString();
                        var filebody = isRock.LineBot.Utility.GetUserUploadedContent(LineContentID, ChannelAccessToken);
                        //建立唯一名稱
                        var filename = "/tempFolder/" + Guid.NewGuid() + ".png";
                        var path = System.Web.HttpContext.Current.Request.MapPath(filename);
                        //save
                        System.IO.File.WriteAllBytes(path, filebody);
                        //回覆訊息
                        var str = await ocrService.getOCR($"https://{System.Web.HttpContext.Current.Request.Url.Host}{filename}");
                        str = ocrService.GetOCRResponseString(str);
                        this.ReplyMessage(LineEvent.replyToken, $"{str}");

                        System.IO.File.Delete(path);


                    }
                }
                //response OK
                return Ok();
            }
            catch (Exception ex)
            {
                //如果發生錯誤，傳訊息給Admin
                this.PushMessage(AdminUserId, "發生錯誤:\n" + ex.Message);

                this.ReplyMessage(replyToken, "發生錯誤:\n" + ex.Message);

                //response OK
                return Ok();
            }
        }
    }
}
