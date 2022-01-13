using System;
using TelegramBot;
using System.IO;
using System.Collections.Generic;
using Telegram.Bot.Types;
using SendableFiles;
using System.Threading.Tasks;
using TableParser;
using System.Linq;

// TODO: Сделать создание сценария использования более удобным и поддерживаемым
// TODO: Refactoring
// TODO: Протестировать перекрёстный огонь

namespace QueueBot
{
    public class BotData : Bot
    {

        private readonly List<Tuple<IMember, Room, Team>> _sessions;

        public List<INotificator> Notificators { get; }
        private DataBase DataBase { get; }

        private List<Room> _rooms;

        private readonly Random _random;

        public StickerSet ExpertStickers { get; }
        public StickerSet StudentStickers { get; }

        public BotData(string pathToToken, IUpdateHandler updateHandler, DataBase db)
            : base(ReadLineFromFile(pathToToken), updateHandler)
        {
            ExpertStickers = GetStickerSet("KattBot");
            StudentStickers = GetStickerSet("Urfu_Robot");

            // TODO: Убрать из конструктора вычисления и построения
            _random = new Random();
            _sessions = new List<Tuple<IMember, Room, Team>>();
            DataBase = db;
            Notificators = new List<INotificator>();
            _rooms = db.GetRooms().ToList();
            foreach (var room in _rooms)
            {
                if (room.Name != "Комната 1")
                    continue;

                foreach (var team in room.Teams)
                {
                    foreach (var member in team.Members)
                    {
                        member.OnNotifyCalled += () => SendMessage(long.Parse(member.Name), "Подключайтесь!");
                    }
                }

                var not = new Notification(room);
                not.StartPolling();
                Notificators.Add(not);
            }

            //AddExpertBranch();
            //AddStudentBranch();

            //SetNotTwoTeamsResponse();

            //SetNotAutoRepsonse();

            //SetNotTenMinutesResponse();

            //GetTeamsResponse();

            //ConnectToTeamResponse();

            //DisconnectFromTeamResponse();

            //NotSettingsResponse();
        }

        //private void AddResponseMessage(
        //    string textToResponse,
        //    string responseText,
        //    IEnumerable<ISendable> responseContent,
        //    Keyboard responseKeyboard)
        //{
        //    _updateHandler.AddResponse(
        //        (Message mes) => mes.Text == textToResponse,
        //        (update) =>
        //        {
        //            return SendMessageTest(
        //                update.Message.Chat.Id,
        //                responseText,
        //                responseContent,
        //                responseKeyboard
        //            );
        //        }
        //    );
        //}

        //private void AddResponseMessageWithSticker(
        //    string textToResponse,
        //    string responseText,
        //    IEnumerable<ISendable> responseContent,
        //    StickerSet stickerSet,
        //    int[] stickersIndexes,
        //    Keyboard responseKeyboard)
        //{
        //    _updateHandler.AddResponse(
        //        (Message mes) => mes.Text == textToResponse,
        //        (update) =>
        //        {
        //            responseContent ??= new ISendable[] { };
        //            return SendMessageTest(
        //                update.Message.Chat.Id,
        //                responseText,
        //                responseContent
        //                    .Concat((ISendable[])stickerSet.GetRandomSticker(stickersIndexes).GetSendableSticker()),
        //                responseKeyboard
        //            );
        //        }
        //    );
        //}

        public static string ReadLineFromFile(string path)
        {
            using StreamReader sr = new StreamReader(path);
            return sr.ReadLine();
        }


        //#region Constructors


        //#region Expert branch
        

        

        //private void AddGoogleSheetQuery()
        //{
        //    //AddResponseQuery(
        //    //        "Получить коды комнат",
        //    //        _updateHandler.GetQuery(GetRoomsResponse())
        //    //    );

        //    //AddResponseMessageWithSticker(
        //    //        "Получить коды комнат",
        //    //        "Отправьте сюда ссылку на Google-таблицу.",
        //    //        null,
        //    //        _expertStickers,
        //    //        new[] { 2, 3 },
        //    //        null
        //    //    );
        //}

