using System;
using TelegramBot;
using System.IO;
using System.Collections.Generic;
using Telegram.Bot.Types;
using FileManager;
using System.Threading.Tasks;
using TableParser;
using System.Linq;

// TODO: Сделать создание сценария использования более удобным и поддерживаемым
// TODO: Парсить _rooms из базы данных
// TODO: Refactoring
// TODO: Разузнать про перекрёстный огонь
// TODO: Отправка стикеров

namespace QueueBot
{
    public class QueueBot : Bot
    {
        private const string src = "../../../../src/";

        private List<Tuple<IMember, Room, Team>> _sessions;

        private readonly List<INotificator> _notificators;
        private readonly DataBase _dataBase;

        private List<Room> _rooms;

        public QueueBot(string token, IUpdateHandler updateHandler, DataBase db) : base(token, updateHandler)
        {
            _sessions = new List<Tuple<IMember, Room, Team>>();
            _dataBase = db;
            _notificators = new List<INotificator>();
            _rooms = new List<Room>();

            AddStartWelcome();
            AddExpertBranch();
            AddStudentBranch();

            SetNotTwoTeamsResponse();

            SetNotAutoRepsonse();

            SetNotTenMinutesResponse();

            GetTeamsResponse();

            ConnectToTeamResponse();

            DisconnectFromTeamResponse();

            NotSettingsResponse();
        }

        private void NotSettingsResponse()
        {
            _updateHandler.AddResponse(
                                        (Message mes) => mes.Text == "Настройка уведомлений",
                                        null,
                                        (update) =>
                                        {
                                            return SendMessage(
                                                    chatId: update.Message.Chat.Id,
                                                    text: "Выберите подходящий вариант:",
                                                    replyMarkup: new Keyboard(new List<string> { "За 10 минут", "За две команды", "Автоматически", "Назад" }).GetReplyMarkup()
                                                );
                                        }
                                    );
        }

        private void DisconnectFromTeamResponse()
        {
            _updateHandler.AddResponse(
                                        (Message mes) => mes.Text == "Покинуть команду",
                                        null,
                                        (update) =>
                                        {
                                            var currentSession = _sessions.Where(session => session.Item1.Name == update.Message.Chat.Id.ToString()).First();

                                            _dataBase.RemoveMemberFromTeam(
                                                currentSession.Item2,
                                                currentSession.Item3,
                                                currentSession.Item1
                                            );

                                            return SendMessage(
                                                chatId: update.Message.Chat.Id,
                                                text: "Disconnected"
                                            );
                                        }
                                    );
        }

        private void ConnectToTeamResponse()
        {
            _updateHandler.AddResponse(
                                        (Message mes) =>
                                        {
                                            Room currentRoom = GetRoomByChatId(mes.Chat.Id);

                                            int num = 0;
                                            return int.TryParse(mes.Text, out num) && num >= 0 && num < currentRoom.Teams.Count
                                                || mes.Text == "Назад";
                                        },
                                        null,
                                        (update) =>
                                        {
                                            if (update.Message.Text != "Назад")
                                            {
                                                var currentSession = _sessions
                                                    .Where(session => session.Item1.Name == update.Message.Chat.Id.ToString())
                                                    .FirstOrDefault();

                                                var currentSessionIndex = _sessions.IndexOf(currentSession);

                                                IMember currentMember = currentSession.Item1;
                                                Room currentRoom = currentSession.Item2;

                                                int teamId = int.Parse(update.Message.Text);
                                                List<Team> teams = currentRoom.Teams;
                                                Team currentTeam = teams[teamId];

                                                _sessions[currentSessionIndex] = new Tuple<IMember, Room, Team>(currentMember, currentRoom, currentTeam);

                                                _dataBase.AddMemberToTeam(
                                                    currentRoom,
                                                    currentTeam,
                                                    currentMember
                                                );
                                            }

                                            return SendMessage(
                                                chatId: update.Message.Chat.Id,
                                                text: "Информация о команде",
                                                replyMarkup: new Keyboard(new List<string> { "Покинуть команду", "Настройка уведомлений" }).GetReplyMarkup()
                                            );
                                        }
                                    );
        }

        private void GetTeamsResponse()
        {
            _updateHandler.AddResponse(
                                        (Message mes) => mes.Text == "Посмотреть команды",
                                        null,
                                        (update) =>
                                        {
                                            Room currentRoom = GetRoomByChatId(update.Message.Chat.Id);
                                            List<Team> teams = currentRoom.Teams;

                                            string t = string.Join('\n', teams.Select(team => string.Format("{0} - {1}", teams.IndexOf(team), team.Name)));
                                            
                                            return SendMessage(
                                                chatId: update.Message.Chat.Id,
                                                text: t
                                            );
                                        }
                                    );
        }

        private void SetNotTenMinutesResponse()
        {
            _updateHandler.AddResponse(
                                        (Message mes) => mes.Text == "За 10 минут",
                                        null,
                                        (update) =>
                                        {
                                            var currentSession = _sessions.Where(session => session.Item1.Name == update.Message.Chat.Id.ToString()).First();
                                            currentSession.Item1.SetNotificationType(NotificationType.TEN_MINUTES);

                                            return SendMessage(
                                                    chatId: update.Message.Chat.Id,
                                                    text: "done",
                                                    replyMarkup: new Keyboard(new List<string> { "За 10 минут", "За две команды", "Автоматически", "Назад" }).GetReplyMarkup()
                                                );
                                        }
                                    );
        }

