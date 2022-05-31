using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Bots;
using Bots.UpdateHandlers;
using TableParser;

// TODO: Parse rooms from DB after starting
// TODO: Refactoring
// TODO: Протестировать перекрёстный огонь
// TODO: Add resources in build

namespace BotBuilder
{
    enum SYSTEM_COMMANDS
    {
        PARSE_ROOMS_FROM_TABLE,
        CONNECT_TO_ROOM,
        NOTIFY_ALL
    }

    public class Builder
    {
        private const string src = "../../../../src/";

        private IBot _bot;

        public void Build(IBot bot)
        {
            _bot = bot;

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

        private Room GetRoomByChatId(long id)
        {
            return _bot.Data.Sessions
                .Where(session => session.Member.Name == id.ToString())
                .FirstOrDefault()
                .Room;
        }

        private void AddDefaultResponse(Response.FilterFunc filter, Response.ResponseFunc response)
        {
            //Response r = _bot.GetResponse(filter, response);
            //_bot.UpdateHandler.AddResponse(r);

            _bot.AddResponse(filter, response);
        }

        private void AddStartResponse()
        {
            AddDefaultResponse(
                    (InputMessage im) => im.Text == "/start",
                    (InputMessage im) =>
                    {
                        _bot.SendMessageAsync(
                                im.SenderId,
                                ReadLineFromFile(src + "startWelcome.txt"),
                                _bot.GetKeyboard("Студент", "Эксперт")
                            );
                    }
                );
        }

        private void ConfigureCommands()
        {
            AddDefaultResponse(
                    (InputMessage im) =>
                    {
                        return !int.TryParse(im.Text, out _) &&
                            Enum.TryParse(typeof(SYSTEM_COMMANDS), im.Text, false, out var com) &&
                            (SYSTEM_COMMANDS)com == SYSTEM_COMMANDS.PARSE_ROOMS_FROM_TABLE;
                    },
                    (InputMessage im) =>
                    {
                        _bot.UpdateHandler.AddQuery(im.SenderId, ParseTableResponseFunc);
                    }
                );

            AddDefaultResponse(
                    (InputMessage im) =>
                    {
                        return !int.TryParse(im.Text, out _) &&
                            Enum.TryParse(typeof(SYSTEM_COMMANDS), im.Text, false, out var com) &&
                            (SYSTEM_COMMANDS)com == SYSTEM_COMMANDS.CONNECT_TO_ROOM;
                    },
                    (InputMessage im) =>
                    {
                        _bot.UpdateHandler.AddQuery(im.SenderId, ConnectToRoomResponseFunc);
                    }
                );

            AddDefaultResponse(
                    (InputMessage im) =>
                    {
                        return !int.TryParse(im.Text, out _) &&
                            Enum.TryParse(typeof(SYSTEM_COMMANDS), im.Text, false, out var com) &&
                            (SYSTEM_COMMANDS)com == SYSTEM_COMMANDS.NOTIFY_ALL;
                    },
                    (InputMessage im) =>
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
                    (InputMessage im) => im.Text == "Эксперт",
                    (InputMessage im) =>
                    {
                        _bot.SendMessageAsync(
                                im.SenderId,
                                ReadLineFromFile(src + "expertWelcome.txt"),
                                _bot.GetKeyboard("Получить коды комнат", "Шаблон гугл таблицы")
                            );

                        _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(1, 2));
                    }
                );
        }

        // COMPLETE
        // TODO: Order files
        private void AddGoogleSheetPattern()
        {
            AddDefaultResponse(
                    (InputMessage im) => im.Text == "Шаблон гугл таблицы",
                    (InputMessage im) =>
                    {
                        _bot.SendMessageAsync(
                                im.SenderId,
                                ReadLineFromFile(src + "exampleAlert.txt"),
                                _bot.GetKeyboard("Получить коды комнат", "Шаблон гугл таблицы")
                            );

                        _bot.SendStickerAsync(im.SenderId, "expert", (uint)GetRandomIndex(5, 6));

                        _bot.SendDocumentAsync(im.SenderId, src + "Описание шаблона.xlsx", "Описание шаблона");
                        _bot.SendPhotoAsync(im.SenderId, src + "Пример шаблона.jpg", "Пример шаблона");
                        _bot.SendDocumentAsync(im.SenderId, src + "Пример шаблона.xlsx", "Пример шаблона");
                    }
                );
        }