        ////private void AddResponseQuery(
        ////    string textToResponse,
        ////    Func<Update, Task> queryTask)
        ////{
        ////    _updateHandler.AddResponse(
        ////        (Message mes) => mes.Text == textToResponse,
        ////        (update) => queryTask(update)
        ////    );
        ////}
        //#endregion

        //private void NotSettingsResponse()
        //{
        //    //AddResponseMessageWithSticker(
        //    //        "Настройка уведомлений",
        //    //        "Укажите, когда мне уведомить о вашем выступлении.",
        //    //        null,
        //    //        _studentStickers,
        //    //        new[] { 9, 10 },
        //    //        new Keyboard(new List<string> { "За 10 минут", "За две команды", "Автоматически", "Назад" })
        //    //    );
        //}

        //private void DisconnectFromTeamResponse()
        //{
        //    //_updateHandler.AddResponse(
        //    //    (Message mes) => mes.Text == "Покинуть команду",
        //    //    (update) =>
        //    //    {
        //    //        var currentSession = _sessions.Where(session => session.Item1.Name == update.Message.Chat.Id.ToString()).First();
        //    //        currentSession.Item1.OnNotifyCalled -= () => SendMessage(update.Message.Chat.Id, "Подключайтесь!");
        //    //        _dataBase.RemoveMemberFromTeam(
        //    //            currentSession.Item2,
        //    //            currentSession.Item3,
        //    //            currentSession.Item1
        //    //        );

        //    //        return SendMessage(
        //    //            chatId: update.Message.Chat.Id,
        //    //            text: "Disconnected"
        //    //        );
        //    //    }
        //    //);
        //}

        //private void ConnectToTeamResponse()
        //{
        ////    _updateHandler.AddResponse(
        ////                                (Message mes) =>
        ////                                {
        ////                                    Room currentRoom = GetRoomByChatId(mes.Chat.Id);

        ////                                    int num = 0;
        ////                                    return int.TryParse(mes.Text, out num) && num >= 0 && num < currentRoom.Teams.Count
        ////                                        || mes.Text == "Назад";
        ////                                },
        ////                                (update) =>
        ////                                {
        ////                                    if (update.Message.Text != "Назад")
        ////                                    {
        ////                                        var currentSession = _sessions
        ////                                            .Where(session => session.Item1.Name == update.Message.Chat.Id.ToString())
        ////                                            .FirstOrDefault();

        ////                                        var currentSessionIndex = _sessions.IndexOf(currentSession);

        ////                                        IMember currentMember = currentSession.Item1;
        ////                                        Room currentRoom = currentSession.Item2;

        ////                                        int teamId = int.Parse(update.Message.Text);
        ////                                        List<Team> teams = currentRoom.Teams;
        ////                                        Team currentTeam = teams[teamId];

        ////                                        _sessions[currentSessionIndex] = new Tuple<IMember, Room, Team>(currentMember, currentRoom, currentTeam);

        ////                                        _dataBase.AddMemberToTeam(
        ////                                            currentRoom,
        ////                                            currentTeam,
        ////                                            currentMember
        ////                                        );
        ////                                    }

        ////                                    var stickers = new List<ISendable> {
        ////                                                new SendableFiles.Sticker("CAACAgIAAxkBAAEDfnFhuihYCd06zNFzS7Tmq_RZYWUvGQACNB0AAlbNcEnrfWT3dUwAAXkjBA"),
        ////                                                new SendableFiles.Sticker("CAACAgIAAxkBAAEDfnNhuihbOKDNy0zgp94Y5KdUgvyilgAC3RIAAlVtcUlTPrU7vxiuSiME")
        ////                                            };

        ////                                    int n = _random.Next(0, 2);

        ////                                    return SendMessageResponse(
        ////                                            "Дальше вы можете покинуть команду, если попали не туда, вернуться к просмотру всех команд или настроить уведомления.",
        ////                                            new List<ISendable> { stickers[n] },
        ////                                            new Keyboard(new List<string> { "Настройка уведомлений", "Покинуть команду" }).GetReplyMarkup()
        ////                                        )(update);
        ////                                }
        ////                            );
        //}

