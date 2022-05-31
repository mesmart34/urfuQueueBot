using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Enums.Filters;
using System.Threading;
using Bots.UpdateHandlers;
using TableParser;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.IO;

namespace Bots.VK
{
    public class VKBot : IBot
    {
        private VkApi _api;
        private bool _isWorking = false;

        public Data Data { get; }

        public IUpdateHandler UpdateHandler { get; }

        public VKBot(Token token, IUpdateHandler updateHandler, Data data)
        {
            _api = new VkApi();
            _api.Authorize(new ApiAuthParams
            {
                AccessToken = token.GetToken()
            });

            UpdateHandler = updateHandler;
            Data = data;
        }

        public struct Message
        {
            public string text;
            public string keyname;
            public long? userId;
        }

        public async void Receive()
        {
            if (!TryGetMessage(out InputMessage im))
                return;

            await (UpdateHandler as Bots.UpdateHandlers.VK.VKUpdateHandler).HandleUpdateAsync(im);

            //VkNet.Model.Keyboard.KeyboardBuilder kb = new VkNet.Model.Keyboard.KeyboardBuilder();

            //switch(im.Text.ToLower())
            //{
            //    case "привет":
            //        //kb.AddButton("Menu", "menu_btn", VkNet.Enums.SafetyEnums.KeyboardButtonColor.Positive);
            //        //kb.AddLine();
            //        //kb.AddButton("Back", "back_btn", VkNet.Enums.SafetyEnums.KeyboardButtonColor.Negative);
            //        Keyboard k = new Keyboard("Menu", "Back");
            //        SendMessageAsync(im.SenderId, "Choose required button", k);
            //        break;
            //}
        }

        private bool TryGetMessage(out InputMessage im)
        {
            var m = _api.Messages.GetConversations(new GetConversationsParams
            {
                Count = 1,
                Filter = VkNet.Enums.SafetyEnums.GetConversationFilter.Unread
            });

            if (m.Items.Count == 0)
            {
                im.Text = "";
                im.SenderId = -1;
                im.Time = DateTime.MinValue;
                im.Username = "";
                return false;
            }

            VkNet.Model.Message message = m.Items[0].LastMessage;

            im.Text = (message.Text != "" && message.Text != null) ? message.Text : "";
            // TODO : Maybe add payload-field to IM
            //msg.keyname = (message.Payload != null && message.Payload != "") ? message.Payload : "";
            im.SenderId = message.FromId.Value;
            im.Time = message.Date.Value;
            // TODO : Find out the name
            im.Username = "";

            _api.Messages.MarkAsRead(im.SenderId.ToString());

            return true;
        }

        // =================================================================================
        // =================================================================================
        // =================================================================================
        // =================================================================================

        public Bots.Keyboard GetKeyboard(params string[] buttonsNames)
        {
            return new Keyboard(buttonsNames);
        }

        public void AddResponse(Response.FilterFunc filter, Response.ResponseFunc response)
        {
            UpdateHandler.AddResponse(new Response(filter, response));
        }

        public void StartReceiving()
        {
            _isWorking = true;

            Task.Run(() =>
            {
                while (_isWorking)
                {
                    Receive();
                    Thread.Sleep(50);
                }
            });
        }

        public void StopReceiving()
        {
            _isWorking = false;
        }

        public async void SendMessageAsync(long chatId, string text, Bots.Keyboard keyboard = null)
        {
            if (keyboard is not Keyboard && keyboard != null)
                throw new Exception("Not-VK keyboard using for VK bot");

            if (text != null)
            await _api.Messages.SendAsync(new MessagesSendParams
            {
                Message = text,
                UserId = chatId,
                RandomId = new Random().Next(),
                Keyboard = (keyboard as Keyboard)?.GetKeyboard()// ?? Keyboard.RemoveKeyboard
            });
        }

        public void InvokeMessage(long userId, string message)
        {
            UpdateHandler.InvokeMessage(this, userId, message);
        }

        public async void SendPhotoAsync(long userId, string path, string caption, Bots.Keyboard keyboard = null)
        {
            var uploadServer = _api.Photo.GetMessagesUploadServer(213475502);
            var response = await UploadFile(uploadServer.UploadUrl, path, "png");

            var attachment = _api.Photo.SaveMessagesPhoto(response);

            await _api.Messages.SendAsync(new MessagesSendParams
            {
                UserId = userId,
                RandomId = new Random().Next(),
                Message = caption,
                Keyboard = (keyboard as Keyboard)?.GetKeyboard(),// ?? Keyboard.RemoveKeyboard,
                Attachments = attachment
            });
        }

        public async void SendDocumentAsync(long userId, string path, string caption, Bots.Keyboard keyboard = null)
        {
            var uploadServer = _api.Photo.GetMessagesUploadServer(213475502);
            var response = await UploadFile(uploadServer.UploadUrl, path, path.Split('.').Last());

            var attachment = new List<VkNet.Model.Attachments.MediaAttachment>
            {
                _api.Docs.Save(response, caption ?? Guid.NewGuid().ToString(), null)[0].Instance
            };

            await _api.Messages.SendAsync(new MessagesSendParams
            {
                UserId = userId,
                Attachments = attachment,
                RandomId = new Random().Next()
            });
        }

        public void SendStickerAsync(long userId, string set, uint index, Bots.Keyboard keyboard = null)
        {
            SendPhotoAsync(userId, $"../../../../src/stickers/{set}/{index}.png", "", keyboard);
        }

        private async Task<string> UploadFile(string serverUrl, string file, string fileExtension)
        {
            // Получение массива байтов из файла
            var data = GetBytes(file);

            // Создание запроса на загрузку файла на сервер
            using (var client = new HttpClient())
            {
                var requestContent = new MultipartFormDataContent();
                var content = new ByteArrayContent(data);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                requestContent.Add(content, "file", $"file.{fileExtension}");

                var response = client.PostAsync(serverUrl, requestContent).Result;
                return Encoding.Default.GetString(await response.Content.ReadAsByteArrayAsync());
            }
        }

        private byte[] GetBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }
    }
}