        // COMPLETE
        private void AddGoogleSheetQuery()
        {
            AddDefaultResponse(
                    (InputMessage im) => im.Text == "Получить коды комнат",
                    (InputMessage im) =>
                    {
                        // start new iteration
                        _bot.InvokeMessage(im.SenderId, SYSTEM_COMMANDS.PARSE_ROOMS_FROM_TABLE.ToString());

                        _bot.SendMessageAsync(
                                im.SenderId,
                                "Отправьте сюда ссылку на Google-таблицу.",
                                keyboard: _bot.GetKeyboard("back")
                            );

                        _bot.SendStickerAsync(im.SenderId, "expert", (uint)GetRandomIndex(2, 3));
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
            catch (Exception e)
            {
                Console.WriteLine($"=== ERROR ===\n{e.Message}\n=== ERROR ===");
            }
            finally
            {
                outRooms = res.ToArray();
            }

            return isSuccess;
        }

        // COMPLETE
        void ParseTableResponseFunc(InputMessage im)
        {
            if (im.Text == "back")
            {
                _bot.InvokeMessage(im.SenderId, "/start");
                return;
            }

            if (TryParseRooms(im.Text, out Room[] newRooms))
            {
                _bot.SendMessageAsync(
                        chatId: im.SenderId,
                        text: "Коды комнат:\n" + string.Join("\n", newRooms.Select(room => $"\t[{room.StartTime:g}] {room.Name} - {room.Link}"))
                        );

                _bot.SendMessageAsync(
                            chatId: im.SenderId,
                            text: "Дальше я сам буду брать всю информацию с вашей таблицы. (Стикер)"
                        );

                _bot.SendStickerAsync(im.SenderId, "expert", (uint)GetRandomIndex(4, 7));
            }
            else
            {
                // start new iteration
                _bot.InvokeMessage(im.SenderId, SYSTEM_COMMANDS.PARSE_ROOMS_FROM_TABLE.ToString());

                _bot.SendMessageAsync(
                    chatId: im.SenderId,
                    text: "Вы отправили неправильную ссылку. Попробуйте снова.",
                    keyboard: _bot.GetKeyboard("back")
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
                (InputMessage im) => im.Text == "Студент" || im.Text == "Покинуть команду",
                (InputMessage im) =>
                {
                    if (im.Text == "Покинуть команду")
                    {
                        _bot.InvokeMessage(im.SenderId, "Посмотреть команды");
                    }

                    var currentSession = _bot.Data.Sessions
                        .Where(s => s.Member.Name == im.SenderId.ToString())
                        .FirstOrDefault();

                    if (currentSession.Member == null)
                    {
                        _bot.SendMessageAsync(
                            chatId: im.SenderId,
                            text: ReadLineFromFile(src + "studentWelcome.txt"),
                            keyboard: _bot.GetKeyboard("Подключиться к комнате")
                            );

                        _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(1, 2));
                    }
                    else
                    {
                        _bot.InvokeMessage(im.SenderId, "Назад");
                    }
                }
            );
        }

        // COMPLETE
        private void AddConnectQuery()
        {
            AddDefaultResponse(
                    (InputMessage im) => im.Text == "Подключиться к комнате",
                    (InputMessage im) =>
                    {
                        _bot.InvokeMessage(im.SenderId, SYSTEM_COMMANDS.CONNECT_TO_ROOM.ToString());

                        _bot.SendMessageAsync(
                            chatId: im.SenderId,
                            text: "Введите, пожалуйста, код, который вам выдали ваши эксперты.",
                            keyboard: _bot.GetKeyboard("back")
                            );

                        _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(3, 4));
                    }
                );
        }

        private void ConnectToRoomResponseFunc(InputMessage im)
        {
            var commandsTable = _bot.Data.DataBase.Table;
            var rooms = commandsTable.GetAllSheets();
            var roomsLinks = rooms.Select(sheet => sheet.Name);

            if (roomsLinks.Contains(im.Text))
            {
                string currentRoomLink = im.Text;

                // TODO: Parse rooms from Linker sheet
                Room currentRoom = _bot.Data.DataBase.GetRooms()
                    .Where(room => room.Link == currentRoomLink).FirstOrDefault();

                var member = new Member(im.SenderId.ToString());

                member.OnNotifyCalled += () => _bot.SendMessageAsync(im.SenderId, "Подключайтесь!");

                _bot.Data.Sessions.Add(new Session { Member = member, Room = currentRoom, Team = null });

                _bot.SendMessageAsync(
                        chatId: im.SenderId,
                        text: "Тут вы можете посмотреть все существующие команды в данной комнате и выбрать нужную вам.",
                        _bot.GetKeyboard("Посмотреть команды")
                    );

                _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(5, 6));
            }
            else if (im.Text == "back")
            {
                _bot.InvokeMessage(im.SenderId, "/start");
            }
            else
            {
                _bot.InvokeMessage(im.SenderId, SYSTEM_COMMANDS.CONNECT_TO_ROOM.ToString());

                _bot.SendMessageAsync(
                    chatId: im.SenderId,
                    text: "Такой комнаты нет. Попробуйте ещё раз.",
                    keyboard: _bot.GetKeyboard("back")
                    );
            }
        }