        //private void GetTeamsResponse()
        //{
        //    //_updateHandler.AddResponse(
        //    //                            (Message mes) => mes.Text == "Посмотреть команды",
        //    //                            (update) =>
        //    //                            {
        //    //                                Room currentRoom = GetRoomByChatId(update.Message.Chat.Id);
        //    //                                List<Team> teams = currentRoom.Teams;

        //    //                                string t = string.Join('\n', teams.Select(team => string.Format("{0} - {1}", teams.IndexOf(team), team.Name)));

        //    //                                return SendMessage(
        //    //                                    chatId: update.Message.Chat.Id,
        //    //                                    text: t
        //    //                                );
        //    //                            }
        //    //                        );
        //}

        //private void SetNotTenMinutesResponse()
        //{
        //    //_updateHandler.AddResponse(
        //    //                            (Message mes) => mes.Text == "За 10 минут",
        //    //                            (update) =>
        //    //                            {
        //    //                                var currentSession = _sessions.Where(session => session.Item1.Name == update.Message.Chat.Id.ToString()).FirstOrDefault();
        //    //                                currentSession.Item1.SetNotificationType(NotificationType.TEN_MINUTES);
        //    //                                _dataBase.WriteRoom(currentSession.Item2);

        //    //                                var stickers = new List<ISendable> {
        //    //                                            new SendableFiles.Sticker("CAACAgIAAxkBAAEDfoNhuiqP35kXR98ne7HgfZsQXvoSsgAC9RIAApK2iUmPqphPUh4TbCME"),
        //    //                                            new SendableFiles.Sticker("CAACAgIAAxkBAAEDfoRhuiqQiB4RdSW0oFr9SnhI06X7mAACnxEAAkfUiUmq61SvrE6ekyME")
        //    //                                        };

        //    //                                int n = _random.Next(0, 2);

        //    //                                return SendMessageResponse(
        //    //                                        "Настройки зафиксированы. Ожидайте оповещения.",
        //    //                                        new List<ISendable> { stickers[n] },
        //    //                                        new Keyboard(new List<string> { "За 10 минут", "За две команды", "Автоматически", "Назад" }).GetReplyMarkup()
        //    //                                    )(update);
        //    //                            }
        //    //                        );
        //}

        //private void SetNotAutoRepsonse()
        //{
        //    //_updateHandler.AddResponse(
        //    //                            (Message mes) => mes.Text == "Автоматически",
        //    //                            (update) =>
        //    //                            {
        //    //                                var currentSession = _sessions.Where(session => session.Item1.Name == update.Message.Chat.Id.ToString()).First();
        //    //                                currentSession.Item1.SetNotificationType(NotificationType.AUTO);
        //    //                                _dataBase.WriteRoom(currentSession.Item2);

        //    //                                Task[] t = new Task[2];

        //    //                                var stickers = new List<ISendable> {
        //    //                                            new SendableFiles.Sticker("CAACAgIAAxkBAAEDfodhuiuZuZrJo74uPwRENWp-_Tah1wACWhMAAjPbiUnrQj-0UDequiME"),
        //    //                                            new SendableFiles.Sticker("CAACAgIAAxkBAAEDfohhuiualQ1tEJtMloszPnMAAehO6VgAAjASAAK0zYlJL7qobRafj0cjBA")
        //    //                                        };

        //    //                                int n = _random.Next(0, 2);

        //    //                                t[0] = SendMessageResponse(
        //    //                                        "Высчитывается среднее арифметическое времени выступлений всех команд, которые уже выступили, и уведомление приходит за рассчитанное время до вашего выступления.",
        //    //                                        new List<ISendable> { stickers[n] },
        //    //                                        new Keyboard(new List<string> { "За 10 минут", "За две команды", "Автоматически", "Назад" }).GetReplyMarkup()
        //    //                                    )(update);

        //    //                                stickers = new List<ISendable> {
        //    //                                            new SendableFiles.Sticker("CAACAgIAAxkBAAEDfoNhuiqP35kXR98ne7HgfZsQXvoSsgAC9RIAApK2iUmPqphPUh4TbCME"),
        //    //                                            new SendableFiles.Sticker("CAACAgIAAxkBAAEDfoRhuiqQiB4RdSW0oFr9SnhI06X7mAACnxEAAkfUiUmq61SvrE6ekyME")
        //    //                                        };

