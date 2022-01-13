using System;
using System.IO;
using TelegramBot;
using TelegramBot.UpdateHandler;
using Telegram.Bot.Types;
using SendableFiles;
using System.Collections.Generic;
using System.Linq;
using TableParser;

// TODO: Parse rooms from DB after starting
// TODO: Refactoring
// TODO: Протестировать перекрёстный огонь
// TODO: Add resources in build

namespace BotConstructor
{
    enum SYSTEM_COMMANDS
    {
        PARSE_ROOMS_FROM_TABLE,
        CONNECT_TO_ROOM,
        NOTIFY_ALL
    }

    public class Builder : IBuilder
    {
        private const string src = "../../../../src/";

        private readonly Bot _bot;

        private readonly StickerSet _expertStickers;
        private readonly StickerSet _studentStickers;

        public Builder(Bot botData)
        {
            _bot = botData;

            _expertStickers = _bot.GetStickerSet("KattBot");
            _studentStickers = _bot.GetStickerSet("Urfu_Robot");
        }

        public void Build()
        {
            ConfigureCommands();
            
            AddStartResponse();
            AddExpertBranch();
            AddStudentBranch();
        }

        private static string ReadLineFromFile(string path)
        {
            using StreamReader sr = new(path);
            return sr.ReadLine();
        }

        private Room GetRoomByChatId(ChatId id)
        {
            // TODO: V
            return _bot.Data.Sessions
                .Where(session => session.Member.Name == id.ToString())
                .FirstOrDefault()
                .Room;
        }

        private void AddDefaultResponse(Response.FilterFunc filter, Response.ResponseFunc response)
        {
            DefaultResponse r = new(filter, response);
            AddDefaultResponse(r);
        }

        private void AddDefaultResponse(DefaultResponse response)
        {
            _bot.UpdateHandler.AddResponse(response);
        }

        // COMPLETE
        private void AddStartResponse()
        {
            AddDefaultResponse(
                    (Message mes) => mes.Text == "/start",
                    (Update upd) => 
                    {
                        _bot.SendMessageAsync(
                                upd.Message.Chat.Id,
                                ReadLineFromFile(src + "startWelcome.txt"),
                                null,
                                new Keyboard(new List<string> { "Студент", "Эксперт" })
                            );
                    }
                );
        }

        private void ConfigureCommands()
        {
            AddDefaultResponse(
                    (Message mes) =>
                    {
                        return !int.TryParse(mes.Text, out _) &&
                            Enum.TryParse(typeof(SYSTEM_COMMANDS), mes.Text, false, out var com) &&
                            (SYSTEM_COMMANDS)com == SYSTEM_COMMANDS.PARSE_ROOMS_FROM_TABLE;
                    },
                    (Update upd) =>
                    {
                        _bot.UpdateHandler.AddQuery(upd, ParseTableResponseFunc);
                    }
                );

            AddDefaultResponse(
                    (Message mes) =>
                    {
                        return !int.TryParse(mes.Text, out _) && 
                            Enum.TryParse(typeof(SYSTEM_COMMANDS), mes.Text, false, out var com) &&
                            (SYSTEM_COMMANDS)com == SYSTEM_COMMANDS.CONNECT_TO_ROOM;
                    },
                    (Update upd) =>
                    {
                        _bot.UpdateHandler.AddQuery(upd, ConnectToRoomResponseFunc);
                    }
                );

            AddDefaultResponse(
                    (Message mes) =>
                    {
                        return !int.TryParse(mes.Text, out _) &&
                            Enum.TryParse(typeof(SYSTEM_COMMANDS), mes.Text, false, out var com) &&
                            (SYSTEM_COMMANDS)com == SYSTEM_COMMANDS.NOTIFY_ALL;
                    },
                    (Update upd) =>
                    {
                        foreach (var session in _bot.Data.Sessions)
                        {
                            session.Member.Notify();
                        }
                    }
                );
        }

        #region Expert branch

        private void AddExpertBranch()
        {
            AddExpertWelcomeResponse();

            AddGoogleSheetPattern();
            AddGoogleSheetQuery();
        }

        // COMPLETE
        private void AddExpertWelcomeResponse()
        {
            AddDefaultResponse(
                    (Message mes) => mes.Text == "Эксперт",
                    (Update upd) => 
                    {
                        _bot.SendMessageAsync(
                                upd.Message.Chat.Id,
                                ReadLineFromFile(src + "expertWelcome.txt"),
                                (ISendable[])_expertStickers.GetRandomSticker(1, 2).GetSendableSticker(),
                                new Keyboard("Получить коды комнат", "Шаблон гугл таблицы")
                            );
                    }
                );
        }

