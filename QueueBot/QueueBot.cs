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

            // TODO: Парсить _rooms из базы данных
            // Нужно также хранить название комнаты в базе данных
            //_rooms = new List<Room>();
            _rooms = db.GetRooms().ToList();

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
                                            currentSession.Item1.OnNotifyCalled -= () => SendMessage(update.Message.Chat.Id, "Подключайтесь!");
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
                                                replyMarkup: new Keyboard(new List<string> { "Настройка уведомлений", "Покинуть команду" }).GetReplyMarkup()
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
                                            var currentSession = _sessions.Where(session => session.Item1.Name == update.Message.Chat.Id.ToString()).FirstOrDefault();
                                            currentSession.Item1.SetNotificationType(NotificationType.TEN_MINUTES);
                                            _dataBase.WriteRoom(currentSession.Item2);

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
                                            _dataBase.WriteRoom(currentSession.Item2);

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
                                            _dataBase.WriteRoom(currentSession.Item2);

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
            var session = _sessions
                .Where(session => session.Item1.Name == id.ToString())
                .FirstOrDefault();
            return session?.Item2;
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

                tasks.Add(Task.Run(async () => {
                    await SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: "Коды комнат:\n" + string.Join("\n", _rooms.Select(room => $"    [{room.StartTime:g}] {room.Name} - {room.Link}")),
                        replyMarkup: Keyboard.RemoveMarkup
                        );
                    await SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: "Дальше я сам буду брать всю информацию с вашей таблицы. (Стикер)",
                        replyMarkup: new Keyboard(new List<string> { "Получить коды комнат", "Шаблон гугл таблицы" }).GetReplyMarkup()
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
            _updateHandler.AddResponse(
                (Message mes) => mes.Text == "Студент" || mes.Text == "Покинуть команду",
                null,
                (update) =>
                {
                    if (update.Message.Text == "Покинуть команду")
                    {
                        Message m = new Message();
                        m.Chat = update.Message.Chat;
                        m.From = update.Message.From;
                        m.Text = "Посмотреть команды";
                        _updateHandler.InvokeMessage(_botClient, m);
                        return Task.CompletedTask;
                    }

                    using StreamReader sr = new StreamReader("../../../../src/studentWelcome.txt");
                    var currentSession = _sessions.Where(s => s.Item1.Name == update.Message.Chat.Id.ToString()).FirstOrDefault();
                    if (currentSession == null)
                    {
                        return SendMessageResponse(
                            text: sr.ReadLine(),
                            keyboard: new Keyboard(new List<string> { "Подключиться к комнате" }).GetReplyMarkup()
                        )(update);
                    }
                    else
                    {
                        Message m = new Message();
                        m.Chat = update.Message.Chat;
                        m.From = update.Message.From;
                        m.Text = "Назад";
                        _updateHandler.InvokeMessage(_botClient, m);
                        return Task.CompletedTask;
                    }
                }
            );
        }

        private void AddConnectQuery()
        {
            _updateHandler.AddResponse(
                (Message mes) => mes.Text == "Подключиться к комнате",
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

                        Room currentRoom = _rooms.Where(room => room.Link == currentRoomLink).FirstOrDefault();

                        var member = new Member(update.Message.Chat.Id.ToString());
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
