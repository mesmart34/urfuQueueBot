using System;
using TelegramBot;
using System.IO;
using System.Collections.Generic;
using Telegram.Bot.Types;
using FileManager;
using System.Threading.Tasks;
using TableParser;
using System.Linq;
using TableQueries;

// TODO: Сделать создание сценария использования более удобным и поддерживаемым

namespace QueueBot
{
    public class QueueBot : Bot
    {
        private const string src = "../../../../src/";

        private IQuery _querier;

        private Dictionary<ChatId, IMember> _sessions;

        public QueueBot(string token, IUpdateHandler updateHandler, IQuery querier) : base(token, updateHandler)
        {
            _querier = querier;
            _sessions = new Dictionary<ChatId, IMember>();

            AddStartWelcome();
            AddExpertBranch();
            AddStudentBranch();
        }

        public void AddStartWelcome()
        {
            using StreamReader sr = new StreamReader(src + "startWelcome.txt");
            _updateHandler.AddResponse(
                null,
                new List<string> { "/start" },
                SendMessageResponse(
                    text: sr.ReadLine(),
                    keyboard: new Keyboard(new List<string> { "Студент", "Эксперт" }).GetReplyMarkup()
                )
            );
        }

        #region Expert branch
        public void AddExpertBranch()
        {
            AddExpertWelcomeResponse();

            AddGoogleSheetResponse();
            AddGoogleSheetQuery();
        }

        private void AddExpertWelcomeResponse()
        {
            using StreamReader sr = new StreamReader(src + "expertWelcome.txt");
            _updateHandler.AddResponse(
                null,
                new List<string> { "Эксперт" },
                SendMessageResponse(
                    text: sr.ReadLine(),
                    keyboard: new Keyboard(new List<string> { "Получить коды комнат", "Шаблон гугл таблицы" }).GetReplyMarkup()
                )
            );
        }

        private void AddGoogleSheetResponse()
        {
            _updateHandler.AddResponse(
                mes => mes.Text == "Шаблон гугл таблицы",
                null,
                SendMessageResponse(
                    files: new List<IFile>
                    {
                        new FileManager.File(FileType.Document, "Описание шаблона", "../../../../src/Описание шаблона.xlsx"),
                        new FileManager.File(FileType.Image, "Пример шаблона", "../../../../src/Пример шаблона.jpg"),
                        new FileManager.File(FileType.Document, "Пример шаблона", "../../../../src/Пример шаблона.xlsx")
                    },
                    keyboard: new Keyboard(new List<string> { "Получить коды комнат", "Шаблон гугл таблицы" })
                        .GetReplyMarkup())
                );
        }

        private void AddGoogleSheetQuery()
        {
            _updateHandler.AddResponse(
                (Message mes) => mes.Text == "Получить коды комнат",
                null,
                _updateHandler.GetQuery(this,
                             "Отправьте сюда ссылку на Google-таблицу.",
                             GetRoomsResponse()
                )
            );
        }
        #endregion

        #region Student
        public void AddStudentBranch()
        {
            AddStudentWelcomeResponse();

            AddConnectQuery();
        }

        private void AddConnectQuery()
        {
            //_updateHandler.AddResponse(
            //    (Message mes) => mes.Text == "Подключение к комнате",
            //    null,
            //    GetSendMessageFunction("Введите, пожалуйста, код, который вам выдали ваши эксперты. (Стикер)")
            //);

            _updateHandler.AddResponse(
                (Message mes) => mes.Text == "Подключиться к комнате",
                null,
                _updateHandler.GetQuery(this,
                             "Введите, пожалуйста, код, который вам выдали ваши эксперты. (Стикер)",
                             ConnectToRoomResponse()
                )
            );
        }