        // COMPLETE
        // TODO: Order files
        private void AddGoogleSheetPattern()
        {
            var files = new List<ISendable>
            {
                new SendableFiles.Document(src + "Описание шаблона.xlsx", "Описание шаблона"),
                new SendableFiles.Image(src + "Пример шаблона.jpg", "Пример шаблона"),
                new SendableFiles.Document(src + "Пример шаблона.xlsx", "Пример шаблона")
            };

            AddDefaultResponse(
                    (Message mes) => mes.Text == "Шаблон гугл таблицы",
                    (Update upd) => 
                    {
                        _bot.SendMessageAsync(
                                upd.Message.Chat.Id,
                                ReadLineFromFile(src + "exampleAlert.txt"),
                                files.Concat((ISendable[])_expertStickers.GetRandomSticker(6, 7).GetSendableSticker()),
                                new Keyboard("Получить коды комнат", "Шаблон гугл таблицы")
                            );
                    }
                );
        }

        // COMPLETE
        private void AddGoogleSheetQuery()
        {
            AddDefaultResponse(
                    (Message mes) => mes.Text == "Получить коды комнат",
                    (Update upd) => 
                    {
                        // start new iteration
                        Message mes = upd.Message;
                        mes.Text = SYSTEM_COMMANDS.PARSE_ROOMS_FROM_TABLE.ToString();
                        _bot.InvokeMessage(mes);

                        _bot.SendMessageAsync(
                                upd.Message.Chat.Id,
                                "Отправьте сюда ссылку на Google-таблицу.",
                                (ISendable[])_expertStickers.GetRandomSticker(3, 4).GetSendableSticker(),
                                null
                            );
                    }
                );
        }

        // COMPLETE
        private bool TryParseRooms(string tableUrl, out Room[] outRooms)
        {
            List<Room> res = new();
            bool isSuccess = false;

            try
            {
                string tableId = tableUrl.Split('/')[5];
                var roomsTable = new TableIO(tableId);

                // TODO: ???

                foreach (var sheet in roomsTable.GetAllSheets())
                {
                    var sheetRooms = roomsTable.ParseRooms(sheet).ToList();

                    res.AddRange(sheetRooms);

                    foreach (var room in sheetRooms)
                    {
                        var not = new Notificator(roomsTable, room);
                        not.StartPolling();
                        _bot.Data.Notificators.Add(not);
                        _bot.Data.DataBase.WriteRoom(room);
                    }
                }

                isSuccess = true;
            }
            catch
            {
            }
            finally
            {
                outRooms = res.ToArray();
            }

            return isSuccess;
        }

