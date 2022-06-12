using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Bots;
using Bots.UpdateHandlers;
using TableParser;
using Requests;

// TODO: Parse rooms from DB after starting
// TODO: Refactoring
// TODO: Протестировать перекрёстный огонь

namespace BotBuilder
{
    enum SYSTEM_COMMANDS
    {
        PARSE_ROOMS_FROM_TABLE,
        PK_CONNECT,
        PK_GET_LOGIN,
        PK_GET_PASSWORD,
        NOTIFY_ALL
    }

    public class Builder
    {
        // TODO : Create path to data-folder in BotData
        // After : Get src and .private_cfg folders with Data.PathToData + "src/"...
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


        // TODO : Parse sessions
        private Room GetRoomByChatId(long id)
        {
            return _bot.Data.Sessions
                .Where(session => session.Member.Name == id.ToString())
                .FirstOrDefault()
                .Room;
        }

        private void AddResponse(Response.FilterFunc filter, Response.ResponseFunc response)
        {
            _bot.AddResponse(filter, response);
        }

        private void AddStartResponse()
        {
            AddResponse(
                    (InputMessage im) => im.Text == "/start" || im.Text == "Начать",
                    (InputMessage im) =>
                    {
                        _bot.SendMessageAsync(
                                im.SenderId,
                                ReadLineFromFile(src + "startWelcome.txt"),
                                _bot.GetKeyboard("Студент", "Организатор защит")
                            );
                    }
                );
        }