        private void AddStudentWelcomeResponse()
        {
            using StreamReader sr = new StreamReader("../../../../src/studentWelcome.txt");
            _updateHandler.AddResponse(
                (Message mes) => mes.Text == "Студент",
                null,
                SendMessageResponse(
                    text: sr.ReadLine(),
                    keyboard: new Keyboard(new List<string> { "Подключиться к комнате" }).GetReplyMarkup()
                )
            );
        }
        #endregion

        public Func<Update, Task> GetRoomsResponse()
        {
            Task RoomsResponse(Update update)
            {
                //var roomsTable = new TableIO(update.Message.Text);
                //var rooms = roomsTable.GetAllSheets();
                //var links = rooms.Select(room => new Room(room.Name, null, null).GetLink());

                List<Task> tasks = new List<Task>();

                List<Room> rooms = new List<Room>(_querier.GetRooms(update.Message.Text));

                tasks.Add(SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: "Коды комнат:\n" + String.Join("\n", rooms.Select(room => $"    {room.Name} - {new Room(room.Name, null, null).GetLink()}")),
                        replyMarkup: Keyboard.RemoveMarkup
                        ));

                //tasks.Add(Task.Run(() => {
                //    var commandsTable = new TableIO("1bUR8BaBnYjc5rkaC9DHW6vm79qu7UVGgqBOVuZgeMAc");
                //    foreach (string link in links)
                //    {
                //        commandsTable.CreateSheet(link);
                //    }
                //}));

                //tasks.Add(SendMessage(
                //        chatId: update.Message.Chat.Id,
                //        text: "Коды комнат:\n" + String.Join("\n", rooms.Select(room => $"    {room.Name} - {new Room(room.Name, null, null).GetLink()}")),
                //        replyMarkup: Keyboard.RemoveMarkup
                //        ));

                Task test = Task.Run(() => {
                    tasks.Last().Wait();
                    SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: "Дальше я сам буду брать всю информацию с вашей таблицы. (Стикер)",
                        replyMarkup: Keyboard.RemoveMarkup
                        );
                });

                tasks.Add(test);

                return Task.WhenAll(tasks);
            }

            return RoomsResponse;
        }

        public Func<Update, Task> ConnectToRoomResponse()
        {
            Task ResponseTask(Update update)
            {
                //var commandsTable = new TableIO("1bUR8BaBnYjc5rkaC9DHW6vm79qu7UVGgqBOVuZgeMAc");
                //var rooms = commandsTable.GetAllSheets();
                //var roomsNames = rooms.Select(sheet => sheet.Name);

                List<Room> rooms = new List<Room>(_querier.GetRooms(update.Message.Text));
                List<string> roomsNames = rooms.Select(room => room.Name).ToList();

                List<Task> tasks = new List<Task>();

                // завернуть условие в таск
                if (roomsNames.Contains(update.Message.Text))
                {
                    tasks.Add(Task.Run(() =>
                    {
                        // TODO: Использовать реализацию IMember
                        // MemberImpl member = new(name); - notification set to default
                        _sessions.Add(update.Message.Chat.Id, member);

                        return SendMessage(
                            chatId: update.Message.Chat.Id, 
                            text: "Подключилось.", 
                            replyMarkup: new Keyboard(new List<string> { "Посмотреть команды" }).GetReplyMarkup()
                         );
                    }));

                    _updateHandler.AddResponse(
                            (Message mes) => mes.Text == "Посмотреть команды",
                            null,
                            (update) =>
                            {
                                // TODO: Использовать реализацию IMember
                                // MemberImpl member = new(name); - notification set to default
                                List<Team> teams = _querier.GetTeamsByRoomId(_sessions[member]).ToList();
                                string t = String.Join('\n', teams.Select(team => team.Name));
                                return SendMessage(
                                    chatId: update.Message.Chat.Id,
                                    text: t
                                    );
                            }
                            // add response to connect by name
                        );
                }
                else
                {
                    tasks.Add(SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: "Нет такой комнаты."
                        ));
                }

                tasks.Add(Task.Run(() => { }));

                return Task.WhenAll(tasks);
            }

            return ResponseTask;
        }
    }
}