        // TODO: Check
        private void GetTeamsResponse()
        {
            AddDefaultResponse(
                (InputMessage im) => im.Text == "Посмотреть команды",
                (InputMessage im) =>
                {
                    Room currentRoom = GetRoomByChatId(im.SenderId);
                    List<Team> teams = currentRoom.Teams;

                    string t = string.Join('\n', teams.Select(team => string.Format("{0} - {1}", teams.IndexOf(team) + 1, team.Name)));

                    _bot.SendMessageAsync(
                        chatId: im.SenderId,
                        text: t
                    );
                }
            );
        }

        // TODO: Remake as query
        private void ConnectToTeamResponse()
        {
            AddDefaultResponse(
                (InputMessage im) =>
                {
                    Room currentRoom = GetRoomByChatId(im.SenderId);
                    return int.TryParse(im.Text, out int num) && num >= 1 && num < currentRoom.Teams.Count + 1
                        || im.Text == "Назад";
                },
                (InputMessage im) =>
                {
                    if (im.Text != "Назад")
                    {
                        var currentSession = _bot.Data.Sessions
                            .Where(session => session.Member.Name == im.SenderId.ToString())
                            .FirstOrDefault();


                        var currentSessionIndex = _bot.Data.Sessions.IndexOf(currentSession);

                        Member currentMember = currentSession.Member;
                        Room currentRoom = currentSession.Room;

                        int teamId = int.Parse(im.Text) - 1;

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
                        chatId: im.SenderId,
                        text: "Дальше вы можете покинуть команду, если попали не туда, вернуться к просмотру всех команд или настроить уведомления.",
                        _bot.GetKeyboard("Настройка уведомлений", "Покинуть команду")
                    );

                    _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(7, 8));
                }
            );
        }