        private void ConfigureCommands()
        {
            AddResponse(
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

            AddResponse(
                    (InputMessage im) =>
                    {
                        return !int.TryParse(im.Text, out _) &&
                            Enum.TryParse(typeof(SYSTEM_COMMANDS), im.Text, false, out var com) &&
                            (SYSTEM_COMMANDS)com == SYSTEM_COMMANDS.PK_CONNECT;
                    },
                    (InputMessage im) =>
                    {
                        _bot.UpdateHandler.AddQuery(im.SenderId, ConnectToPKResponseFunc);
                    }
                );

            AddResponse(
                    (InputMessage im) =>
                    {
                        return !int.TryParse(im.Text, out _) &&
                            Enum.TryParse(typeof(SYSTEM_COMMANDS), im.Text, false, out var com) &&
                            (SYSTEM_COMMANDS)com == SYSTEM_COMMANDS.PK_GET_LOGIN;
                    },
                    (InputMessage im) =>
                    {
                        _bot.UpdateHandler.AddQuery(im.SenderId, GetLoginResponseFunc);
                    }
                );

            AddResponse(
                    (InputMessage im) =>
                    {
                        return !int.TryParse(im.Text, out _) &&
                            Enum.TryParse(typeof(SYSTEM_COMMANDS), im.Text, false, out var com) &&
                            (SYSTEM_COMMANDS)com == SYSTEM_COMMANDS.PK_GET_PASSWORD;
                    },
                    (InputMessage im) =>
                    {
                        _bot.UpdateHandler.AddQuery(im.SenderId, GetPassResponseFunc);
                    }
                );

            AddResponse(
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
            AddResponse(
                    (InputMessage im) => im.Text == "Организатор защит",
                    (InputMessage im) =>
                    {
                        _bot.SendMessageAsync(
                                im.SenderId,
                                ReadLineFromFile(src + "expertWelcome.txt"),
                                _bot.GetKeyboard("Отправить Google-Таблицу", "Шаблон гугл таблицы")
                            );

                        _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(1, 2));
                    }
                );
        }

        // COMPLETE
        // TODO: Order files
        private void AddGoogleSheetPattern()
        {
            AddResponse(
                    (InputMessage im) => im.Text == "Шаблон гугл таблицы",
                    (InputMessage im) =>
                    {
                        _bot.SendMessageAsync(
                                im.SenderId,
                                ReadLineFromFile(src + "exampleAlert.txt"),
                                _bot.GetKeyboard("Отправить Google-Таблицу", "Шаблон гугл таблицы")
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
            AddResponse(
                    (InputMessage im) => im.Text == "Отправить Google-Таблицу",
                    (InputMessage im) =>
                    {
                        // start new iteration
                        _bot.InvokeMessage(im.SenderId, SYSTEM_COMMANDS.PARSE_ROOMS_FROM_TABLE.ToString());

                        _bot.SendMessageAsync(
                                im.SenderId,
                                "Отправьте сюда ссылку на Google-таблицу.",
                                keyboard: _bot.GetKeyboard("Вернуться")
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
            if (im.Text == "Вернуться")
            {
                _bot.InvokeMessage(im.SenderId, "/start");
                return;
            }

            _bot.SendMessageAsync(
                        chatId: im.SenderId,
                        text: "Пробуем получить комнаты..."
                        );

            if (TryParseRooms(im.Text, out Room[] newRooms))
            {
                // TODO : Запускать асинхронно блок из Send[STH](), чтобы сохранялся порядок
                // TODO : Добавить боту метод SendSticker(..., StickerId, ...), где StickerId — не номер, а перечисление или название, чтобы
                // стикер выбирася самим ботом

                _bot.SendMessageAsync(
                        chatId: im.SenderId,
                        text: "Дальше я сам буду брать всю информацию с вашей таблицы.",
                        _bot.GetKeyboard("Студент", "Организатор защит")
                    );

                _bot.SendStickerAsync(im.SenderId, "expert", (uint)GetRandomIndex(4, 7));
            }
            else
            {
                _bot.InvokeMessage(im.SenderId, SYSTEM_COMMANDS.PARSE_ROOMS_FROM_TABLE.ToString());

                _bot.SendMessageAsync(
                    chatId: im.SenderId,
                    text: "Вы отправили неправильную ссылку. Попробуйте снова.",
                    keyboard: _bot.GetKeyboard("Вернуться")
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

        private void AddStudentWelcomeResponse()
        {
            AddResponse(
                (InputMessage im) => im.Text == "Студент",
                (InputMessage im) =>
                {
                    var currentSession = _bot.Data.Sessions
                        .Where(s => s.Member.Name == im.SenderId.ToString())
                        .FirstOrDefault();

                    if (currentSession.Member == null)
                    {
                        _bot.SendMessageAsync(
                            chatId: im.SenderId,
                            text: ReadLineFromFile(src + "studentWelcome.txt"),
                            keyboard: _bot.GetKeyboard("Подключиться к ПроКомпетенции")
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

        private void AddConnectQuery()
        {
            AddResponse(
                    (InputMessage im) => im.Text == "Подключиться к ПроКомпетенции",
                    (InputMessage im) =>
                    {
                        _bot.InvokeMessage(im.SenderId, SYSTEM_COMMANDS.PK_GET_LOGIN.ToString());

                        _bot.SendMessageAsync(
                            chatId: im.SenderId,
                            text: "Введите, пожалуйста, логин от ПроКомпетенции",
                            keyboard: _bot.GetKeyboard("Вернуться")
                            );

                        _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(3, 4));
                    }
                );
        }

        public struct PK_INFO
        {
            public long UserId;
            public string Login;
            public string Pass;
        }

        private readonly HashSet<PK_INFO> _pkInfos = new HashSet<PK_INFO>();

        private void GetLoginResponseFunc(InputMessage im)
        {
            if (im.Text == "Вернуться")
            {
                _bot.InvokeMessage(im.SenderId, "/start");
                return;
            }

            _pkInfos.Add(new PK_INFO { 
                UserId = im.SenderId,
                Login = im.Text,
                Pass = ""
            });

            _bot.InvokeMessage(im.SenderId, SYSTEM_COMMANDS.PK_GET_PASSWORD.ToString());

            _bot.SendMessageAsync(
                chatId: im.SenderId,
                text: "Введите, пожалуйста, пароль от ПроКомпетенции",
                keyboard: _bot.GetKeyboard("Вернуться")
                );
        }

        private void GetPassResponseFunc(InputMessage im)
        {
            if (im.Text == "Вернуться")
            {
                _bot.InvokeMessage(im.SenderId, "/start");
                return;
            }

            var i = _pkInfos.First(i => i.UserId == im.SenderId);
            i.Pass = im.Text;

            _bot.SendMessageAsync(
                chatId: im.SenderId,
                text: "Пробуем подключиться к ПроКомпетенции...",
                keyboard: _bot.GetKeyboard("Вернуться")
                );

            ConnectToPKResponseFunc(im);
        }

        private void ConnectToPKResponseFunc(InputMessage im)
        {
            var i = _pkInfos.First(i => i.UserId == im.SenderId);

            string sid;
            Requests.Team team = new Requests.Team
            {
                Name = "",
                Students = null
            };

            try
            {
                sid = Authorize(i.Login, i.Pass);

                Console.WriteLine($"SSID : {sid}");

                RequestsManager rm = new RequestsManager(sid);
                team = rm.GetEventData();
            }
            catch
            {
                _bot.SendMessageAsync(
                        chatId: im.SenderId,
                        text: "Неправильный логин и/или пароль"
                    );

                _bot.InvokeMessage(im.SenderId, "/start");

                return;
            }

            var rooms = _bot.Data.DataBase.GetRooms();

            Room room = null;

            foreach (Room r in rooms)
            {
                var teams = r.Teams;
                if (teams.Any(t => t.Name == team.Name))
                {
                    room = r;
                    break;
                }
            }

            if (room != null)
            {
                TableParser.Team tpTeam = room.Teams.Where(t => t.Name == team.Name).FirstOrDefault();
                var test_member = new Member(im.SenderId.ToString());

                test_member.OnNotifyCalled += () => _bot.SendMessageAsync(im.SenderId, "Подключайтесь!");


                _bot.Data.Sessions.Add(new Session { Member = test_member, Room = room, Team = tpTeam });

                _bot.Data.DataBase.AddMemberToTeam(
                                room,
                                tpTeam,
                                test_member
                            );

                _bot.SendMessageAsync(
                        chatId: im.SenderId,
                        text: $"Room: {room.Name} @ {room.StartTime.ToString("dd.MM.yyyy HH:mm")}\nTeam: {tpTeam.Name}",
                        _bot.GetKeyboard("Настройка уведомлений")
                    );

                _bot.SendMessageAsync(
                            chatId: im.SenderId,
                            text: "Дальше вы можете настроить уведомления.",
                            _bot.GetKeyboard("Настройка уведомлений")
                        );

                _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(7, 8));
            }
            else
            {
                _bot.SendMessageAsync(
                        chatId: im.SenderId,
                        text: "Мы не смогли найти вашу команду в комнатах. Обратитесь к модератору."
                    );

                _bot.InvokeMessage(im.SenderId, "/start");
            }
        }

        private string Authorize(string login, string password)
        {
            string sid;
            // line = login : sid : expired_date [dd/MM/yyyy hh:mm:ss]
            foreach (string line in File.ReadAllLines("pk_sessions.txt"))
            {
                string[] info = line.Split(" : ");
                if (info.Length > 1)
                {
                    if (info[0] == login)
                    {
                        if (DateTime.Parse(info[2]) > DateTime.UtcNow)
                        {
                            return info[1];
                        }
                        else
                        {
                            sid = RequestsManager.GetSID(login, password);
                            DeletePKLine(login);
                            AddPKLine($"{login} : {sid} : {DateTime.UtcNow.AddMonths(1):G}");
                            return sid;
                        }
                    }
                    else continue;
                }
            }

            sid = RequestsManager.GetSID(login, password);
            AddPKLine($"{login} : {sid} : {DateTime.UtcNow.AddMonths(1):G}");
            return sid;
        }

        private void DeletePKLine(string login)
        {
            string path = "pk_sessions.txt";
            var oldLines = File.ReadAllLines(path);
            var newLines = oldLines.Where(line => !line.StartsWith(login));
            File.WriteAllLines(path, newLines);
            FileStream obj = new FileStream(path, FileMode.Append);
            obj.Close();
        }

        private void AddPKLine(string line)
        {
            FileInfo file = new FileInfo("pk_sessions.txt");
            using (StreamWriter sw = file.AppendText())
            {
                sw.WriteLine(line);
            }
        }

        // TODO: Check
        private void GetTeamsResponse()
        {
            AddResponse(
                (InputMessage im) => im.Text == "Посмотреть команды",
                (InputMessage im) =>
                {
                    Room currentRoom = GetRoomByChatId(im.SenderId);
                    List<TableParser.Team> teams = currentRoom.Teams;

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
            AddResponse(
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

                        List<TableParser.Team> teams = currentRoom.Teams;

                        TableParser.Team currentTeam = teams[teamId];

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
                        text: "Дальше вы можете настроить уведомления.",
                        _bot.GetKeyboard("Настройка уведомлений")
                    );

                    _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(7, 8));
                }
            );
        }

        private void SetNotTenMinutesResponse()
        {
            AddResponse(
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
                            keyboard: _bot.GetKeyboard("За 10 минут", "За две команды до начала", "Автоматически", "Назад")
                            );

                        _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(13, 14));
                    }
                );
        }

        private void SetNotAutoRepsonse()
        {
            AddResponse(
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
                                keyboard: _bot.GetKeyboard("За 10 минут", "За две команды до начала", "Автоматически", "Назад")
                            );

                    _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(11, 12));

                    _bot.SendMessageAsync(
                                chatId: im.SenderId,
                                text: "Настройки зафиксированы. Ожидайте оповещения.",
                                keyboard: _bot.GetKeyboard("За 10 минут", "За две команды до начала", "Автоматически", "Назад")
                            );

                    _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(13, 14));
                }
            );
        }

        private void SetNotTwoTeamsResponse()
        {
            AddResponse(
                (InputMessage im) => im.Text == "За две команды",
                (InputMessage im) =>
                {
                    var currentSession = _bot.Data.Sessions.Where(session => session.Member.Name == im.SenderId.ToString()).First();
                    currentSession.Member.SetNotificationType(NotificationType.TWO_TEAMS);
                    _bot.Data.DataBase.WriteRoom(currentSession.Room);

                    _bot.SendMessageAsync(
                        chatId: im.SenderId,
                        text: "Настройки зафиксированы. Ожидайте оповещения.",
                        keyboard: _bot.GetKeyboard("За 10 минут", "За две команды до начала", "Автоматически", "Назад")
                    );

                    _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(13, 14));
                }
            );
        }

        private void NotSettingsResponse()
        {
            AddResponse(
                    (InputMessage im) => im.Text == "Настройка уведомлений",
                    (InputMessage im) =>
                    {
                        _bot.SendMessageAsync(
                                chatId: im.SenderId,
                                text: "Укажите, когда мне уведомить о вашем выступлении.",
                                keyboard: _bot.GetKeyboard("За 10 минут", "За две команды до начала", "Автоматически", "Назад")
                            );

                        _bot.SendStickerAsync(im.SenderId, "student", (uint)GetRandomIndex(9, 10));
                    }
                );
        }

        
        private void DisconnectFromTeamResponse()
        {
            AddResponse(
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