        // COMPLETE
        void ParseTableResponseFunc(Update update)
        {
            if (TryParseRooms(update.Message.Text, out Room[] newRooms))
            {
                _bot.SendMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: "Коды комнат:\n" + string.Join("\n", newRooms.Select(room => $"\t[{room.StartTime:g}] {room.Name} - {room.Link}"))
                        );

                _bot.SendMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: "Дальше я сам буду брать всю информацию с вашей таблицы. (Стикер)",
                            content: (ISendable[])_expertStickers.GetRandomSticker(5, 8).GetSendableSticker()
                        );
            }
            else
            {
                // start new iteration
                Message mes = update.Message;
                mes.Text = SYSTEM_COMMANDS.PARSE_ROOMS_FROM_TABLE.ToString();
                _bot.InvokeMessage(mes);

                _bot.SendMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "Вы отправили неправильную ссылку. Попробуйте снова."
                );
            }
        }

        #endregion

        #region Student branch

        private void AddStudentBranch()
        {
            AddStudentWelcomeResponse();

            AddConnectQuery();

            GetTeamsResponse();
            SetNotTenMinutesResponse();
            SetNotAutoRepsonse();
            SetNotTwoTeamsResponse();
            NotSettingsResponse();

            // TODO: Rewrite with system commands
            DisconnectFromTeamResponse();
            ConnectToTeamResponse();
        }

        // TODO: move "leave team" to next step
        private void AddStudentWelcomeResponse()
        {
            AddDefaultResponse(
                (Message mes) => mes.Text == "Студент" || mes.Text == "Покинуть команду",
                (update) =>
                {
                    if (update.Message.Text == "Покинуть команду")
                    {
                        Message m = update.Message;
                        m.Text = "Посмотреть команды";
                        _bot.InvokeMessage(m);
                    }

                    var currentSession = _bot.Data.Sessions
                        .Where(s => s.Member.Name == update.Message.Chat.Id.ToString())
                        .FirstOrDefault();

                    if (currentSession.Member == null)
                    {
                        _bot.SendMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: ReadLineFromFile(src + "studentWelcome.txt"),
                            content: (ISendable[])_studentStickers.GetRandomSticker(1, 2).GetSendableSticker(),
                            keyboard: new Keyboard(new List<string> { "Подключиться к комнате" })
                            );
                    }
                    else
                    {
                        Message m = update.Message;
                        m.Text = "Назад";
                        _bot.InvokeMessage(m);
                    }
                }
            );
        }

        // COMPLETE
        private void AddConnectQuery()
        {
            AddDefaultResponse(
                    (Message mes) => mes.Text == "Подключиться к комнате",
                    (Update upd) =>
                    {
                        Message m = upd.Message;
                        m.Text = SYSTEM_COMMANDS.CONNECT_TO_ROOM.ToString();
                        _bot.InvokeMessage(m);

                        _bot.SendMessageAsync(
                            chatId: upd.Message.Chat.Id,
                            text: "Введите, пожалуйста, код, который вам выдали ваши эксперты.",
                            content: (ISendable[])_studentStickers.GetRandomSticker(3, 4).GetSendableSticker()
                            );
                    }
                );
        }

        private void ConnectToRoomResponseFunc(Update update)
        {
            var commandsTable = _bot.Data.DataBase.Table;
            var rooms = commandsTable.GetAllSheets();
            var roomsLinks = rooms.Select(sheet => sheet.Name);

            if (roomsLinks.Contains(update.Message.Text))
            {
                string currentRoomLink = update.Message.Text;

                // TODO: Parse rooms from Linker sheet
                Room currentRoom = _bot.Data.DataBase.GetRooms()
                    .Where(room => room.Link == currentRoomLink).FirstOrDefault();

                var member = new Member(update.Message.Chat.Id.ToString());

                member.OnNotifyCalled += () => _bot.SendMessageAsync(update.Message.Chat.Id, "Подключайтесь!");

                _bot.Data.Sessions.Add(new Session { Member = member, Room = currentRoom, Team = null });

                _bot.SendMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: "Тут вы можете посмотреть все существующие команды в данной комнате и выбрать нужную вам.",
                        content: (ISendable[])_studentStickers.GetRandomSticker(5, 6).GetSendableSticker(),
                        new Keyboard(new List<string> { "Посмотреть команды" })
                    );
            }
            else
            {
                Message m = update.Message;
                m.Text = SYSTEM_COMMANDS.CONNECT_TO_ROOM.ToString();
                _bot.InvokeMessage(m);

                _bot.SendMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "Такой комнаты нет. Попробуйте ещё раз."
                    );
            }
        }

        // TODO: Check
        private void GetTeamsResponse()
        {
            AddDefaultResponse(
                (Message mes) => mes.Text == "Посмотреть команды",
                (update) =>
                    {
                        Room currentRoom = GetRoomByChatId(update.Message.Chat.Id);
                        List<Team> teams = currentRoom.Teams;

                        string t = string.Join('\n', teams.Select(team => string.Format("{0} - {1}", teams.IndexOf(team) + 1, team.Name)));

                        _bot.SendMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: t
                        );
                    }
            );
        }

        // TODO: Remake as query
        private void ConnectToTeamResponse()
        {
            AddDefaultResponse(
                (Message mes) =>
                {
                    Room currentRoom = GetRoomByChatId(mes.Chat.Id);
                    return int.TryParse(mes.Text, out int num) && num >= 1 && num < currentRoom.Teams.Count + 1
                        || mes.Text == "Назад";
                },
                (update) =>
                {
                    if (update.Message.Text != "Назад")
                    {
                        var currentSession = _bot.Data.Sessions
                            .Where(session => session.Member.Name == update.Message.Chat.Id.ToString())
                            .FirstOrDefault();


                        var currentSessionIndex = _bot.Data.Sessions.IndexOf(currentSession);

                        IMember currentMember = currentSession.Member;
                        Room currentRoom = currentSession.Room;

                        int teamId = int.Parse(update.Message.Text) - 1;

                        List<Team> teams = currentRoom.Teams;

                        Team currentTeam = teams[teamId];

                        _bot.Data.Sessions[currentSessionIndex] = new Session
                        {
                            Member = currentMember,
                            Room = currentRoom,
                            Team = currentTeam
                        };

                        _bot.Data.DataBase.AddMemberToTeam(
                            currentRoom,
                            currentTeam,
                            currentMember
                        );
                    }

                    _bot.SendMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: "Дальше вы можете покинуть команду, если попали не туда, вернуться к просмотру всех команд или настроить уведомления.",
                        content: (ISendable[])_studentStickers.GetRandomSticker(7, 8).GetSendableSticker(),
                        new Keyboard(new List<string> { "Настройка уведомлений", "Покинуть команду" })
                    );
                }
            );
        }

        private void SetNotTenMinutesResponse()
        {
            AddDefaultResponse(
                    (Message mes) => mes.Text == "За 10 минут",
                    (update) =>
                    {
                        var currentSession = _bot.Data.Sessions
                            .Where(session => session.Member.Name == update.Message.Chat.Id.ToString())
                            .FirstOrDefault();

                        currentSession.Member.SetNotificationType(NotificationType.TEN_MINUTES);
                        _bot.Data.DataBase.WriteRoom(currentSession.Room);

                        _bot.SendMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: "Настройки зафиксированы. Ожидайте оповещения.",
                            content: (ISendable[])_studentStickers.GetRandomSticker(13, 14).GetSendableSticker(),
                            keyboard: new Keyboard(new List<string> { "За 10 минут", "За две команды", "Автоматически", "Назад" })
                            );
                    }
                );
        }

        private void SetNotAutoRepsonse()
        {
            AddDefaultResponse(
                (Message mes) => mes.Text == "Автоматически",
                (update) =>
                {
                    var currentSession = _bot.Data.Sessions
                        .Where(session => session.Member.Name == update.Message.Chat.Id.ToString())
                        .First();
                    currentSession.Member.SetNotificationType(NotificationType.AUTO);
                    _bot.Data.DataBase.WriteRoom(currentSession.Room);

                    _bot.SendMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: "Высчитывается среднее арифметическое времени выступлений всех команд, которые уже выступили, и уведомление приходит за рассчитанное время до вашего выступления.",
                                content: (ISendable[])_studentStickers.GetRandomSticker(11, 12).GetSendableSticker(),
                                keyboard: new Keyboard(new List<string> { "За 10 минут", "За две команды", "Автоматически", "Назад" })
                            );

                    _bot.SendMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: "Настройки зафиксированы. Ожидайте оповещения.",
                                content: (ISendable[])_studentStickers.GetRandomSticker(13, 14).GetSendableSticker(),
                                keyboard: new Keyboard(new List<string> { "За 10 минут", "За две команды", "Автоматически", "Назад" })
                            );
                }
            );
        }

        private void SetNotTwoTeamsResponse()
        {
            AddDefaultResponse(
                (Message mes) => mes.Text == "За две команды",
                (update) =>
                {
                    var currentSession = _bot.Data.Sessions.Where(session => session.Member.Name == update.Message.Chat.Id.ToString()).First();
                    currentSession.Member.SetNotificationType(NotificationType.TWO_TEAMS);
                    _bot.Data.DataBase.WriteRoom(currentSession.Room);

                    _bot.SendMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: "Настройки зафиксированы. Ожидайте оповещения.",
                        content: (ISendable[])_studentStickers.GetRandomSticker(13, 14).GetSendableSticker(),
                        keyboard: new Keyboard(new List<string> { "За 10 минут", "За две команды", "Автоматически", "Назад" })
                    );
                }
            );
        }

        private void NotSettingsResponse()
        {
            AddDefaultResponse(
                    (Message mes) => mes.Text == "Настройка уведомлений",
                    (Update update) =>
                    {
                        _bot.SendMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: "Укажите, когда мне уведомить о вашем выступлении.",
                                content: (ISendable[])_studentStickers.GetRandomSticker(9, 10).GetSendableSticker(),
                                keyboard: new Keyboard(new List<string> { "За 10 минут", "За две команды", "Автоматически", "Назад" })
                            );
                    }
                );
        }

        private void DisconnectFromTeamResponse()
        {
            AddDefaultResponse(
                (Message mes) => mes.Text == "Покинуть команду",
                (update) =>
                {
                    var currentSession = _bot.Data.Sessions.Where(session => session.Member.Name == update.Message.Chat.Id.ToString()).First();
                    currentSession.Member.OnNotifyCalled -= () => _bot.SendMessageAsync(update.Message.Chat.Id, "Подключайтесь!");
                    _bot.Data.DataBase.RemoveMemberFromTeam(
                        currentSession.Room,
                        currentSession.Team,
                        currentSession.Member
                    );

                    _bot.SendMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: "Disconnected"
                    );
                }
            );
        }
        #endregion
    }

    public static class StickerSetExtensions
    {
        public static Telegram.Bot.Types.Sticker GetRandomSticker(this StickerSet set, params int[] indexes)
        {
            Random r = new();
            int i = r.Next(0, indexes.Length);

            return set.Stickers[indexes[i] - 1];
        }
    }

    public static class StickerExtensions
    {
        public static SendableFiles.Sticker GetSendableSticker(this Telegram.Bot.Types.Sticker sticker)
        {
            return new SendableFiles.Sticker(sticker.FileId);
        }
    }
}