        //    //                                n = _random.Next(0, 2);

        //    //                                t[1] = SendMessageResponse(
        //    //                                        "Настройки зафиксированы. Ожидайте оповещения.",
        //    //                                        new List<ISendable> { stickers[n] },
        //    //                                        new Keyboard(new List<string> { "За 10 минут", "За две команды", "Автоматически", "Назад" }).GetReplyMarkup()
        //    //                                    )(update);

        //    //                                return Task.WhenAll(t);
        //    //                            }
        //    //                        );
        //}

        //private void SetNotTwoTeamsResponse()
        //{
        //    //_updateHandler.AddResponse(
        //    //                            (Message mes) => mes.Text == "За две команды",
        //    //                            (update) =>
        //    //                            {
        //    //                                var currentSession = _sessions.Where(session => session.Item1.Name == update.Message.Chat.Id.ToString()).First();
        //    //                                currentSession.Item1.SetNotificationType(NotificationType.TWO_TEAMS);
        //    //                                _dataBase.WriteRoom(currentSession.Item2);

        //    //                                var stickers = new List<ISendable> {
        //    //                                            new SendableFiles.Sticker("CAACAgIAAxkBAAEDfoNhuiqP35kXR98ne7HgfZsQXvoSsgAC9RIAApK2iUmPqphPUh4TbCME"),
        //    //                                            new SendableFiles.Sticker("CAACAgIAAxkBAAEDfoRhuiqQiB4RdSW0oFr9SnhI06X7mAACnxEAAkfUiUmq61SvrE6ekyME")
        //    //                                        };

        //    //                                int n = _random.Next(0, 2);

        //    //                                return SendMessageResponse(
        //    //                                        "Настройки зафиксированы. Ожидайте оповещения.",
        //    //                                        new List<ISendable> { stickers[n] },
        //    //                                        new Keyboard(new List<string> { "За 10 минут", "За две команды", "Автоматически", "Назад" }).GetReplyMarkup()
        //    //                                    )(update);
        //    //                            }
        //    //                        );
        //}

        //private Room GetRoomByChatId(ChatId id)
        //{
        //    // TODO: V
        //    if (_sessions.Count == 0)
        //        return null;
        //    var session = _sessions
        //        .Where(session => session.Item1.Name == id.ToString())
        //        .FirstOrDefault();
        //    return session?.Item2;
        //}

        //#region Student
        //private void AddStudentBranch()
        //{
        //    AddStudentWelcomeResponse();

        //    AddConnectQuery();
        //}

        //private void AddStudentWelcomeResponse()
        //{
        //    //_updateHandler.AddResponse(
        //    //    (Message mes) => mes.Text == "Студент" || mes.Text == "Покинуть команду",
        //    //    (update) =>
        //    //    {
        //    //        if (update.Message.Text == "Покинуть команду")
        //    //        {
        //    //            Message m = new Message();
        //    //            m.Chat = update.Message.Chat;
        //    //            m.From = update.Message.From;
        //    //            m.Text = "Посмотреть команды";
        //    //            _updateHandler.InvokeMessage(_botClient, m);
        //    //            return Task.CompletedTask;
        //    //        }

        //    //        using StreamReader sr = new StreamReader("../../../../src/studentWelcome.txt");
        //    //        var currentSession = _sessions.Where(s => s.Item1.Name == update.Message.Chat.Id.ToString()).FirstOrDefault();
        //    //        if (currentSession == null)
        //    //        {
        //    //            var stickers = new List<ISendable> {
        //    //                    new SendableFiles.Sticker("CAACAgIAAxkBAAEDflNhuiEpwcZKiXXs4MJTd382XiLbEwACMBIAAhkxYEnjBg8TiaZlpiME"),
        //    //                    new SendableFiles.Sticker("CAACAgIAAxkBAAEDflVhuiEtt5u-PHw0cH_I9bHJWvW9dQAC0xEAArdPaEkDm3vqhyVFiSME")
        //    //                };