        private void SetNotTenMinutesResponse()
        {
            AddDefaultResponse(
                    (InputMessage im) => im.Text == "За 10 минут",
                    (InputMessage im) =>
                    {
                        var currentSession = _bot.Data.Sessions
                            .Where(session => session.Member.Name == im.SenderId.ToString())
                            .FirstOrDefault();

                        currentSession.Member.SetNotificationType(NotificationType.TEN_MINUTES);
                        _bot.Data.DataBase.WriteRoom(currentSession.Room);

                        _bot.SendMessageAsync(
                            chatId: im.SenderId,
                            text: "Настройки зафиксированы. Ожидайте оповещения.",
                            keyboard: _bot.GetKeyboard("За 10 минут", "За две команды", "Автоматически", "Назад")
                            );

                        _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(13, 14));
                    }
                );
        }

        private void SetNotAutoRepsonse()
        {
            AddDefaultResponse(
                (InputMessage im) => im.Text == "Автоматически",
                (InputMessage im) =>
                {
                    var currentSession = _bot.Data.Sessions
                        .Where(session => session.Member.Name == im.SenderId.ToString())
                        .First();
                    currentSession.Member.SetNotificationType(NotificationType.AUTO);
                    _bot.Data.DataBase.WriteRoom(currentSession.Room);

                    _bot.SendMessageAsync(
                                chatId: im.SenderId,
                                text: "Высчитывается среднее арифметическое времени выступлений всех команд, которые уже выступили, и уведомление приходит за рассчитанное время до вашего выступления.",
                                keyboard: _bot.GetKeyboard("За 10 минут", "За две команды", "Автоматически", "Назад")
                            );

                    _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(11, 12));

                    _bot.SendMessageAsync(
                                chatId: im.SenderId,
                                text: "Настройки зафиксированы. Ожидайте оповещения.",
                                keyboard: _bot.GetKeyboard("За 10 минут", "За две команды", "Автоматически", "Назад")
                            );

                    _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(13, 14));
                }
            );
        }

        private void SetNotTwoTeamsResponse()
        {
            AddDefaultResponse(
                (InputMessage im) => im.Text == "За две команды",
                (InputMessage im) =>
                {
                    var currentSession = _bot.Data.Sessions.Where(session => session.Member.Name == im.SenderId.ToString()).First();
                    currentSession.Member.SetNotificationType(NotificationType.TWO_TEAMS);
                    _bot.Data.DataBase.WriteRoom(currentSession.Room);

                    _bot.SendMessageAsync(
                        chatId: im.SenderId,
                        text: "Настройки зафиксированы. Ожидайте оповещения.",
                        keyboard: _bot.GetKeyboard("За 10 минут", "За две команды", "Автоматически", "Назад")
                    );

                    _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(13, 14));
                }
            );
        }

        private void NotSettingsResponse()
        {
            AddDefaultResponse(
                    (InputMessage im) => im.Text == "Настройка уведомлений",
                    (InputMessage im) =>
                    {
                        _bot.SendMessageAsync(
                                chatId: im.SenderId,
                                text: "Укажите, когда мне уведомить о вашем выступлении.",
                                keyboard: _bot.GetKeyboard("За 10 минут", "За две команды", "Автоматически", "Назад")
                            );

                        _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(9, 10));
                    }
                );
        }

        private void DisconnectFromTeamResponse()
        {
            AddDefaultResponse(
                (InputMessage im) => im.Text == "Покинуть команду",
                (InputMessage im) =>
                {
                    var currentSession = _bot.Data.Sessions.Where(session => session.Member.Name == im.SenderId.ToString()).First();
                    currentSession.Member.OnNotifyCalled -= () => _bot.SendMessageAsync(im.SenderId, "Подключайтесь!");
                    _bot.Data.DataBase.RemoveMemberFromTeam(
                        currentSession.Room,
                        currentSession.Team,
                        currentSession.Member
                    );

                    _bot.SendMessageAsync(
                        chatId: im.SenderId,
                        text: "Disconnected"
                    );
                }
            );
        }
        #endregion

        private int GetRandomIndex(params int[] indexes)
        {
            Random r = new Random();
            int i = r.Next(0, indexes.Length);

            return indexes[i];
        }
    }
}