        private void SetNotAutoRepsonse()
        {
            _updateHandler.AddResponse(
                                        (Message mes) => mes.Text == "Автоматически",
                                        null,
                                        (update) =>
                                        {
                                            var currentSession = _sessions.Where(session => session.Item1.Name == update.Message.Chat.Id.ToString()).First();
                                            currentSession.Item1.SetNotificationType(NotificationType.AUTO);

                                            return SendMessage(
                                                    chatId: update.Message.Chat.Id,
                                                    text: "done",
                                                    replyMarkup: new Keyboard(new List<string> { "За 10 минут", "За две команды", "Автоматически", "Назад" }).GetReplyMarkup()
                                                );
                                        }
                                    );
        }

        private void SetNotTwoTeamsResponse()
        {
            _updateHandler.AddResponse(
                                        (Message mes) => mes.Text == "За две команды",
                                        null,
                                        (update) =>
                                        {
                                            var currentSession = _sessions.Where(session => session.Item1.Name == update.Message.Chat.Id.ToString()).First();
                                            currentSession.Item1.SetNotificationType(NotificationType.TWO_TEAMS);

                                            return SendMessage(
                                                    chatId: update.Message.Chat.Id,
                                                    text: "done",
                                                    replyMarkup: new Keyboard(new List<string> { "За 10 минут", "За две команды", "Автоматически", "Назад" }).GetReplyMarkup()
                                                );
                                        }
                                    );
        }

        private Room GetRoomByChatId(ChatId id)
        {
            if (_sessions.Count == 0)
                return null;
            return _sessions
                .Where(session => session.Item1.Name == id.ToString())
                .FirstOrDefault()
                .Item2;
        }

        private void AddStartWelcome()
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
        private void AddExpertBranch()
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

        private Func<Update, Task> GetRoomsResponse()
        {
            IEnumerable<Room> GetRooms(string tableId)
            {
                List<Room> res = new List<Room>();
                var roomsTable = new TableIO(tableId);
                foreach (var sheet in roomsTable.GetAllSheets())
                {
                    var rooms = RoomParser.CreateRooms(roomsTable, sheet).ToList();
                    res.AddRange(rooms);
                    foreach (var room in rooms)
                    {
                        var db = new TableIO("1GISEntayuaYagp7K9Zb13-YLGiDO6AaQIVfP39REkI0");
                        var not = new Notification(roomsTable, room);
                        not.StartPolling();
                        _notificators.Add(not);
                        _dataBase.WriteRoom(room);
                    }
                }

                return res;
            }

            Task RoomsResponse(Update update)
            {
                _rooms = new List<Room>(GetRooms(update.Message.Text));

                List<Task> tasks = new List<Task>();

                tasks.Add(SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: "Коды комнат:\n" + String.Join("\n", _rooms.Select(room => $"    {room.Name} - {room.Link}")),
                        replyMarkup: Keyboard.RemoveMarkup
                        ));

                tasks.Add(Task.Run(() => {
                    tasks.Last().Wait();
                    SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: "Дальше я сам буду брать всю информацию с вашей таблицы. (Стикер)",
                        replyMarkup: Keyboard.RemoveMarkup
                        );
                }));

                return Task.WhenAll(tasks);
            }

            return RoomsResponse;
        }
        #endregion

        #region Student
        private void AddStudentBranch()
        {
            AddStudentWelcomeResponse();

            AddConnectQuery();
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
            // if _sessions.contains(...) skip connect step
        }

        private void AddConnectQuery()
        {
            _updateHandler.AddResponse(
                (Message mes) => mes.Text == "Подключиться к комнате" || mes.Text == "Покинуть команду",
                null,
                _updateHandler.GetQuery(this,
                             "Введите, пожалуйста, код, который вам выдали ваши эксперты. (Стикер)",
                             ConnectToRoomResponse()
                )
            );
        }

        private Func<Update, Task> ConnectToRoomResponse()
        {
            Task ResponseTask(Update update)
            {
                var commandsTable = new TableIO("1GISEntayuaYagp7K9Zb13-YLGiDO6AaQIVfP39REkI0");
                var rooms = commandsTable.GetAllSheets();
                var roomsLinks = rooms.Select(sheet => sheet.Name);

                List<Task> tasks = new List<Task>();

                if (roomsLinks.Contains(update.Message.Text))
                {
                    tasks.Add(Task.Run(() =>
                    {
                        string currentRoomLink = update.Message.Text;

                        // get rooms from tech. table with teams and members
                        if (_rooms.Count == 0)
                        {
                            List<Room> res = new List<Room>();
                            var roomsTable = new TableIO("1jp7fIEEOrGEVtoFU5dvE3FIwzKzjwMzxKJy5YH04w24");
                            foreach (var sheet in roomsTable.GetAllSheets())
                            {
                                var rooms = RoomParser.CreateRooms(roomsTable, sheet).ToList();
                                res.AddRange(rooms);
                                foreach (var room in rooms)
                                {
                                    var not = new Notification(roomsTable, room);
                                    not.StartPolling();
                                    _notificators.Add(not);
                                    _dataBase.WriteRoom(room);
                                }
                            }

                            _rooms = res;
                        }

                        Room currentRoom = _rooms.Where(room => room.Link == currentRoomLink).FirstOrDefault();

                        var member = new Member(update.Message.Chat.Id.ToString(), NotificationType.TEN_MINUTES);
                        member.OnNotifyCalled += () => SendMessage(update.Message.Chat.Id, "Подключайтесь!");
                        _sessions.Add(new Tuple<IMember, Room, Team>(member, currentRoom, null));

                        SendMessage(
                            chatId: update.Message.Chat.Id,
                            text: "Подключилось.",
                            replyMarkup: new Keyboard(new List<string> { "Посмотреть команды" }).GetReplyMarkup()
                         );
                    }));
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
        #endregion
    }
}