        //    //            int n = _random.Next(0, 2);
        //    //            return SendMessageResponse(
        //    //                text: sr.ReadLine(),
        //    //                files: new List<ISendable> { stickers[n] },
        //    //                keyboard: new Keyboard(new List<string> { "Подключиться к комнате" }).GetReplyMarkup()
        //    //            )(update);
        //    //        }
        //    //        else
        //    //        {
        //    //            Message m = new Message();
        //    //            m.Chat = update.Message.Chat;
        //    //            m.From = update.Message.From;
        //    //            m.Text = "Назад";
        //    //            _updateHandler.InvokeMessage(_botClient, m);
        //    //            return Task.CompletedTask;
        //    //        }
        //    //    }
        //    //);
        //}

        //private void AddConnectQuery()
        //{
        //    var stickers = new List<ISendable> {
        //                        new SendableFiles.Sticker("CAACAgIAAxkBAAEDfldhuiMEMlMQ6HS1YGmKxFj9y9qo_gACHBMAAiIuYEmQsf3riDZcGyME"),
        //                        new SendableFiles.Sticker("CAACAgIAAxkBAAEDfllhuiMIWNMnGuTDkpCgF44FMfRL3AACqBAAApDsaEnJId7n5IGh-yME")
        //                    };

        //    //AddResponseQuery(
        //    //        "Подключиться к комнате",
        //    //        _updateHandler.GetQuery(ConnectToRoomResponse())
        //    //    );

        //    //AddResponseMessage(
        //    //        "Подключиться к комнате",
        //    //        "Введите, пожалуйста, код, который вам выдали ваши эксперты.",
        //    //         new List<ISendable> { stickers[_random.Next(0, 2)] },
        //    //         null
        //    //    );
        //}

        //private Func<Update, Task> ConnectToRoomResponse()
        //{
        //    Task ResponseTask(Update update)
        //    {
        //        var commandsTable = new TableIO("1GISEntayuaYagp7K9Zb13-YLGiDO6AaQIVfP39REkI0");
        //        var rooms = commandsTable.GetAllSheets();
        //        var roomsLinks = rooms.Select(sheet => sheet.Name);

        //        List<Task> tasks = new List<Task>();

        //        if (roomsLinks.Contains(update.Message.Text))
        //        {
        //            tasks.Add(Task.Run(() =>
        //            {
        //                string currentRoomLink = update.Message.Text;

        //                Room currentRoom = _rooms.Where(room => room.Link == currentRoomLink).FirstOrDefault();

        //                var member = new Member(update.Message.Chat.Id.ToString());
        //                member.OnNotifyCalled += () => SendMessage(update.Message.Chat.Id, "Подключайтесь!");
        //                _sessions.Add(new Tuple<IMember, Room, Team>(member, currentRoom, null));

        //                //SendMessage(
        //                //    chatId: update.Message.Chat.Id,
        //                //    text: "Подключилось.",
        //                //    replyMarkup: new Keyboard(new List<string> { "Посмотреть команды" }).GetReplyMarkup()
        //                // );

        //                var stickers = new List<ISendable> {
        //                    new SendableFiles.Sticker("CAACAgIAAxkBAAEDflthuiV1RNK1oYzNtFggB1Ix_lXXjAACxRUAAqMJaUlpUY5ODuCPQiME"),
        //                            new SendableFiles.Sticker("CAACAgIAAxkBAAEDfl1huiV4n_8ePO6Sj9YUekSG7UWgewACaBgAAiGmaUnddaIF8ZclSSME")
        //                        };

        //                int n = _random.Next(0, 2);

        //                SendMessageResponse(
        //                        text: "Тут вы можете посмотреть все существующие команды в данной комнате и выбрать нужную вам.",
        //                        new List<ISendable> { stickers[n] },
        //                        new Keyboard(new List<string> { "Посмотреть команды" }).GetReplyMarkup()
        //                    )(update);
        //            }));
        //        }
        //        else
        //        {
        //            tasks.Add(SendMessage(
        //                chatId: update.Message.Chat.Id,
        //                text: "Нет такой комнаты."
        //                ));
        //        }

        //        tasks.Add(Task.Run(() => { }));

        //        return Task.WhenAll(tasks);
        //    }

        //    return ResponseTask;
        //}
        //#endregion

        //#endregion
    }
}
